namespace Sudoku.Engine.Tests;

[TestFixture]
public class BoardTests
{
    [Test]
    public void CanPlace_EmptyBoard_AllowsAnyValue()
    {
        var b = new Board();
        Assert.That(b.CanPlace(0, 0, 5), Is.True);
    }

    [Test]
    public void CanPlace_DuplicateInRow_Rejected()
    {
        var b = new Board();
        b.Set(0, 0, 7);
        Assert.That(b.CanPlace(0, 8, 7), Is.False);
    }

    [Test]
    public void CanPlace_DuplicateInColumn_Rejected()
    {
        var b = new Board();
        b.Set(0, 3, 9);
        Assert.That(b.CanPlace(8, 3, 9), Is.False);
    }

    [Test]
    public void CanPlace_DuplicateInBlock_Rejected()
    {
        var b = new Board();
        b.Set(0, 0, 4);
        Assert.That(b.CanPlace(2, 2, 4), Is.False);
    }

    [Test]
    public void CanPlace_IgnoresOwnCell()
    {
        var b = new Board();
        b.Set(0, 0, 3);
        Assert.That(b.CanPlace(0, 0, 3), Is.True);
    }

    [Test]
    public void Clone_IsIndependent()
    {
        var a = new Board();
        a.Set(0, 0, 5);
        var c = a.Clone();
        c.Set(0, 0, 9);
        Assert.That(a.Get(0, 0), Is.EqualTo(5));
    }
}
