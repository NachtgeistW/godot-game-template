using Godot;
using Plutono.Scripts.Utils;
using starrynight.scripts.games;
using System.Collections.Generic;

namespace starrynight;

public partial class StarSpawner : Node2D
{
    [Export] public NodePath CameraPath { get; set; }
    [Export] public NodePath AudioPlayerPath { get; set; }
    [Export] public string MidiFilePath { get; set; }

    private Camera2D camera;
    private PlayerController player;
    private AudioStreamPlayer audioPlayer;
    private MidiStarGenerator midiGenerator;
    private PackedScene starPrefab;
    private readonly List<Node2D> activeStars = [];
    private readonly List<float> recentStarXPositions = []; // For meteorite collision avoidance

    public override void _Ready()
    {
        camera = GetNode<Camera2D>(CameraPath);
        player = camera.GetParent<PlayerController>(); // Get player from camera's parent
        audioPlayer = GetNode<AudioStreamPlayer>(AudioPlayerPath);
        starPrefab = GD.Load<PackedScene>("res://prefabs/star.tscn");

        // Initialize AudioAnalyzer singleton for playback position tracking
        if (AudioAnalyzer.Instance == null)
        {
            var analyzer = new AudioAnalyzer();
            AddChild(analyzer);
        }

        AudioAnalyzer.Instance.Initialize(audioPlayer);

        // Initialize MIDI generator
        midiGenerator = new MidiStarGenerator(MidiFilePath);

        Debug.Log($"StarSpawner initialized with MIDI generation from {MidiFilePath}");
    }

    public override void _Process(double delta)
    {
        var cameraX = camera.GlobalPosition.X;

        // Process MIDI-based star spawning
        ProcessMidiSpawning(cameraX);

        DespawnDistantStars(cameraX);
        CleanupOldStarPositions(cameraX);
    }

    /// <summary>
    /// Process MIDI notes and spawn stars based on current playback position
    /// Stars are positioned dynamically so player reaches them exactly at note timestamp
    /// </summary>
    private void ProcessMidiSpawning(float cameraX)
    {
        if (midiGenerator == null || player == null)
        {
            return;
        }

        var analyzer = AudioAnalyzer.Instance;
        if (analyzer == null || !analyzer.IsPlaying())
        {
            return;
        }

        // Get current playback position
        var currentTime = analyzer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency();
        var currentSpeed = player.CurrentSpeed;

        if (currentSpeed <= 0)
        {
            return;
        }

        // Get stars that should spawn now from MIDI generator
        // Stars are positioned based on: playerPosition + speed × (noteTime - currentTime)
        var starsToSpawn = midiGenerator.GetStarsToSpawn(
            currentTime,
            currentSpeed,
            player.GlobalPosition.X,
            cameraX
        );

        // Spawn all stars
        foreach (var starPosition in starsToSpawn)
        {
            SpawnStarAtPosition(starPosition);
        }
    }

    /// <summary>
    /// Spawn a star at the given position
    /// </summary>
    private void SpawnStarAtPosition(Vector2 position)
    {
        var star = starPrefab.Instantiate<Node2D>();
        star.GlobalPosition = position;

        GetParent().AddChild(star);
        activeStars.Add(star);

        // Track X position for meteorite collision avoidance
        recentStarXPositions.Add(position.X);
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