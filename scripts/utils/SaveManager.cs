using Godot;

namespace Plutono.Scripts.Utils;

public partial class SaveManager : Node
{
	public static SaveManager Instance { get; private set; }

	private const string SavePath = "user://savegame.cfg";
	private const string SectionPlayer = "player";
	private const string KeyTotalStars = "total_stars";

	private ConfigFile config;

	public override void _Ready()
	{
		Instance = this;
		config = new ConfigFile();
	}

	public void SaveTotalStars(int stars)
	{
		config.SetValue(SectionPlayer, KeyTotalStars, stars);
		var error = config.Save(SavePath);

		if (error != Error.Ok)
		{
			GD.PushError($"Failed to save game: {error}");
		}
		else
		{
			Debug.Log($"Game saved: total stars = {stars}");
		}
	}

	public int LoadTotalStars()
	{
		var error = config.Load(SavePath);

		if (error != Error.Ok)
		{
			return 0;
		}

		var totalStars = config.GetValue(SectionPlayer, KeyTotalStars, 0).AsInt32();
		return totalStars;
	}

	public bool SaveExists()
	{
		return FileAccess.FileExists(SavePath);
	}

	public void DeleteSave()
	{
		if (SaveExists())
		{
			DirAccess.RemoveAbsolute(SavePath);
			Debug.Log("Save file deleted");
		}
	}
}
