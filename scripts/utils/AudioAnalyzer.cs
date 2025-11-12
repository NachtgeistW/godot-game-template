using Godot;
using System;

namespace Plutono.Scripts.Utils;

/// <summary>
/// Singleton that manages FFT audio analysis for music-reactive gameplay
/// </summary>
public partial class AudioAnalyzer : Node
{
    private static AudioAnalyzer instance;
    public static AudioAnalyzer Instance => instance;

    private AudioStreamPlayer2D audioPlayer;
    private AudioEffectSpectrumAnalyzer spectrumAnalyzer;
    private int audioBusIndex;

    // Frequency ranges (in Hz)
    private const float BassMinFreq = 20f;
    private const float BassMaxFreq = 250f;
    private const float MidMinFreq = 250f;
    private const float MidMaxFreq = 2000f;

    public override void _EnterTree()
    {
        if (instance != null && instance != this)
        {
            QueueFree();
            return;
        }
        instance = this;
    }

    public override void _ExitTree()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    /// Initialize the audio analyzer with the game's audio player
    /// </summary>
    public void Initialize(AudioStreamPlayer2D player)
    {
        audioPlayer = player;

        if (audioPlayer == null)
        {
            GD.PushError("AudioAnalyzer: AudioStreamPlayer2D is null!");
            return;
        }

        // Get the audio bus
        audioBusIndex = AudioServer.GetBusIndex(audioPlayer.Bus);

        // Check if spectrum analyzer already exists
        var effectCount = AudioServer.GetBusEffectCount(audioBusIndex);
        for (int i = 0; i < effectCount; i++)
        {
            var effect = AudioServer.GetBusEffect(audioBusIndex, i);
            if (effect is AudioEffectSpectrumAnalyzer)
            {
                spectrumAnalyzer = effect as AudioEffectSpectrumAnalyzer;
                Debug.Log("AudioAnalyzer: Found existing spectrum analyzer");
                return;
            }
        }

        // Add spectrum analyzer if it doesn't exist
        spectrumAnalyzer = new AudioEffectSpectrumAnalyzer();
        spectrumAnalyzer.FftSize = AudioEffectSpectrumAnalyzer.FftSizeEnum.Size2048;
        spectrumAnalyzer.TapBackPos = 0.0f;
        AudioServer.AddBusEffect(audioBusIndex, spectrumAnalyzer);

        Debug.Log("AudioAnalyzer: Initialized with spectrum analyzer");
    }

    /// <summary>
    /// Get FFT amplitude for the specified frequency band
    /// </summary>
    public float GetFFTValue(FrequencyBand band)
    {
        if (spectrumAnalyzer == null || audioPlayer == null || !audioPlayer.Playing)
        {
            return 0f;
        }

        var effectInstance = AudioServer.GetBusEffectInstance(audioBusIndex, 0) as AudioEffectSpectrumAnalyzerInstance;
        if (effectInstance == null)
        {
            return 0f;
        }

        return band switch
        {
            FrequencyBand.Bass => GetAverageAmplitude(effectInstance, BassMinFreq, BassMaxFreq),
            FrequencyBand.MidRange => GetAverageAmplitude(effectInstance, MidMinFreq, MidMaxFreq),
            FrequencyBand.MultiBand => GetAverageAmplitude(effectInstance, MidMinFreq, MidMaxFreq), // Default to mid for multi-band
            _ => 0f
        };
    }

    /// <summary>
    /// Get bass amplitude specifically for multi-band timing
    /// </summary>
    public float GetBassAmplitude()
    {
        if (spectrumAnalyzer == null || audioPlayer == null || !audioPlayer.Playing)
        {
            return 0f;
        }

        var effectInstance = AudioServer.GetBusEffectInstance(audioBusIndex, 0) as AudioEffectSpectrumAnalyzerInstance;
        if (effectInstance == null)
        {
            return 0f;
        }

        return GetAverageAmplitude(effectInstance, BassMinFreq, BassMaxFreq);
    }

