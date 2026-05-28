namespace Sudoku.Engine.Tests;

[TestFixture]
public class SudokuGameTests
{
    private SudokuGame _game = null!;

    [SetUp]
    public void SetUp()
    {
        _game = new SudokuGame(Difficulty.Easy, seed: 100);
    }

    [TearDown]
    public void TearDown() => _game.Dispose();

    [Test]
    public void NewGame_StartsWithThreeLivesAndThreeHints()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_game.LivesRemaining, Is.EqualTo(3));
            Assert.That(_game.HintsRemaining, Is.EqualTo(3));
            Assert.That(_game.IsSolved, Is.False);
            Assert.That(_game.IsLost, Is.False);
        });
    }

    [Test]
    public void TryPlace_OnGivenCell_ReturnsGivenCell_AndDoesNotChangeLives()
    {
        // Найдём заданную клетку.
        var given = _game.GetAllCells().First(c => c.IsGiven);
        var result = _game.TryPlace(given.Row, given.Col, given.Value == 9 ? 1 : 9);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(MoveResult.GivenCell));
            Assert.That(_game.LivesRemaining, Is.EqualTo(3));
        });
    }

    [Test]
    public void TryPlace_OutOfRange_ReturnsOutOfRange()
    {
        Assert.That(_game.TryPlace(-1, 0, 5), Is.EqualTo(MoveResult.OutOfRange));
        Assert.That(_game.TryPlace(0, 0, 99), Is.EqualTo(MoveResult.OutOfRange));
    }

    [Test]
    public void TryPlace_WrongValue_ReturnsConflict_AndDecrementsLives()
    {
        // Найдём пустую клетку и поставим заведомо неверное значение.
        var empty = _game.GetAllCells().First(c => !c.IsGiven && c.Value == 0);
        var solution = SolveAndGet(empty.Row, empty.Col);
        var wrong = solution == 9 ? 1 : 9;

        var result = _game.TryPlace(empty.Row, empty.Col, wrong);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(MoveResult.Conflict));
            Assert.That(_game.LivesRemaining, Is.EqualTo(2));
            Assert.That(_game.GetCell(empty.Row, empty.Col).HasConflict, Is.True);
        });
    }

    [Test]
    public void TryPlace_Clear_ReturnsCleared()
    {
        var empty = _game.GetAllCells().First(c => !c.IsGiven && c.Value == 0);
        _game.TryPlace(empty.Row, empty.Col, 5);

        var result = _game.TryPlace(empty.Row, empty.Col, 0);
        Assert.That(result, Is.EqualTo(MoveResult.Cleared));
        Assert.That(_game.GetCell(empty.Row, empty.Col).Value, Is.EqualTo(0));
    }

    [Test]
    public void ThreeWrongMoves_RaisesGameLost()
    {
        var lostRaised = false;
        _game.GameLost += (_, _) => lostRaised = true;

        var emptyCells = _game.GetAllCells().Where(c => !c.IsGiven && c.Value == 0).Take(3).ToList();
        foreach (var c in emptyCells)
        {
            var sol = SolveAndGet(c.Row, c.Col);
            _game.TryPlace(c.Row, c.Col, sol == 9 ? 1 : 9);
        }

        Assert.Multiple(() =>
        {
            Assert.That(_game.IsLost, Is.True);
            Assert.That(_game.LivesRemaining, Is.EqualTo(0));
            Assert.That(lostRaised, Is.True);
        });
    }

    [Test]
    public void UseHint_DecrementsHintsAndFillsCorrectValue()
    {
        var result = _game.UseHint();
        Assert.That(result, Is.Not.Null);
        var (r, c, v) = result!.Value;

        Assert.Multiple(() =>
        {
            Assert.That(_game.HintsRemaining, Is.EqualTo(2));
            Assert.That(_game.GetCell(r, c).Value, Is.EqualTo(v));
            Assert.That(_game.GetCell(r, c).HasConflict, Is.False);
        });
    }

    [Test]
    public void SolvingAllCells_RaisesGameWon()
    {
        var wonRaised = false;
        _game.GameWon += (_, _) => wonRaised = true;

        // Заполняем все клетки решением.
        var emptyCells = _game.GetAllCells().Where(c => !c.IsGiven).ToList();
        foreach (var cell in emptyCells)
        {
            var value = SolveAndGet(cell.Row, cell.Col);
            _game.TryPlace(cell.Row, cell.Col, value);
        }

        Assert.Multiple(() =>
        {
            Assert.That(_game.IsSolved, Is.True);
            Assert.That(wonRaised, Is.True);
        });
    }

    [Test]
    public void Restart_ResetsLivesAndHintsAndClearsUserCells()
    {
        var empty = _game.GetAllCells().First(c => !c.IsGiven && c.Value == 0);
        _game.TryPlace(empty.Row, empty.Col, 5);

        _game.Restart();

        Assert.Multiple(() =>
        {
            Assert.That(_game.LivesRemaining, Is.EqualTo(3));
            Assert.That(_game.HintsRemaining, Is.EqualTo(3));
            Assert.That(_game.GetCell(empty.Row, empty.Col).Value, Is.EqualTo(0));
        });
    }

    /// <summary>
    /// Достаёт правильное значение клетки из решения. Не пользуется приватным
    /// полем _solution напрямую — копирует доску и решает её собственным сольвером.
    /// </summary>
    private int SolveAndGet(int row, int col)
    {
        var board = new Board();
        foreach (var cell in _game.GetAllCells())
            if (cell.IsGiven) board.Set(cell.Row, cell.Col, cell.Value);
        BoardSolver.TrySolve(board);
        return board.Get(row, col);
    }
}
