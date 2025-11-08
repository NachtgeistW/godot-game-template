using Godot;
using Plutono.Util;

namespace starrynight;

public partial class UIController : CanvasLayer
{
    private Label _scoreLabel;

    public override void _Ready()
    {
        _scoreLabel = GetNode<Label>("ScoreLabel");
        EventCenter.AddListener<ScoreChangedEvent>(OnScoreChanged);
        UpdateScoreDisplay(0);
    }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        UpdateScoreDisplay(evt.Score);
    }

    private void UpdateScoreDisplay(int score)
    {
        _scoreLabel.Text = $"Score: {score}";
    }

    public override void _ExitTree()
    {
        EventCenter.RemoveListener<ScoreChangedEvent>(OnScoreChanged);
    }
}
