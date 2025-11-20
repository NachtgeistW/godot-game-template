using Godot;
using Plutono.Util;
using Plutono.Scripts.Utils;

namespace StarfallNight;

public partial class GameManager : Node
{
    private int score = 0;
    private int totalStars = 0;
    private bool isGameOver = false;

    public override void _Ready()
    {
        totalStars = SaveManager.Instance.LoadTotalStars();
        score = totalStars;

        EventCenter.Broadcast(new ScoreChangedEvent(score));

        EventCenter.AddListener<StarCollectedEvent>(OnStarCollected);
        EventCenter.AddListener<GameOverEvent>(OnGameOver);
    }

    private void OnStarCollected(StarCollectedEvent evt)
    {
        if (isGameOver) return;

        score += Parameters.ScorePerStar;
        EventCenter.Broadcast(new ScoreChangedEvent(score));
    }

    private void OnGameOver(GameOverEvent evt)
    {
        isGameOver = true;
        SaveManager.Instance.SaveTotalStars(score);
        EventCenter.Broadcast(new GameSavedEvent());
        GetTree().Paused = true;
    }

    public override void _ExitTree()
    {
        EventCenter.RemoveListener<StarCollectedEvent>(OnStarCollected);
        EventCenter.RemoveListener<GameOverEvent>(OnGameOver);
    }
}
