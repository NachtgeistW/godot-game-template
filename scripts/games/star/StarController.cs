using Godot;
using Plutono.Util;
using starrynight.scripts.games;

namespace starrynight;

public partial class StarController : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is PlayerController)
        {
            EventCenter.Broadcast(new StarCollectedEvent());
            QueueFree();
        }
    }

    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
    }
}
