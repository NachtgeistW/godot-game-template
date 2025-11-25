using Godot;
using Plutono.Util;
using Plutono.Scripts.Utils;

namespace StarfallNight;

public partial class UIController : CanvasLayer
{
    [Export] private Label scoreLabel;
    [Export] private Label healthLabel;
    [Export] private Control gameOverPanel;
    [Export] private Label finalScoreLabel;
    [Export] private Button restartButton;
    [Export] private Button menuButton;

    private int currentScore;

    public override void _Ready()
    {
        EventCenter.AddListener<ScoreChangedEvent>(OnScoreChanged);
        EventCenter.AddListener<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
        EventCenter.AddListener<GameOverEvent>(OnGameOver);

        gameOverPanel.Visible = false;

        UpdateScoreDisplay(0);
        UpdateHealthDisplay(Parameters.MaxHealth);

        restartButton.Text = Tr("RESTART");
        menuButton.Text = Tr("MENU");

        restartButton.Pressed += OnRestartPressed;
        menuButton.Pressed += OnMenuPressed;
    }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        currentScore = evt.Score;
        UpdateScoreDisplay(evt.Score);
    }

    private void OnPlayerHealthChanged(PlayerHealthChangedEvent evt)
    {
        UpdateHealthDisplay(evt.CurrentHealth);
    }

    private void OnGameOver(GameOverEvent evt)
    {
        gameOverPanel.Visible = true;
        finalScoreLabel.Text = $"Final Score: {currentScore}";
    }

    private void UpdateScoreDisplay(int score)
    {
        scoreLabel.Text = $"Score: {score}";
    }

    private void UpdateHealthDisplay(int health)
    {
        healthLabel.Text = $"HP: {health}/{Parameters.MaxHealth}";
    }

    private void OnRestartPressed()
    {
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
    }

    private void OnMenuPressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://scenes/start_menu.tscn");
    }

    public override void _ExitTree()
    {
        EventCenter.RemoveListener<ScoreChangedEvent>(OnScoreChanged);
        EventCenter.RemoveListener<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
        EventCenter.RemoveListener<GameOverEvent>(OnGameOver);

        restartButton.Pressed -= OnRestartPressed;
        menuButton.Pressed -= OnMenuPressed;
    }
}
