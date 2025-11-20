using Godot;
using Plutono.Scripts.Utils;

namespace StarfallNight;

public class RandomStarGenerator : IStarGenerator
{
    private readonly RandomNumberGenerator random;
    private readonly float minHeight;
    private readonly float maxHeight;

    public RandomStarGenerator(float minHeight = Parameters.MinStarHeight, float maxHeight = Parameters.MaxStarHeight)
    {
        random = new RandomNumberGenerator();
        random.Randomize();
        this.minHeight = minHeight;
        this.maxHeight = maxHeight;
    }

    public Vector2 GenerateStarPosition(float xPosition)
    {
        var yPosition = random.RandfRange(minHeight, maxHeight);
        return new Vector2(xPosition, yPosition);
    }
}
