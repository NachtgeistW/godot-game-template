using Godot;
using Plutono.Scripts.Utils;
using Plutono.Util;
using starrynight.scripts.games;
using System.Collections.Generic;

namespace starrynight;

public partial class StarSpawner : Node2D
{
    [Export] public NodePath CameraPath { get; set; }
    [Export] public NodePath AudioPlayerPath { get; set; }
    [Export] public FrequencyBand FrequencyBand { get; set; } = FrequencyBand.MidRange;

    private Camera2D camera;
    private PlayerController player;
    private AudioStreamPlayer2D audioPlayer;
    private IStarGenerator generator;
    private PackedScene starPrefab;
    private readonly List<Node2D> activeStars = [];
    private readonly List<float> recentStarXPositions = []; // For meteorite collision avoidance

    public override void _Ready()
    {
        camera = GetNode<Camera2D>(CameraPath);
        player = camera.GetParent<PlayerController>(); // Get player from camera's parent
        audioPlayer = GetNode<AudioStreamPlayer2D>(AudioPlayerPath);
        starPrefab = GD.Load<PackedScene>("res://prefabs/star.tscn");

        // Initialize AudioAnalyzer singleton
        if (AudioAnalyzer.Instance == null)
        {
            var analyzer = new AudioAnalyzer();
            AddChild(analyzer);
        }
        AudioAnalyzer.Instance.Initialize(audioPlayer);

        // Create FFT-based generator for Y positions
        generator = new FFTStarGenerator(FrequencyBand);

        // Subscribe to beat events
        EventCenter.AddListener<BeatHitEvent>(OnBeatHit);

        Debug.Log($"StarSpawner initialized with predictive beat spawning, FFT band: {FrequencyBand}");
    }

    public override void _ExitTree()
    {
        // Unsubscribe from beat events
        EventCenter.RemoveListener<BeatHitEvent>(OnBeatHit);
    }

    public override void _Process(double delta)
    {
        var cameraX = camera.GlobalPosition.X;

        // Handle despawning only (spawning is done in OnBeatHit)
        DespawnDistantStars(cameraX);
        CleanupOldStarPositions(cameraX);
    }

    /// <summary>
    /// Event handler for beat hits - spawns stars predictively for rhythm game synchronization
    /// Stars are spawned now but will be reached by player at a future beat
    /// </summary>
    private void OnBeatHit(BeatHitEvent beatEvent)
    {
        var analyzer = AudioAnalyzer.Instance;
        if (analyzer == null || player == null)
        {
            return;
        }

        // Calculate time for player to reach spawn position
        var currentSpeed = player.CurrentSpeed;
        if (currentSpeed <= 0)
        {
            return;
        }

        var timeToReach = Parameters.StarSpawnDistance / currentSpeed;

        // Calculate which future beat the player will be at when reaching this star
        var bpm = analyzer.GetBpm();
        if (bpm <= 0)
        {
            Debug.LogWarning("StarSpawner: BPM not configured! Cannot calculate future beat.");
            return;
        }

        var beatsAhead = (timeToReach * bpm) / 60f;
        var futureBeat = beatEvent.BeatNumber + beatsAhead;

        // Check FFT at current time to gate spawning
        var fftValue = analyzer.GetFFTValue(FrequencyBand);

        // Skip spawning if music is silent (below threshold)
        if (fftValue < Parameters.FFTThreshold)
        {
            // Log every 8th beat to avoid spam
            if (beatEvent.BeatNumber % 8 == 0)
            {
                Debug.Log($"StarSpawner: Beat {beatEvent.BeatNumber} - Skipping spawn (FFT={fftValue:F3} < threshold, silent section)");
            }
            return;
        }

        // Calculate spawn position ahead of camera
        var cameraX = camera.GlobalPosition.X;
        //var spawnX = cameraX + Parameters.StarSpawnDistance;
        //TODO
        var spawnX = cameraX;

        // Spawn star - it will be reached at the calculated future beat
        SpawnStar(spawnX);

        // Log spawn decision (every 8th beat to reduce spam)
        if (beatEvent.BeatNumber % 8 == 0)
        {
            Debug.Log($"StarSpawner: Beat {beatEvent.BeatNumber} - Spawning star (FFT={fftValue:F3}, speed={currentSpeed:F1}, timeToReach={timeToReach:F2}s, futureBeat={futureBeat:F1})");
        }
    }

    private void SpawnStar(float xPosition)
    {
        var starPosition = generator.GenerateStarPosition(xPosition);

        // Skip if position is out of bounds (indicates FFT below threshold)
        if (starPosition.Y is > Parameters.MaxStarHeight or < Parameters.MinStarHeight)
        {
            return;
        }

        var star = starPrefab.Instantiate<Node2D>();
        star.GlobalPosition = starPosition;
        GetParent().AddChild(star);
        activeStars.Add(star);

        // Track X position for meteorite collision avoidance
        recentStarXPositions.Add(xPosition);
    }

    private void DespawnDistantStars(float cameraX)
    {
        activeStars.RemoveAll(star =>
        {
            if (!IsInstanceValid(star))
            {
                return true;
            }

            if (star.GlobalPosition.X < cameraX - Parameters.StarDespawnDistance)
            {
                star.QueueFree();
                return true;
            }

            return false;
        });
    }

    private void CleanupOldStarPositions(float cameraX)
    {
        // Remove star positions that are far behind camera (no longer needed for collision checking)
        recentStarXPositions.RemoveAll(x => x < cameraX - Parameters.StarDespawnDistance);
    }

    /// <summary>
    /// Check if a given X position is too close to any recent star spawn
    /// Used by MeteoriteSpawner for collision avoidance
    /// </summary>
    public bool IsPositionNearStar(float xPosition)
    {
        foreach (var starX in recentStarXPositions)
        {
            if (Mathf.Abs(xPosition - starX) < Parameters.MinStarMeteoriteDistance)
            {
                return true;
            }
        }
        return false;
    }
}
