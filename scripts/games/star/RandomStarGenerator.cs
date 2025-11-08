using Godot;
using Plutono.Scripts.Utils;

namespace starrynight;

public class RandomStarGenerator : IStarGenerator
{
    private readonly RandomNumberGenerator _random;
    private readonly float _minHeight;
    private readonly float _maxHeight;

    public RandomStarGenerator(float minHeight = Parameters.MinStarHeight, float maxHeight = Parameters.MaxStarHeight)
    {
        _random = new RandomNumberGenerator();
        _random.Randomize();
        _minHeight = minHeight;
        _maxHeight = maxHeight;
    }

    public Vector2 GenerateStarPosition(float xPosition)
    {
        var yPosition = _random.RandfRange(_minHeight, _maxHeight);
        return new Vector2(xPosition, yPosition);
    }
}
