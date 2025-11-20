using Plutono.Util;

namespace StarfallNight;

public struct StarCollectedEvent : IEvent;

public struct ScoreChangedEvent : IEvent
{
    public int Score { get; init; }

    public ScoreChangedEvent(int score)
    {
        Score = score;
    }
}

public struct MeteoriteHitEvent : IEvent
{
}

public struct PlayerHealthChangedEvent : IEvent
{
    public int CurrentHealth { get; init; }

    public PlayerHealthChangedEvent(int currentHealth)
    {
        CurrentHealth = currentHealth;
    }
}

public struct GameOverEvent : IEvent;
public struct GameRestartEvent : IEvent;

public struct BeatHitEvent : IEvent
{
    public int BeatNumber { get; init; }
    public float ExactBeat { get; init; }

    public BeatHitEvent(int beatNumber, float exactBeat)
    {
        BeatNumber = beatNumber;
        ExactBeat = exactBeat;
    }
}


