namespace Sudoku.Engine;

public sealed class MoveAppliedEventArgs : EventArgs
{
    public int Row { get; init; }
    public int Col { get; init; }
    public int Value { get; init; }
    public MoveResult Result { get; init; }
}
