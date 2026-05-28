namespace Sudoku.Engine;

/// <summary>
/// Генератор паззлов Судоку с гарантированно единственным решением.
/// </summary>
internal static class BoardGenerator
{
    /// <summary>
    /// Возвращает пару (puzzle, solution): puzzle — доска с дырами для игрока,
    /// solution — её полное решение.
    /// </summary>
    public static (Board puzzle, int[,] solution) Generate(Difficulty difficulty, Random rng)
    {
        var board = new Board();
        // 1. Заполняем три независимых диагональных блока случайными перестановками 1..9.
        for (int b = 0; b < Board.Size; b += Board.BlockSize)
            FillDiagonalBlock(board, b, rng);

        // 2. Досолвиваем — получаем полное решение.
        if (!BoardSolver.TrySolve(board))
            throw new InvalidOperationException("Не удалось построить полную доску");

        var solution = SnapshotValues(board);

        // 3. Случайно удаляем клетки, оставляя единственность решения.
        var target = TargetEmptyCells(difficulty);
        RemoveCells(board, target, rng);

        board.FreezeGivens();
        return (board, solution);
    }

    private static int TargetEmptyCells(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy   => 38,
        Difficulty.Medium => 46,
        Difficulty.Hard   => 52,
        Difficulty.Expert => 56,
        _ => 46,
    };

    private static void FillDiagonalBlock(Board board, int corner, Random rng)
    {
        var values = Enumerable.Range(1, 9).ToArray();
        Shuffle(values, rng);
        var k = 0;
        for (int r = corner; r < corner + Board.BlockSize; r++)
            for (int c = corner; c < corner + Board.BlockSize; c++)
                board.Set(r, c, values[k++]);
    }

    private static void RemoveCells(Board board, int targetEmpty, Random rng)
    {
        var positions = new (int r, int c)[Board.Size * Board.Size];
        var i = 0;
        for (int r = 0; r < Board.Size; r++)
            for (int c = 0; c < Board.Size; c++)
                positions[i++] = (r, c);
        Shuffle(positions, rng);

        var removed = 0;
        foreach (var (r, c) in positions)
        {
            if (removed >= targetEmpty) break;
            var backup = board.Get(r, c);
            if (backup == 0) continue;

            board.Set(r, c, 0);
            // Решение должно оставаться единственным.
            // Работаем на клоне — CountSolutions затирает значения по ходу.
            var clone = board.Clone();
            if (BoardSolver.CountSolutions(clone, 2) == 1)
                removed++;
            else
                board.Set(r, c, backup); // вернули — лишило бы единственности
        }
    }

    private static int[,] SnapshotValues(Board board)
    {
        var snap = new int[Board.Size, Board.Size];
        for (int r = 0; r < Board.Size; r++)
            for (int c = 0; c < Board.Size; c++)
                snap[r, c] = board.Get(r, c);
        return snap;
    }

    private static void Shuffle<T>(T[] array, Random rng)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
