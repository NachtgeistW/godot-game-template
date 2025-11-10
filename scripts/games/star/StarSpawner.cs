using Godot;
using Plutono.Scripts.Utils;
using System.Collections.Generic;

namespace starrynight;

public partial class StarSpawner : Node2D
{
    [Export] public NodePath CameraPath { get; set; }

    private Camera2D camera;
    private IStarGenerator generator;
    private PackedScene starPrefab;
    private float nextSpawnX;
    private readonly List<Node2D> activeStars = [];

    public override void _Ready()
    {
        camera = GetNode<Camera2D>(CameraPath);
        generator = new RandomStarGenerator();
        starPrefab = GD.Load<PackedScene>("res://prefabs/star.tscn");

        nextSpawnX = camera.GlobalPosition.X + Parameters.StarSpawnDistance;
    }

    public override void _Process(double delta)
    {
        var cameraX = camera.GlobalPosition.X;

        while (cameraX + Parameters.StarSpawnDistance >= nextSpawnX)
        {
            SpawnStar(nextSpawnX);
            nextSpawnX += Parameters.StarSpawnInterval;
        }

        DespawnDistantStars(cameraX);
    }

    private void SpawnStar(float xPosition)
    {
        var starPosition = generator.GenerateStarPosition(xPosition);
        var star = starPrefab.Instantiate<Node2D>();
        star.GlobalPosition = starPosition;
        GetParent().AddChild(star);
        activeStars.Add(star);
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
}
