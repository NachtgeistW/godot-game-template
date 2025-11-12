using Godot;
using Plutono.Util;
using starrynight;

namespace Plutono.Scripts.Utils;

/// <summary>
/// Tracks beat progression and broadcasts beat events for rhythm-based gameplay
/// </summary>
public partial class BeatSynchronizer : Node
{
    [Export] public BeatSubdivision Subdivision { get; set; } = BeatSubdivision.Eighth;

    private float lastBeat = -1f;
    private int lastBeatInt = -1;
    private float lastPlaybackPosition = 0f;

    public override void _Process(double delta)
    {
        var analyzer = AudioAnalyzer.Instance;
        if (analyzer == null || !analyzer.IsPlaying())
        {
            return;
        }

        // Check for loop or restart (playback position jumped backwards)
        var currentPosition = analyzer.GetPlaybackPosition();
        if (currentPosition < lastPlaybackPosition - 1f) // 1 second tolerance for slight variations
        {
            Debug.Log("BeatSynchronizer: Detected loop or restart, resetting beat tracking");
            lastBeat = -1f;
            lastBeatInt = -1;
        }
        lastPlaybackPosition = currentPosition;

        // Get current subdivided beat
        var currentBeat = analyzer.GetSubdividedBeat(Subdivision);
        var beatInt = Mathf.FloorToInt(currentBeat);

        // Detect new beat subdivision hit
        if (beatInt > lastBeatInt && analyzer.GetBpm() > 0)
        {
            // Broadcast beat event
            EventCenter.Broadcast(new BeatHitEvent(beatInt, currentBeat));

            // Log every 8th beat to avoid spam (adjust as needed)
            if (beatInt % 8 == 0)
            {
                Debug.Log($"BeatSynchronizer: Beat {beatInt} at {currentPosition:F2}s (subdivision: {Subdivision})");
            }
        }

        lastBeat = currentBeat;
        lastBeatInt = beatInt;
    }

    public override void _Ready()
    {
        var analyzer = AudioAnalyzer.Instance;
        if (analyzer == null)
        {
            Debug.LogWarning("BeatSynchronizer: AudioAnalyzer not found!");
            return;
        }

        var bpm = analyzer.GetBpm();
        if (bpm <= 0)
        {
            Debug.LogWarning($"BeatSynchronizer: BPM not configured in audio import settings! Please set BPM in the Import tab.");
        }
        else
        {
            Debug.Log($"BeatSynchronizer: Initialized with BPM={bpm}, Subdivision={Subdivision}");
        }
    }
}
