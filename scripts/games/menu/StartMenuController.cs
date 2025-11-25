using Godot;

namespace StarfallNight;

public partial class StartMenuController : Node2D
{
    [Export] private Label titleLabel;
    [Export] private Button startButton;
    [Export] private Button quitButton;

    public override void _Ready()
    {
        titleLabel = GetNode<Label>("UILayer/Control/VBoxContainer/TitleLabel");
        startButton = GetNode<Button>("UILayer/Control/VBoxContainer/ButtonContainer/StartButton");
        quitButton = GetNode<Button>("UILayer/Control/VBoxContainer/ButtonContainer/QuitButton");

        titleLabel.Text = Tr("GAME_TITLE");
        startButton.Text = Tr("START_GAME");
        quitButton.Text = Tr("QUIT_GAME");

        startButton.Pressed += OnStartPressed;
        quitButton.Pressed += OnQuitPressed;
    }

    private void OnStartPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/game.tscn");
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }

    public override void _ExitTree()
    {
        startButton.Pressed -= OnStartPressed;
        quitButton.Pressed -= OnQuitPressed;
    }
}
