using Godot;
using Plutono.Scripts.Utils;
using System.Collections.Generic;

namespace starrynight;

public partial class StarSpawner : Node2D
{
    [Export] public NodePath CameraPath { get; set; }

    private Camera2D _camera;
    private IStarGenerator _generator;
    private PackedScene _starPrefab;
    private float _nextSpawnX;
    private List<Node2D> _activeStars = new List<Node2D>();

    public override void _Ready()
    {
        _camera = GetNode<Camera2D>(CameraPath);
        _generator = new RandomStarGenerator();
        _starPrefab = GD.Load<PackedScene>("res://prefabs/star.tscn");

        _nextSpawnX = _camera.GlobalPosition.X + Parameters.StarSpawnDistance;
    }

    public override void _Process(double delta)
    {
        var cameraX = _camera.GlobalPosition.X;

        while (cameraX + Parameters.StarSpawnDistance >= _nextSpawnX)
        {
            SpawnStar(_nextSpawnX);
            _nextSpawnX += Parameters.StarSpawnInterval;
        }

        DespawnDistantStars(cameraX);
    }

    private void SpawnStar(float xPosition)
    {
        var starPosition = _generator.GenerateStarPosition(xPosition);
        var star = _starPrefab.Instantiate<Node2D>();
        star.GlobalPosition = starPosition;
        GetParent().AddChild(star);
        _activeStars.Add(star);
    }

    private void DespawnDistantStars(float cameraX)
    {
        _activeStars.RemoveAll(star =>
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