    /// <summary>
    /// Get current playback position in seconds
    /// </summary>
    public float GetPlaybackPosition()
    {
        if (audioPlayer == null || !audioPlayer.Playing)
        {
            return 0f;
        }

        return (float)audioPlayer.GetPlaybackPosition();
    }

    /// <summary>
    /// Check if audio is currently playing
    /// </summary>
    public bool IsPlaying()
    {
        return audioPlayer != null && audioPlayer.Playing;
    }

    /// <summary>
    /// Get BPM (beats per minute) from audio stream metadata
    /// Returns 0 if not configured or stream type doesn't support BPM
    /// </summary>
    public float GetBpm()
    {
        if (audioPlayer?.Stream == null)
        {
            return 0f;
        }

        // Try MP3 format
        if (audioPlayer.Stream is AudioStreamMP3 mp3)
        {
            return (float)mp3.Bpm;
        }

        // Try OGG format
        if (audioPlayer.Stream is AudioStreamOggVorbis ogg)
        {
            return (float)ogg.Bpm;
        }

        return 0f;
    }

    /// <summary>
    /// Get total beat count from audio stream metadata
    /// </summary>
    public int GetBeatCount()
    {
        if (audioPlayer?.Stream == null)
        {
            return 0;
        }

        if (audioPlayer.Stream is AudioStreamMP3 mp3)
        {
            return mp3.BeatCount;
        }

        if (audioPlayer.Stream is AudioStreamOggVorbis ogg)
        {
            return ogg.BeatCount;
        }

        return 0;
    }

    /// <summary>
    /// Get beats per bar (time signature) from audio stream metadata
    /// </summary>
    public int GetBarBeats()
    {
        if (audioPlayer?.Stream == null)
        {
            return 4; // Default to 4/4 time
        }

        if (audioPlayer.Stream is AudioStreamMP3 mp3)
        {
            return mp3.BarBeats;
        }

        if (audioPlayer.Stream is AudioStreamOggVorbis ogg)
        {
            return ogg.BarBeats;
        }

        return 4;
    }

    /// <summary>
    /// Calculate current beat position (fractional) based on playback position and BPM
    /// </summary>
    public float GetCurrentBeat()
    {
        var bpm = GetBpm();
        if (bpm <= 0)
        {
            return 0f;
        }

        var position = GetPlaybackPosition();
        return (position * bpm) / 60f;
    }

    /// <summary>
    /// Get current beat number (integer)
    /// </summary>
    public int GetCurrentBeatInt()
    {
        return Mathf.FloorToInt(GetCurrentBeat());
    }

    /// <summary>
    /// Get current beat with subdivision applied
    /// </summary>
    /// <param name="subdivision">Beat subdivision (Quarter=1, Eighth=2, Sixteenth=4, etc.)</param>
    /// <returns>Subdivided beat position (fractional)</returns>
    public float GetSubdividedBeat(BeatSubdivision subdivision)
    {
        return GetCurrentBeat() * (int)subdivision;
    }

    /// <summary>
    /// Get current subdivided beat number (integer)
    /// </summary>
    public int GetSubdividedBeatInt(BeatSubdivision subdivision)
    {
        return Mathf.FloorToInt(GetSubdividedBeat(subdivision));
    }

    private float GetAverageAmplitude(AudioEffectSpectrumAnalyzerInstance instance, float minFreq, float maxFreq)
    {
        var magnitude = instance.GetMagnitudeForFrequencyRange(minFreq, maxFreq);
        var energy = magnitude.Length();

        // Convert to logarithmic scale and normalize
        var db = Mathf.LinearToDb(energy);
        var normalized = Mathf.Clamp((db + 60f) / 60f, 0f, 1f); // Map -60dB to 0dB range to 0-1

        return normalized;
    }
}
