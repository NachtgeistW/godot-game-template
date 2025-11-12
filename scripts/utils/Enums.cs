namespace Plutono.Scripts.Utils;

/// <summary>
/// Frequency band options for FFT-based star generation
/// </summary>
public enum FrequencyBand
{
    /// <summary>
    /// Bass frequencies (20-250 Hz) - Good for beat-driven generation
    /// </summary>
    Bass,

    /// <summary>
    /// Mid-range frequencies (250-2000 Hz) - Good for melody-driven generation (default)
    /// </summary>
    MidRange,

    /// <summary>
    /// Multi-band analysis - Uses bass for timing, mid for Y position
    /// </summary>
    MultiBand
}

/// <summary>
/// Beat subdivision options for rhythm-based star generation
/// </summary>
public enum BeatSubdivision
{
    /// <summary>
    /// Quarter notes - Spawn on every beat (1 per beat)
    /// </summary>
    Quarter = 1,

    /// <summary>
    /// Eighth notes - Spawn twice per beat (2 per beat)
    /// </summary>
    Eighth = 2,

    /// <summary>
    /// Sixteenth notes - Spawn four times per beat (4 per beat)
    /// </summary>
    Sixteenth = 4,

    /// <summary>
    /// Triplets - Spawn three times per beat (3 per beat)
    /// </summary>
    Triplet = 3
}
