#nullable enable
namespace Sudoku.UI;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private BoardControl boardControl = null!;
    private Label labelTimer = null!;
    private Label labelLives = null!;
    private Label labelHints = null!;
    private Label labelDifficulty = null!;
    private Button btnHint = null!;
    private Button btnRestart = null!;
    private Label labelNewGame = null!;
    private Button btnNewEasy = null!;
    private Button btnNewMedium = null!;
    private Button btnNewHard = null!;
    private Button btnNewExpert = null!;

    private void InitializeComponent()
    {
        boardControl = new BoardControl();
        labelTimer = new Label();
        labelLives = new Label();
        labelHints = new Label();
        labelDifficulty = new Label();
        btnHint = new Button();
        btnRestart = new Button();
        labelNewGame = new Label();
        btnNewEasy = new Button();
        btnNewMedium = new Button();
        btnNewHard = new Button();
        btnNewExpert = new Button();

        SuspendLayout();

        // Доска занимает левую часть окна.
        boardControl.Location = new Point(15, 15);
        boardControl.Size = new Size(540, 540);
        boardControl.TabIndex = 0;

        // Правая колонка — статус.
        var infoFont = new Font("Segoe UI", 14F);

        labelTimer.Location = new Point(575, 20);
        labelTimer.Size = new Size(220, 30);
        labelTimer.Font = infoFont;
        labelTimer.Text = "⏱ 00:00";

        labelLives.Location = new Point(575, 55);
        labelLives.Size = new Size(220, 30);
        labelLives.Font = infoFont;
        labelLives.Text = "❤ 3 / 3";

        labelHints.Location = new Point(575, 90);
        labelHints.Size = new Size(220, 30);
        labelHints.Font = infoFont;
        labelHints.Text = "💡 3 / 3";

        labelDifficulty.Location = new Point(575, 125);
        labelDifficulty.Size = new Size(220, 30);
        labelDifficulty.Font = new Font("Segoe UI", 11F);
        labelDifficulty.Text = "Сложность: Easy";

        btnHint.Location = new Point(575, 170);
        btnHint.Size = new Size(220, 36);
        btnHint.Text = "💡 Подсказка";
        btnHint.Font = new Font("Segoe UI", 10F);
        btnHint.Click += btnHint_Click;

        btnRestart.Location = new Point(575, 212);
        btnRestart.Size = new Size(220, 36);
        btnRestart.Text = "↺ Перезапустить";
        btnRestart.Font = new Font("Segoe UI", 10F);
        btnRestart.Click += btnRestart_Click;

        labelNewGame.Location = new Point(575, 270);
        labelNewGame.Size = new Size(220, 24);
        labelNewGame.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        labelNewGame.Text = "Новая игра:";

        btnNewEasy.Location   = new Point(575, 300);
        btnNewEasy.Size       = new Size(220, 32);
        btnNewEasy.Text       = "Easy";
        btnNewEasy.Click += btnNewEasy_Click;

        btnNewMedium.Location = new Point(575, 336);
        btnNewMedium.Size     = new Size(220, 32);
        btnNewMedium.Text     = "Medium";
        btnNewMedium.Click += btnNewMedium_Click;

        btnNewHard.Location   = new Point(575, 372);
        btnNewHard.Size       = new Size(220, 32);
        btnNewHard.Text       = "Hard";
        btnNewHard.Click += btnNewHard_Click;

        btnNewExpert.Location = new Point(575, 408);
        btnNewExpert.Size     = new Size(220, 32);
        btnNewExpert.Text     = "Expert";
        btnNewExpert.Click += btnNewExpert_Click;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(810, 570);
        Controls.Add(boardControl);
        Controls.Add(labelTimer);
        Controls.Add(labelLives);
        Controls.Add(labelHints);
        Controls.Add(labelDifficulty);
        Controls.Add(btnHint);
        Controls.Add(btnRestart);
        Controls.Add(labelNewGame);
        Controls.Add(btnNewEasy);
        Controls.Add(btnNewMedium);
        Controls.Add(btnNewHard);
        Controls.Add(btnNewExpert);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Sudoku — ЛР 1";

        ResumeLayout(false);
        PerformLayout();
    }
}
