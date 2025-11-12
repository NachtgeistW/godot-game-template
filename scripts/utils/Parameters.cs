namespace Plutono.Scripts.Utils;

public static class Parameters
{
    // Score System
    public const int ScorePerStar = 1;

    // Star Generation
    public const float MinStarHeight = -90f;
    public const float MaxStarHeight = 90f;
    public const float StarSpawnDistance = 800f;
    public const float StarDespawnDistance = 200f;
    public const float StarSpawnInterval = 400f;

    // Meteorite Generation
    public const float MeteoriteSpawnDistance = 1000f;
    public const float MeteoriteDespawnDistance = 200f;
    public const float MeteoriteInitialSpawnInterval = 500f;
    public const float MeteoriteMinSpawnInterval = 100f;
    public const float MeteoriteDifficultyIncreaseTime = 30f; // seconds

    // Player Health
    public const int MaxHealth = 3;
    public const int MeteoriteDamage = 1;

    // Beat-Based Star Generation
    public const float FFTAmplitudeScale = 90f; // Scale FFT amplitude to Y position range
    public const float FFTThreshold = 0.05f; // Minimum FFT amplitude to spawn star
    public const float FFTYRandomRange = 20f; // Random offset range for Y position (±)
    public const float BeatSyncTolerance = 0.01f; // Tolerance for beat detection (seconds)

    // Star-Meteorite Spacing
    public const float MinStarMeteoriteDistance = 250f; // Minimum X distance between star and meteorite
}
