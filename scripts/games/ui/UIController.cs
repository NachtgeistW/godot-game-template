using Godot;
using Plutono.Util;
using Plutono.Scripts.Utils;

namespace starrynight;

public partial class UIController : CanvasLayer
{
    private Label _scoreLabel;
    private Label _healthLabel;
    private Control _gameOverPanel;
    private Label _finalScoreLabel;
    private Button _restartButton;
    private Button _menuButton;

    private int _currentScore;

    public override void _Ready()
    {
        _scoreLabel = GetNode<Label>("ScoreLabel");
        _healthLabel = GetNode<Label>("HealthLabel");
        _gameOverPanel = GetNode<Control>("GameOverPanel");
        _finalScoreLabel = GetNode<Label>("GameOverPanel/FinalScoreLabel");
        _restartButton = GetNode<Button>("GameOverPanel/RestartButton");
        _menuButton = GetNode<Button>("GameOverPanel/MenuButton");

        EventCenter.AddListener<ScoreChangedEvent>(OnScoreChanged);
        EventCenter.AddListener<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
        EventCenter.AddListener<GameOverEvent>(OnGameOver);

        _gameOverPanel.Visible = false;
        UpdateScoreDisplay(0);
        UpdateHealthDisplay(Parameters.MaxHealth);

        _restartButton.Pressed += OnRestartPressed;
        _menuButton.Pressed += OnMenuPressed;
    }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        _currentScore = evt.Score;
        UpdateScoreDisplay(evt.Score);
    }

    private void OnPlayerHealthChanged(PlayerHealthChangedEvent evt)
    {
        UpdateHealthDisplay(evt.CurrentHealth);
    }

    private void OnGameOver(GameOverEvent evt)
    {
        _gameOverPanel.Visible = true;
        _finalScoreLabel.Text = $"Final Score: {_currentScore}";
    }

    private void UpdateScoreDisplay(int score)
    {
        _scoreLabel.Text = $"Score: {score}";
    }

    private void UpdateHealthDisplay(int health)
    {
        _healthLabel.Text = $"HP: {health}/{Parameters.MaxHealth}";
    }

    private void OnRestartPressed()
    {
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
    }

    private void OnMenuPressed()
    {
        GetTree().Paused = false;
        // TODO: Implement main menu scene transition
        GetTree().Quit();
    }

    public override void _ExitTree()
    {
        EventCenter.RemoveListener<ScoreChangedEvent>(OnScoreChanged);
        EventCenter.RemoveListener<PlayerHealthChangedEvent>(OnPlayerHealthChanged);
        EventCenter.RemoveListener<GameOverEvent>(OnGameOver);

        _restartButton.Pressed -= OnRestartPressed;
        _menuButton.Pressed -= OnMenuPressed;
    }
}
