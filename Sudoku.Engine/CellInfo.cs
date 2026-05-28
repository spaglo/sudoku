namespace Sudoku.Engine;

/// <summary>
/// Снимок состояния клетки для UI. Не несёт ссылки на доску — это «фотография» в момент чтения.
/// </summary>
/// <param name="Row">Строка 0..8.</param>
/// <param name="Col">Столбец 0..8.</param>
/// <param name="Value">Текущее значение 1..9 или 0 если пусто.</param>
/// <param name="IsGiven">true для клеток исходного паззла (UI рисует их жирным/чёрным).</param>
/// <param name="HasConflict">true если значение неверно (не совпадает с решением).</param>
public readonly record struct CellInfo(int Row, int Col, int Value, bool IsGiven, bool HasConflict);
