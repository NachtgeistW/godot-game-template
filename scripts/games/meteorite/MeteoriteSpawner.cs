using System;
using Godot;
using Plutono.Scripts.Utils;
using System.Collections.Generic;

namespace starrynight;

public partial class MeteoriteSpawner : Node2D
{
    [Export] public NodePath CameraPath { get; set; }

    private Camera2D camera;
    private IStarGenerator generator;
    private PackedScene meteoritePrefab;
    private float nextSpawnX;
    private readonly List<Node2D> activeMeteorites = [];

    private float elapsedTime;
    private float currentSpawnInterval;
    
    private Random random;

    public override void _Ready()
    {
        camera = GetNode<Camera2D>(CameraPath);
        generator = new RandomStarGenerator();
        meteoritePrefab = GD.Load<PackedScene>("res://prefabs/meteorite.tscn");

        currentSpawnInterval = Parameters.MeteoriteInitialSpawnInterval;
        nextSpawnX = camera.GlobalPosition.X + Parameters.MeteoriteSpawnDistance;
        elapsedTime = 0f;
        
        random = new Random();
    }

    public override void _Process(double delta)
    {
        elapsedTime += (float)delta;
        UpdateDifficulty();

        var cameraX = camera.GlobalPosition.X;

        while (cameraX + Parameters.MeteoriteSpawnDistance >= nextSpawnX)
        {
            SpawnMeteorite(nextSpawnX);
            var randomDistance = random.Next((int)-Parameters.MeteoriteSpawnDistance, (int)Parameters.MeteoriteSpawnDistance);
            nextSpawnX += currentSpawnInterval + randomDistance;
        }

        DespawnDistantMeteorites(cameraX);
    }

    private void UpdateDifficulty()
    {
        var difficultyLevel = Mathf.FloorToInt(elapsedTime / Parameters.MeteoriteDifficultyIncreaseTime);
        var decreaseAmount = difficultyLevel * 50f;
        currentSpawnInterval = Mathf.Max(
            Parameters.MeteoriteMinSpawnInterval,
            Parameters.MeteoriteInitialSpawnInterval - decreaseAmount
        );
    }

    private void SpawnMeteorite(float xPosition)
    {
        var meteoritePosition = generator.GenerateStarPosition(xPosition);
        var meteorite = meteoritePrefab.Instantiate<Node2D>();
        meteorite.GlobalPosition = meteoritePosition;
        GetParent().AddChild(meteorite);
        activeMeteorites.Add(meteorite);
    }

    private void DespawnDistantMeteorites(float cameraX)
    {
        activeMeteorites.RemoveAll(meteorite =>
        {
            if (!IsInstanceValid(meteorite))
            {
                return true;
            }

            if (meteorite.GlobalPosition.X < cameraX - Parameters.MeteoriteDespawnDistance)
            {
                meteorite.QueueFree();
                return true;
            }

            return false;
        });
    }
}
