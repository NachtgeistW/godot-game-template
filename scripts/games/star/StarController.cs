using Godot;
using Plutono.Util;
using StarfallNight.scripts.games;

namespace StarfallNight;

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
