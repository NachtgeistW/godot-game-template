using Godot;

namespace StarfallNight;

public interface IStarGenerator
{
    Vector2 GenerateStarPosition(float xPosition);
}
