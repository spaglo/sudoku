namespace Sudoku.Engine;

public enum MoveResult
{
    /// <summary>Число поставлено и оно правильное.</summary>
    Accepted,

    /// <summary>Число поставлено, но не совпадает с решением — списывается жизнь.</summary>
    Conflict,

    /// <summary>Клетка — часть исходного паззла, её нельзя изменять.</summary>
    GivenCell,

    /// <summary>Координаты или value вне допустимого диапазона.</summary>
    OutOfRange,

    /// <summary>value=0: клетка очищена.</summary>
    Cleared,

    /// <summary>Ход завершил партию — все клетки правильно заполнены.</summary>
    Solved,
}
