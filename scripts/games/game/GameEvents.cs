using Plutono.Util;

namespace starrynight;

public struct StarCollectedEvent : IEvent
{
}

public struct ScoreChangedEvent : IEvent
{
    public int Score { get; init; }

    public ScoreChangedEvent(int score)
    {
        Score = score;
    }
}
