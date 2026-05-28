namespace Sudoku.Engine;

/// <summary>
/// Backtracking-решатель Судоку. Использует MRV-эвристику (Minimum Remaining Values):
/// на каждом шаге выбирается пустая клетка с наименьшим числом допустимых кандидатов.
/// Это сжимает дерево перебора и делает решатель пригодным для генерации.
/// </summary>
internal static class BoardSolver
{
    /// <summary>Пытается решить доску на месте. true если решение найдено.</summary>
    public static bool TrySolve(Board board)
    {
        if (!TryFindMostConstrained(board, out var row, out var col, out var candidates))
            return true; // нет пустых — решено

        foreach (var v in candidates)
        {
            board.Set(row, col, v);
            if (TrySolve(board)) return true;
            board.Set(row, col, 0);
        }
        return false;
    }

    /// <summary>
    /// Считает число решений доски, останавливаясь при достижении <paramref name="limit"/>.
    /// Доска восстанавливается до исходного состояния.
    /// </summary>
    public static int CountSolutions(Board board, int limit = 2)
    {
        var count = 0;
        Count(board, limit, ref count);
        return count;

        static void Count(Board b, int lim, ref int found)
        {
            if (found >= lim) return;
            if (!TryFindMostConstrained(b, out var row, out var col, out var candidates))
            {
                found++;
                return;
            }
            foreach (var v in candidates)
            {
                b.Set(row, col, v);
                Count(b, lim, ref found);
                b.Set(row, col, 0);
                if (found >= lim) return;
            }
        }
    }

    /// <summary>
    /// Находит пустую клетку с наименьшим числом кандидатов.
    /// Если у какой-то клетки 0 кандидатов — возвращает её (тупик: дальше идти бессмысленно).
    /// Возвращает false, если пустых клеток нет.
    /// </summary>
    private static bool TryFindMostConstrained(Board board, out int row, out int col, out int[] candidates)
    {
        row = col = -1;
        candidates = Array.Empty<int>();
        var bestCount = 10; // 9 — максимум, 10 — «не найдено»

        for (int r = 0; r < Board.Size; r++)
            for (int c = 0; c < Board.Size; c++)
            {
                if (board.Get(r, c) != 0) continue;
                var localCandidates = CollectCandidates(board, r, c);
                if (localCandidates.Length < bestCount)
                {
                    row = r;
                    col = c;
                    candidates = localCandidates;
                    bestCount = localCandidates.Length;
                    if (bestCount == 0) return true; // тупик — выходим сразу
                }
            }
        return row != -1;
    }

    private static int[] CollectCandidates(Board board, int row, int col)
    {
        Span<int> buffer = stackalloc int[9];
        var n = 0;
        for (int v = 1; v <= 9; v++)
            if (board.CanPlace(row, col, v))
                buffer[n++] = v;

        if (n == 0) return Array.Empty<int>();
        var result = new int[n];
        for (int i = 0; i < n; i++) result[i] = buffer[i];
        return result;
    }
}
