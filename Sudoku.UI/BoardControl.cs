using Sudoku.Engine;

namespace Sudoku.UI;

/// <summary>
/// Кастомный контрол, рисующий сетку Судоку 9×9. Реагирует на мышь и клавиатуру.
/// Дёргает только публичный API <see cref="SudokuGame"/> — не знает про правила Судоку.
/// </summary>
public sealed class BoardControl : Control
{
    private const int GridSize = 9;
    private const int BlockSize = 3;

    private SudokuGame? _game;
    private int _selectedRow = 0;
    private int _selectedCol = 0;
    private (int row, int col)? _lastHint;

    public BoardControl()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint
               | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        TabStop = true;
        BackColor = Color.White;
        MinimumSize = new Size(360, 360);
    }

    public void Bind(SudokuGame game)
    {
        if (_game is not null)
        {
            _game.MoveApplied -= OnMoveApplied;
        }
        _game = game;
        _selectedRow = _selectedCol = 0;
        _lastHint = null;
        if (_game is not null)
            _game.MoveApplied += OnMoveApplied;
        Invalidate();
    }

    /// <summary>Подсветить клетку как «только что подсказанную» (UI вызывает после UseHint).</summary>
    public void FlashHint(int row, int col)
    {
        _lastHint = (row, col);
        _selectedRow = row;
        _selectedCol = col;
        Invalidate();
    }

    private void OnMoveApplied(object? sender, MoveAppliedEventArgs e)
    {
        if (InvokeRequired) BeginInvoke(new Action(Invalidate));
        else Invalidate();
    }

    protected override bool IsInputKey(Keys keyData) => keyData switch
    {
        Keys.Up or Keys.Down or Keys.Left or Keys.Right => true,
        _ => base.IsInputKey(keyData),
    };

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        var cell = HitTest(e.X, e.Y);
        if (cell is null) return;
        _selectedRow = cell.Value.row;
        _selectedCol = cell.Value.col;
        _lastHint = null;
        Invalidate();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (_game is null) return;
        switch (e.KeyCode)
        {
            case Keys.Up:    MoveSelection(-1, 0); e.Handled = true; break;
            case Keys.Down:  MoveSelection(+1, 0); e.Handled = true; break;
            case Keys.Left:  MoveSelection(0, -1); e.Handled = true; break;
            case Keys.Right: MoveSelection(0, +1); e.Handled = true; break;
            case Keys.D0 or Keys.NumPad0 or Keys.Back or Keys.Delete:
                _game.TryPlace(_selectedRow, _selectedCol, 0);
                _lastHint = null;
                e.Handled = true;
                break;
            case >= Keys.D1 and <= Keys.D9:
                _game.TryPlace(_selectedRow, _selectedCol, e.KeyCode - Keys.D0);
                _lastHint = null;
                e.Handled = true;
                break;
            case >= Keys.NumPad1 and <= Keys.NumPad9:
                _game.TryPlace(_selectedRow, _selectedCol, e.KeyCode - Keys.NumPad0);
                _lastHint = null;
                e.Handled = true;
                break;
        }
    }

    private void MoveSelection(int dr, int dc)
    {
        _selectedRow = Math.Clamp(_selectedRow + dr, 0, GridSize - 1);
        _selectedCol = Math.Clamp(_selectedCol + dc, 0, GridSize - 1);
        Invalidate();
    }

    private (int row, int col)? HitTest(int x, int y)
    {
        var cell = Math.Min(ClientSize.Width, ClientSize.Height) / GridSize;
        if (cell <= 0) return null;
        var col = x / cell;
        var row = y / cell;
        if (row is < 0 or >= GridSize || col is < 0 or >= GridSize) return null;
        return (row, col);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var cell = Math.Min(ClientSize.Width, ClientSize.Height) / GridSize;
        if (cell <= 0) return;
        var boardSize = cell * GridSize;

        DrawCellBackgrounds(g, cell);
        DrawDigits(g, cell);
        DrawGridLines(g, cell, boardSize);
    }

    private void DrawCellBackgrounds(Graphics g, int cell)
    {
        for (int r = 0; r < GridSize; r++)
            for (int c = 0; c < GridSize; c++)
            {
                var rect = new Rectangle(c * cell, r * cell, cell, cell);
                var color = ChooseBackground(r, c);
                if (color != Color.White)
                    using (var brush = new SolidBrush(color))
                        g.FillRectangle(brush, rect);
            }
    }

    private Color ChooseBackground(int row, int col)
    {
        // Подсказка > конфликт > выделенная > подсветка ряда/столбца/блока > белый.
        if (_game is null) return Color.White;
        var info = _game.GetCell(row, col);

        if (_lastHint is { } h && h.row == row && h.col == col)
            return Color.FromArgb(255, 240, 180);   // янтарный — подсказка

        if (info.HasConflict)
            return Color.FromArgb(255, 200, 200);   // светло-красный — ошибка

        if (row == _selectedRow && col == _selectedCol)
            return Color.FromArgb(180, 220, 255);   // голубой — выделенная

        if (row == _selectedRow || col == _selectedCol
            || (row / BlockSize == _selectedRow / BlockSize
             && col / BlockSize == _selectedCol / BlockSize))
            return Color.FromArgb(235, 240, 248);   // серо-голубой — подсветка

        // Совпадающее значение с выделенной (популярный helper).
        var sel = _game.GetCell(_selectedRow, _selectedCol);
        if (info.Value != 0 && info.Value == sel.Value)
            return Color.FromArgb(220, 230, 240);

        return Color.White;
    }

    private void DrawDigits(Graphics g, int cell)
    {
        if (_game is null) return;
        using var givenFont = new Font("Segoe UI", cell * 0.45f, FontStyle.Bold);
        using var userFont  = new Font("Segoe UI", cell * 0.45f, FontStyle.Regular);
        using var fmt = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };

        for (int r = 0; r < GridSize; r++)
            for (int c = 0; c < GridSize; c++)
            {
                var info = _game.GetCell(r, c);
                if (info.Value == 0) continue;

                var rect = new Rectangle(c * cell, r * cell, cell, cell);
                Color textColor;
                Font font;
                if (info.IsGiven)          { textColor = Color.Black;        font = givenFont; }
                else if (info.HasConflict) { textColor = Color.DarkRed;      font = userFont;  }
                else                       { textColor = Color.MidnightBlue; font = userFont;  }

                using var brush = new SolidBrush(textColor);
                g.DrawString(info.Value.ToString(), font, brush, rect, fmt);
            }
    }

    private static void DrawGridLines(Graphics g, int cell, int boardSize)
    {
        using var thin = new Pen(Color.LightGray, 1);
        using var thick = new Pen(Color.Black, 2.5f);

        for (int i = 0; i <= GridSize; i++)
        {
            var pen = i % BlockSize == 0 ? thick : thin;
            g.DrawLine(pen, i * cell, 0, i * cell, boardSize);
            g.DrawLine(pen, 0, i * cell, boardSize, i * cell);
        }
    }
}
