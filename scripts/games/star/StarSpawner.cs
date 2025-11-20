using Godot;
using Plutono.Scripts.Utils;
using System.Collections.Generic;
using System.Linq;
using StarfallNight.scripts.games;

namespace StarfallNight;

public partial class StarSpawner : Node2D
{
    [Export] private Camera2D Camera { get; set; }
    [Export] private AudioStreamPlayer AudioPlayer { get; set; }
    [Export] private PlayerController Player { get; set; }
    [Export] public string MidiFilePath { get; set; }
    [Export] private PackedScene StarPrefab { get; set; }
    
    private MidiStarGenerator midiGenerator;
    private readonly List<Node2D> activeStars = [];
    private readonly List<float> recentStarXPositions = []; // For meteorite collision avoidance
    private double lastPlaybackPosition; // Track position for loop detection

    public override void _Ready()
    {
        midiGenerator = new MidiStarGenerator(MidiFilePath);
    }

    public override void _Process(double delta)
    {
        var cameraX = Camera.GlobalPosition.X;
        ProcessMidiSpawning(cameraX);
        DespawnDistantStars(cameraX);
        CleanupOldStarPositions(cameraX);
    }

    private void ProcessMidiSpawning(float cameraX)
    {
        if (midiGenerator == null || Player == null)
        {
            return;
        }

        var currentSpeed = Player.CurrentSpeed;
        if (currentSpeed <= 0)
        {
            return;
        }
        
        var currentTime = AudioPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() -
                          AudioServer.GetOutputLatency();
        if (currentTime < lastPlaybackPosition - 1f)
        {
            midiGenerator.Reset();
        }
        lastPlaybackPosition = currentTime;
        
        var starsToSpawn = midiGenerator.GetStarsToSpawn(currentTime, currentSpeed, Player.GlobalPosition.X, cameraX);
        foreach (var starPosition in starsToSpawn)
        {
            SpawnStarAtPosition(starPosition);
        }
    }

    private void SpawnStarAtPosition(Vector2 position)
    {
        var star = StarPrefab.Instantiate<Node2D>();
        star.GlobalPosition = position;

        GetParent().AddChild(star);
        activeStars.Add(star);

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
        recentStarXPositions.RemoveAll(x => x < cameraX - Parameters.StarDespawnDistance);
    }
    
    public bool IsPositionNearStar(float xPosition)
    {
        return recentStarXPositions.Any(starX => Mathf.Abs(xPosition - starX) < Parameters.MinStarMeteoriteDistance);
    }
}