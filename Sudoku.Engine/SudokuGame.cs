using System.Diagnostics;

namespace Sudoku.Engine;

/// <summary>
/// Главный класс игровой логики Судоку. Публичный API, который дёргает UI.
/// </summary>
public sealed class SudokuGame : IDisposable
{
    private const int InitialLives = 3;
    private const int InitialHints = 3;

    private Board _board;
    private int[,] _solution;
    private readonly Stopwatch _stopwatch = new();
    private System.Threading.Timer? _tickTimer;
    private TimeSpan _pausedAt;
    private bool _disposed;

    public Difficulty Difficulty { get; private set; }
    public bool IsSolved { get; private set; }
    public bool IsLost { get; private set; }
    public int LivesRemaining { get; private set; }
    public int HintsRemaining { get; private set; }
    public bool IsPaused { get; private set; }

    /// <summary>Время с начала партии. На паузе не растёт.</summary>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>Раз в секунду — UI должен обновить таймер.</summary>
    public event EventHandler? Tick;
    public event EventHandler<MoveAppliedEventArgs>? MoveApplied;
    public event EventHandler? GameWon;
    public event EventHandler? GameLost;

    public SudokuGame(Difficulty difficulty, int? seed = null)
    {
        (_board, _solution) = GenerateNew(difficulty, seed);
        Difficulty = difficulty;
        LivesRemaining = InitialLives;
        HintsRemaining = InitialHints;
        StartTimer();
    }

    private static (Board, int[,]) GenerateNew(Difficulty difficulty, int? seed)
    {
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();
        return BoardGenerator.Generate(difficulty, rng);
    }

    /// <summary>Сбрасывает текущий паззл в начальное состояние, не генерируя новый.</summary>
    public void Restart()
    {
        // «Размораживаем» доску: оставляем только заданные клетки, остальные обнуляем.
        for (int r = 0; r < Board.Size; r++)
            for (int c = 0; c < Board.Size; c++)
                if (!_board.IsGiven(r, c))
                    _board.Set(r, c, 0);

        LivesRemaining = InitialLives;
        HintsRemaining = InitialHints;
        IsSolved = false;
        IsLost = false;
        _stopwatch.Reset();
        StartTimer();
    }

    /// <summary>Начинает новую партию с другим паззлом.</summary>
    public void NewGame(Difficulty difficulty, int? seed = null)
    {
        (_board, _solution) = GenerateNew(difficulty, seed);
        Difficulty = difficulty;
        LivesRemaining = InitialLives;
        HintsRemaining = InitialHints;
        IsSolved = false;
        IsLost = false;
        IsPaused = false;
        _stopwatch.Reset();
        StartTimer();
    }

    public CellInfo GetCell(int row, int col)
    {
        var value = _board.Get(row, col);
        var given = _board.IsGiven(row, col);
        var conflict = !given && value != 0 && value != _solution[row, col];
        return new CellInfo(row, col, value, given, conflict);
    }

    public IEnumerable<CellInfo> GetAllCells()
    {
        for (int r = 0; r < Board.Size; r++)
            for (int c = 0; c < Board.Size; c++)
                yield return GetCell(r, c);
    }

    public MoveResult TryPlace(int row, int col, int value)
    {
        if (row is < 0 or > 8 || col is < 0 or > 8 || value is < 0 or > 9)
            return MoveResult.OutOfRange;
        if (IsSolved || IsLost)
            return MoveResult.OutOfRange;
        if (_board.IsGiven(row, col))
            return MoveResult.GivenCell;

        if (value == 0)
        {
            _board.Set(row, col, 0);
            RaiseMove(row, col, 0, MoveResult.Cleared);
            return MoveResult.Cleared;
        }

        _board.Set(row, col, value);

        if (value != _solution[row, col])
        {
            LivesRemaining--;
            RaiseMove(row, col, value, MoveResult.Conflict);
            if (LivesRemaining <= 0)
            {
                IsLost = true;
                StopTimer();
                GameLost?.Invoke(this, EventArgs.Empty);
            }
            return MoveResult.Conflict;
        }

        // Правильная цифра. Проверяем, не последняя ли это.
        if (IsBoardComplete())
        {
            IsSolved = true;
            StopTimer();
            RaiseMove(row, col, value, MoveResult.Solved);
            GameWon?.Invoke(this, EventArgs.Empty);
            return MoveResult.Solved;
        }

        RaiseMove(row, col, value, MoveResult.Accepted);
        return MoveResult.Accepted;
    }

    public (int row, int col, int value)? UseHint()
    {
        if (IsSolved || IsLost || HintsRemaining <= 0) return null;

        // Собираем пустые клетки (или с неверным значением — подсказка их исправляет).
        var candidates = new List<(int r, int c)>();
        for (int r = 0; r < Board.Size; r++)
            for (int c = 0; c < Board.Size; c++)
            {
                if (_board.IsGiven(r, c)) continue;
                var v = _board.Get(r, c);
                if (v == 0 || v != _solution[r, c]) candidates.Add((r, c));
            }

        if (candidates.Count == 0) return null;
        var (row, col) = candidates[Random.Shared.Next(candidates.Count)];
        var value = _solution[row, col];

        HintsRemaining--;
        _board.Set(row, col, value);

        if (IsBoardComplete())
        {
            IsSolved = true;
            StopTimer();
            RaiseMove(row, col, value, MoveResult.Solved);
            GameWon?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            RaiseMove(row, col, value, MoveResult.Accepted);
        }

        return (row, col, value);
    }

    public void Pause()
    {
        if (IsPaused || IsSolved || IsLost) return;
        IsPaused = true;
        _pausedAt = _stopwatch.Elapsed;
        _stopwatch.Stop();
        DisposeTimer();
    }

    public void Resume()
    {
        if (!IsPaused || IsSolved || IsLost) return;
        IsPaused = false;
        _stopwatch.Start();
        StartTimerOnly();
    }

    private bool IsBoardComplete()
    {
        for (int r = 0; r < Board.Size; r++)
            for (int c = 0; c < Board.Size; c++)
                if (_board.Get(r, c) != _solution[r, c]) return false;
        return true;
    }

    private void RaiseMove(int row, int col, int value, MoveResult result)
        => MoveApplied?.Invoke(this, new MoveAppliedEventArgs
        {
            Row = row, Col = col, Value = value, Result = result,
        });

    private void StartTimer()
    {
        _stopwatch.Start();
        StartTimerOnly();
    }

    private void StartTimerOnly()
    {
        DisposeTimer();
        _tickTimer = new System.Threading.Timer(
            _ => Tick?.Invoke(this, EventArgs.Empty),
            state: null,
            dueTime: TimeSpan.FromSeconds(1),
            period: TimeSpan.FromSeconds(1));
    }

    private void StopTimer()
    {
        _stopwatch.Stop();
        DisposeTimer();
    }

    private void DisposeTimer()
    {
        _tickTimer?.Dispose();
        _tickTimer = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _stopwatch.Stop();
        DisposeTimer();
    }
}
