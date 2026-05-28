namespace Sudoku.Engine.Tests;

[TestFixture]
public class BoardGeneratorTests
{
    private static int CountGivens(Board board)
    {
        var n = 0;
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (board.IsGiven(r, c)) n++;
        return n;
    }

    [Test]
    public void Generate_Easy_HasUniqueSolution()
    {
        var (puzzle, _) = BoardGenerator.Generate(Difficulty.Easy, new Random(1));
        Assert.That(BoardSolver.CountSolutions(puzzle.Clone(), 2), Is.EqualTo(1));
    }

    [Test]
    public void Generate_Hard_HasUniqueSolution()
    {
        var (puzzle, _) = BoardGenerator.Generate(Difficulty.Hard, new Random(2));
        Assert.That(BoardSolver.CountSolutions(puzzle.Clone(), 2), Is.EqualTo(1));
    }

    [Test]
    public void Generate_SolutionMatchesPuzzle_OnGivenCells()
    {
        var (puzzle, solution) = BoardGenerator.Generate(Difficulty.Medium, new Random(3));
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (puzzle.IsGiven(r, c))
                    Assert.That(puzzle.Get(r, c), Is.EqualTo(solution[r, c]));
    }

    [Test]
    public void Generate_EasyHasMoreGivensThan_Expert()
    {
        var (easy, _) = BoardGenerator.Generate(Difficulty.Easy, new Random(4));
        var (expert, _) = BoardGenerator.Generate(Difficulty.Expert, new Random(5));
        Assert.That(CountGivens(easy), Is.GreaterThan(CountGivens(expert)));
    }

    [Test]
    public void Generate_SameSeed_ProducesSamePuzzle()
    {
        var (a, _) = BoardGenerator.Generate(Difficulty.Medium, new Random(42));
        var (b, _) = BoardGenerator.Generate(Difficulty.Medium, new Random(42));
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                Assert.That(a.Get(r, c), Is.EqualTo(b.Get(r, c)),
                    $"Расхождение в ({r},{c})");
    }
}
