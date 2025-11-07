using Godot;
using Plutono.Scripts.Utils;

namespace starrynight.scripts.games;

public partial class PlayerController : CharacterBody2D
{
	[Export] public float MoveSpeed = 5f;

	private Vector2 _targetPosition;
	private Platform _platform;
	
	[Export] public float MinHeight = -50f;
	[Export] public float MaxHeight = 50f;
	
	[Export] public float InitialSpeed = 150f;
	[Export] public float MaxSpeed = 300f;
	[Export] public float Acceleration = 5f;
	private float currentSpeed;
	public override void _Ready()
	{
		_targetPosition = GlobalPosition;
		_platform = OsDetector.Platform;
		
		currentSpeed = InitialSpeed;
	}

	public override void _Process(double delta)
	{
		if (_platform == Platform.PC)
		{
			_targetPosition.Y = GetGlobalMousePosition().Y;
			if (_targetPosition.Y > MaxHeight) 
				_targetPosition.Y = MaxHeight;
			if (_targetPosition.Y < MinHeight)
				_targetPosition.Y = MinHeight;
		}
		
		if (currentSpeed < MaxSpeed)
		{
			currentSpeed += Acceleration * (float)delta;
			currentSpeed = Mathf.Min(currentSpeed, MaxSpeed);
		}

		GlobalPosition = GlobalPosition.Lerp(_targetPosition, MoveSpeed * (float)delta);
		GlobalPosition += new Vector2(currentSpeed * (float)delta, 0);
	}

	public override void _Input(InputEvent @event)
	{
		if (_platform is Platform.Android or Platform.iOS)
		{
			Vector2? touchPosition = null;

			if (@event is InputEventScreenDrag dragEvent)
			{
				touchPosition = dragEvent.Position;
			}

			if (touchPosition.HasValue)
			{
				var globalTouchPosition = GetViewport().GetCamera2D().GetScreenCenterPosition() +
				                          (touchPosition.Value - GetViewport().GetVisibleRect().Size / 2) /
				                          GetViewport().GetCamera2D().Zoom;

				_targetPosition.Y = globalTouchPosition.Y;
			}
		}
	}
}