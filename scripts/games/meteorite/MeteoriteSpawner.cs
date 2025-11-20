using System;
using Godot;
using Plutono.Scripts.Utils;
using System.Collections.Generic;

namespace StarfallNight;

public partial class MeteoriteSpawner : Node2D
{
    [Export] private Camera2D Camera { get; set; }
    [Export] private StarSpawner StarSpawner { get; set; }
    [Export] private PackedScene MeteoritePrefab { get; set; }
    private RandomStarGenerator generator;    
    
    private float nextSpawnX;
    private readonly List<Node2D> activeMeteorites = [];

    private float elapsedTime;
    private float currentSpawnInterval;

    private Random random;

    public override void _Ready()
    {
        generator = new RandomStarGenerator();

        currentSpawnInterval = Parameters.MeteoriteInitialSpawnInterval;
        nextSpawnX = Camera.GlobalPosition.X + Parameters.MeteoriteSpawnDistance;
        elapsedTime = 0f;

        random = new Random();
    }

    public override void _Process(double delta)
    {
        elapsedTime += (float)delta;
        UpdateDifficulty();

        var cameraX = Camera.GlobalPosition.X;

        while (cameraX + Parameters.MeteoriteSpawnDistance >= nextSpawnX)
        {
            if (StarSpawner != null && StarSpawner.IsPositionNearStar(nextSpawnX))
            {
                nextSpawnX += Parameters.MinStarMeteoriteDistance * 0.5f;
                continue;
            }

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
        var meteorite = MeteoritePrefab.Instantiate<Node2D>();
        meteorite.GlobalPosition = meteoritePosition;

        var sprite2D = meteorite.GetNode<Sprite2D>("Sprite2D");
        sprite2D.Frame = random.Next(0, 5);
        
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
