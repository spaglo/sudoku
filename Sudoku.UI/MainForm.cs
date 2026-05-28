using Sudoku.Engine;

namespace Sudoku.UI;

public partial class MainForm : Form
{
    private SudokuGame _game;

    public MainForm()
    {
        InitializeComponent();
        _game = CreateGame(Difficulty.Easy);
        AttachGame(_game);
        boardControl.Bind(_game);
        UpdateStatus();
    }

    private SudokuGame CreateGame(Difficulty difficulty)
    {
        return new SudokuGame(difficulty);
    }

    private void AttachGame(SudokuGame game)
    {
        game.Tick     += OnTick;
        game.GameWon  += OnGameWon;
        game.GameLost += OnGameLost;
    }

    private void DetachGame(SudokuGame game)
    {
        game.Tick     -= OnTick;
        game.GameWon  -= OnGameWon;
        game.GameLost -= OnGameLost;
    }

    private void OnTick(object? sender, EventArgs e)
        => UiInvoke(UpdateStatus);

    private void OnGameWon(object? sender, EventArgs e) => UiInvoke(() =>
    {
        UpdateStatus();
        MessageBox.Show(this,
            $"Партия решена за {Format(_game.Elapsed)}.\nОшибок: {3 - _game.LivesRemaining}, подсказок: {3 - _game.HintsRemaining}.",
            "Победа!", MessageBoxButtons.OK, MessageBoxIcon.Information);
    });

    private void OnGameLost(object? sender, EventArgs e) => UiInvoke(() =>
    {
        UpdateStatus();
        MessageBox.Show(this,
            "Жизни закончились. Попробуете ещё раз?",
            "Поражение", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    });

    private void UpdateStatus()
    {
        labelTimer.Text   = $"⏱  {Format(_game.Elapsed)}";
        labelLives.Text   = $"❤  {_game.LivesRemaining} / 3";
        labelHints.Text   = $"💡 {_game.HintsRemaining} / 3";
        labelDifficulty.Text = $"Сложность: {_game.Difficulty}";
    }

    private static string Format(TimeSpan t)
        => $"{(int)t.TotalMinutes:D2}:{t.Seconds:D2}";

    private void SwitchGame(SudokuGame newGame)
    {
        DetachGame(_game);
        _game.Dispose();
        _game = newGame;
        AttachGame(_game);
        boardControl.Bind(_game);
        UpdateStatus();
    }

    private void btnNewEasy_Click(object? sender, EventArgs e)    => SwitchGame(CreateGame(Difficulty.Easy));
    private void btnNewMedium_Click(object? sender, EventArgs e)  => SwitchGame(CreateGame(Difficulty.Medium));
    private void btnNewHard_Click(object? sender, EventArgs e)    => SwitchGame(CreateGame(Difficulty.Hard));
    private void btnNewExpert_Click(object? sender, EventArgs e)  => SwitchGame(CreateGame(Difficulty.Expert));

    private void btnHint_Click(object? sender, EventArgs e)
    {
        var hint = _game.UseHint();
        if (hint is { } h) boardControl.FlashHint(h.row, h.col);
        UpdateStatus();
        boardControl.Focus();
    }

    private void btnRestart_Click(object? sender, EventArgs e)
    {
        _game.Restart();
        boardControl.Bind(_game);
        UpdateStatus();
        boardControl.Focus();
    }

    private void UiInvoke(Action action)
    {
        if (IsDisposed) return;
        try
        {
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }
        catch (ObjectDisposedException) { /* форма закрылась */ }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        DetachGame(_game);
        _game.Dispose();
        base.OnFormClosed(e);
    }
}
