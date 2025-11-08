using Godot;
using Plutono.Scripts.Utils;

namespace starrynight.scripts.games;

public partial class PlayerController : CharacterBody2D
{
    [Export] public float MoveSpeed = 5f;

    private Vector2 _targetPosition;
    private Platform _platform;

    [Export] public float MinHeight = -90f;
    [Export] public float MaxHeight = 90f;

    [Export] public float InitialSpeed = 150f;
    [Export] public float MaxSpeed = 300f;
    [Export] public float Acceleration = 1f;
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
            UpdateTargetPositionOnPc();
        }

        IncreaseSpeed();
        UpdatePlayerPosition();
        
        return;

        void UpdateTargetPositionOnPc()
        {
            _targetPosition.Y = GetGlobalMousePosition().Y;
            if (_targetPosition.Y > MaxHeight)
                _targetPosition.Y = MaxHeight;
            if (_targetPosition.Y < MinHeight)
                _targetPosition.Y = MinHeight;
            _targetPosition.X = GlobalPosition.X;
        }

        void IncreaseSpeed()
        {
            if (currentSpeed < MaxSpeed)
            {
                currentSpeed += Acceleration * (float)delta;
                currentSpeed = Mathf.Min(currentSpeed, MaxSpeed);
            }
        }

        void UpdatePlayerPosition()
        {
            LerpYAxisPosition();
            IncreaseXAxisSpeed();
            return;

            void LerpYAxisPosition()
            {
                GlobalPosition = GlobalPosition.Lerp(_targetPosition, MoveSpeed * (float)delta);
            }

            void IncreaseXAxisSpeed()
            {
                GlobalPosition += new Vector2(currentSpeed * (float)delta, 0);
            }
        }
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

                UpdateTargetPositionOnMobile(globalTouchPosition);
            }
        }

        return;

        void UpdateTargetPositionOnMobile(Vector2 globalTouchPosition)
        {
            _targetPosition.Y = globalTouchPosition.Y;
            if (_targetPosition.Y > MaxHeight)
                _targetPosition.Y = MaxHeight;
            if (_targetPosition.Y < MinHeight)
                _targetPosition.Y = MinHeight;
            _targetPosition.X = globalTouchPosition.X;
        }
    }
}