using Godot;
using Plutono.Scripts.Utils;
using Plutono.Util;
using starrynight;

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

    /// <summary>
    /// Current forward speed of the player
    /// </summary>
    public float CurrentSpeed { get; private set; }

    private int currentHealth;

    public override void _Ready()
    {
        _targetPosition = GlobalPosition;
        _platform = OsDetector.Platform;

        CurrentSpeed = InitialSpeed;
        currentHealth = Parameters.MaxHealth;

        EventCenter.AddListener<MeteoriteHitEvent>(OnMeteoriteHit);
        EventCenter.Broadcast(new PlayerHealthChangedEvent(currentHealth));
    }

    private void OnMeteoriteHit(MeteoriteHitEvent evt)
    {
        TakeDamage(Parameters.MeteoriteDamage);
    }

    private void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        EventCenter.Broadcast(new PlayerHealthChangedEvent(currentHealth));

        if (currentHealth <= 0)
        {
            EventCenter.Broadcast(new GameOverEvent());
        }
    }

    public override void _ExitTree()
    {
        EventCenter.RemoveListener<MeteoriteHitEvent>(OnMeteoriteHit);
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
            if (CurrentSpeed < MaxSpeed)
            {
                CurrentSpeed += Acceleration * (float)delta;
                CurrentSpeed = Mathf.Min(CurrentSpeed, MaxSpeed);
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
                GlobalPosition += new Vector2(CurrentSpeed * (float)delta, 0);
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