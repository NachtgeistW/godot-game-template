using Godot;
using Plutono.Util;
using Plutono.Scripts.Utils;

namespace starrynight;

public partial class GameManager : Node
{
    private int _score = 0;
    private bool _isGameOver = false;

    public override void _Ready()
    {
        EventCenter.AddListener<StarCollectedEvent>(OnStarCollected);
        EventCenter.AddListener<GameOverEvent>(OnGameOver);
    }

    private void OnStarCollected(StarCollectedEvent evt)
    {
        if (_isGameOver) return;

        _score += Parameters.ScorePerStar;
        EventCenter.Broadcast(new ScoreChangedEvent(_score));
    }

    private void OnGameOver(GameOverEvent evt)
    {
        _isGameOver = true;
        GetTree().Paused = true;
    }

    public override void _ExitTree()
    {
        EventCenter.RemoveListener<StarCollectedEvent>(OnStarCollected);
        EventCenter.RemoveListener<GameOverEvent>(OnGameOver);
    }
}
