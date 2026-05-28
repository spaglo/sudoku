namespace Sudoku.Engine;

/// <summary>
/// Внутреннее представление доски Судоку 9×9. Хранит значения (0 = пусто, 1..9)
/// и флаги «заданная клетка». Все проверки правил Судоку живут здесь.
/// </summary>
internal sealed class Board
{
    public const int Size = 9;
    public const int BlockSize = 3;

    private readonly int[,] _values = new int[Size, Size];
    private readonly bool[,] _given = new bool[Size, Size];

    public int Get(int row, int col) => _values[row, col];
    public bool IsGiven(int row, int col) => _given[row, col];
    public bool IsEmpty(int row, int col) => _values[row, col] == 0;

    public void Set(int row, int col, int value) => _values[row, col] = value;

    /// <summary>Помечает текущее ненулевое содержимое как «заданное» (исходный паззл).</summary>
    public void FreezeGivens()
    {
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                _given[r, c] = _values[r, c] != 0;
    }

    public Board Clone()
    {
        var copy = new Board();
        Array.Copy(_values, copy._values, _values.Length);
        Array.Copy(_given, copy._given, _given.Length);
        return copy;
    }

    /// <summary>
    /// Можно ли поставить <paramref name="value"/> в клетку (row,col),
    /// не порождая дубликата в строке, столбце или блоке 3×3.
    /// Текущее значение клетки игнорируется.
    /// </summary>
    public bool CanPlace(int row, int col, int value)
    {
        if (value is < 1 or > 9) return false;

        for (int i = 0; i < Size; i++)
        {
            if (i != col && _values[row, i] == value) return false;
            if (i != row && _values[i, col] == value) return false;
        }

        var blockRow = row - row % BlockSize;
        var blockCol = col - col % BlockSize;
        for (int r = blockRow; r < blockRow + BlockSize; r++)
            for (int c = blockCol; c < blockCol + BlockSize; c++)
                if ((r != row || c != col) && _values[r, c] == value) return false;

        return true;
    }

    /// <summary>Полностью ли заполнена доска (нет нулей).</summary>
    public bool IsFull()
    {
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                if (_values[r, c] == 0) return false;
        return true;
    }

    /// <summary>Находит первую пустую клетку — обходом по строкам.</summary>
    public bool TryFindEmpty(out int row, out int col)
    {
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                if (_values[r, c] == 0)
                {
                    row = r; col = c;
                    return true;
                }
        row = col = -1;
        return false;
    }
}
