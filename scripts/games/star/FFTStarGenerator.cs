using Godot;
using Plutono.Scripts.Utils;
using System;

namespace starrynight;

/// <summary>
/// Generates star positions based on FFT audio analysis
/// X positions are deterministic (same for each playthrough of same music)
/// Y positions use FFT + random seed (different each playthrough)
/// </summary>
public class FFTStarGenerator : IStarGenerator
{
    private readonly FrequencyBand frequencyBand;
    private readonly RandomNumberGenerator random;
    private readonly int playthroughSeed;

    /// <summary>
    /// Create FFT star generator with specified frequency band
    /// </summary>
    /// <param name="band">Which frequency band to use for analysis</param>
    /// <param name="seed">Seed for Y position randomness (0 = time-based)</param>
    public FFTStarGenerator(FrequencyBand band = FrequencyBand.MidRange, int seed = 0)
    {
        frequencyBand = band;
        playthroughSeed = seed == 0 ? (int)DateTime.Now.Ticks : seed;

        random = new RandomNumberGenerator();
        random.Seed = (ulong)playthroughSeed;

        Debug.Log($"FFTStarGenerator initialized with band={band}, seed={playthroughSeed}");
    }

    public Vector2 GenerateStarPosition(float xPosition)
    {
        // Get FFT amplitude for the configured frequency band
        var fftValue = GetFFTValueForBand();

        // If below threshold, return position outside bounds (will be skipped by spawner)
        if (fftValue < Parameters.FFTThreshold)
        {
            return new Vector2(xPosition, Parameters.MaxStarHeight + 100f); // Off-screen
        }

        // Scale FFT amplitude to Y position range
        var fftYPosition = MapFFTToYPosition(fftValue);

        // Add random offset for variety across playthroughs
        var randomOffset = random.RandfRange(-Parameters.FFTYRandomRange, Parameters.FFTYRandomRange);
        var finalY = Mathf.Clamp(
            fftYPosition + randomOffset,
            Parameters.MinStarHeight,
            Parameters.MaxStarHeight
        );

        return new Vector2(xPosition, finalY);
    }

    /// <summary>
    /// Get FFT value based on configured frequency band
    /// </summary>
    private float GetFFTValueForBand()
    {
        var analyzer = AudioAnalyzer.Instance;
        if (analyzer == null || !analyzer.IsPlaying())
        {
            return 0f;
        }

        return frequencyBand switch
        {
            FrequencyBand.Bass => analyzer.GetFFTValue(FrequencyBand.Bass),
            FrequencyBand.MidRange => analyzer.GetFFTValue(FrequencyBand.MidRange),
            FrequencyBand.MultiBand => analyzer.GetFFTValue(FrequencyBand.MidRange), // Use mid for Y position
            _ => 0f
        };
    }

    /// <summary>
    /// Map FFT amplitude (0-1) to Y position (-90 to 90)
    /// Uses non-linear mapping to emphasize peaks
    /// </summary>
    private float MapFFTToYPosition(float fftValue)
    {
        // Apply power curve to emphasize peaks (makes quiet sections spawn low, loud sections high)
        var emphasizedValue = Mathf.Pow(fftValue, 1.5f);

        // Map to Y range: 0 -> MinHeight, 1 -> MaxHeight
        var normalizedY = emphasizedValue * 2f - 1f; // Map 0-1 to -1 to 1
        return normalizedY * Parameters.FFTAmplitudeScale;
    }
}
