namespace Sudoku.Engine.Tests;

[TestFixture]
public class BoardSolverTests
{
    /// <summary>Известный паззл с уникальным решением (37 клеток заполнены).</summary>
    private static readonly int[,] KnownPuzzle =
    {
        {5,3,0, 0,7,0, 0,0,0},
        {6,0,0, 1,9,5, 0,0,0},
        {0,9,8, 0,0,0, 0,6,0},

        {8,0,0, 0,6,0, 0,0,3},
        {4,0,0, 8,0,3, 0,0,1},
        {7,0,0, 0,2,0, 0,0,6},

        {0,6,0, 0,0,0, 2,8,0},
        {0,0,0, 4,1,9, 0,0,5},
        {0,0,0, 0,8,0, 0,7,9},
    };

    private static Board From(int[,] values)
    {
        var b = new Board();
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                b.Set(r, c, values[r, c]);
        return b;
    }

    [Test]
    public void TrySolve_KnownPuzzle_ReturnsTrue_AndFillsBoard()
    {
        var b = From(KnownPuzzle);
        Assert.That(BoardSolver.TrySolve(b), Is.True);
        Assert.That(b.IsFull(), Is.True);
    }

    [Test]
    public void CountSolutions_KnownPuzzle_IsExactlyOne()
    {
        var b = From(KnownPuzzle);
        Assert.That(BoardSolver.CountSolutions(b, 2), Is.EqualTo(1));
    }

    [Test]
    public void CountSolutions_EmptyBoard_StopsAtLimit()
    {
        var b = new Board();
        // У пустой доски решений >> 1; ограничение должно сработать.
        Assert.That(BoardSolver.CountSolutions(b, 2), Is.EqualTo(2));
    }

    [Test]
    public void TrySolve_ContradictoryBoard_ReturnsFalse()
    {
        // Конструируем «нерешаемую через одну клетку» ситуацию:
        // в строке 0 заняты столбцы 0..7 значениями 1..8 → в (0,8) обязано быть 9,
        // но 9 уже стоит в (1,8) → 0 кандидатов на (0,8) → нерешаемо.
        var b = new Board();
        for (int v = 1; v <= 8; v++) b.Set(0, v - 1, v);
        b.Set(1, 8, 9);

        Assert.That(BoardSolver.TrySolve(b), Is.False);
    }
}
