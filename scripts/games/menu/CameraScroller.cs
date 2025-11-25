using Godot;

namespace StarfallNight;

public partial class CameraScroller : Camera2D
{
    [Export] private float scrollSpeed = 150f;

    public override void _Process(double delta)
    {
        Position += new Vector2(scrollSpeed * (float)delta, 0);
    }
}
