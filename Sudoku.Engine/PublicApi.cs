namespace Sudoku.Engine;

public enum Difficulty { Easy, Medium, Hard, Expert }

public enum MoveResult
{
    Accepted,   // число поставлено, конфликтов нет
    Conflict,   // создало конфликт → списывается жизнь
    GivenCell,  // нельзя менять — исходная клетка паззла
    OutOfRange, // value/координаты вне диапазона
    Cleared,    // value=0, клетка очищена
    Solved,     // ход завершил партию
}

public readonly record struct CellInfo(int Row, int Col, int Value, bool IsGiven, bool HasConflict);

public sealed class SudokuGame
{
    public Difficulty Difficulty { get; }
    public bool IsSolved { get; }
    public bool IsLost { get; }
    public int LivesRemaining { get; }
    public int HintsRemaining { get; }
    public TimeSpan Elapsed { get; }

    public event EventHandler? Tick;      // раз в секунду — UI обновляет таймер
    public event EventHandler? GameWon;
    public event EventHandler? GameLost;

    public SudokuGame(Difficulty difficulty, int? seed = null);
    public CellInfo GetCell(int row, int col);
    public IEnumerable<CellInfo> GetAllCells();
    public MoveResult TryPlace(int row, int col, int value);  // value=0 = очистить
    public (int row, int col, int value)? UseHint();
    public void NewGame(Difficulty difficulty, int? seed = null);
}