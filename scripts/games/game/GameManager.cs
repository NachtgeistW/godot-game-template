using Godot;
using Plutono.Util;
using Plutono.Scripts.Utils;

namespace starrynight;

public partial class GameManager : Node
{
    private int _score = 0;

    public override void _Ready()
    {
        EventCenter.AddListener<StarCollectedEvent>(OnStarCollected);
    }

    private void OnStarCollected(StarCollectedEvent evt)
    {
        _score += Parameters.ScorePerStar;
        EventCenter.Broadcast(new ScoreChangedEvent(_score));
    }

    public override void _ExitTree()
    {
        EventCenter.RemoveListener<StarCollectedEvent>(OnStarCollected);
    }
}
