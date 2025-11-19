using Godot;
using Plutono.Scripts.Utils;
using System.Collections.Generic;

namespace starrynight;

/// <summary>
/// Manages MIDI note data and provides star generation based on playback position
/// Unlike FFT generator, this pre-loads all notes and spawns based on timing
/// </summary>
public class MidiStarGenerator
{
    private readonly List<MidiNoteData> notes;
    private readonly HashSet<int> spawnedNoteIndices = new();
    private readonly RandomNumberGenerator random = new();

    /// <summary>
    /// Create MIDI star generator from parsed MIDI file
    /// </summary>
    /// <param name="midiFilePath">Path to MIDI file (res:// path)</param>
    public MidiStarGenerator(string midiFilePath)
    {
        var parser = new MidiNoteParser(midiFilePath);
        notes = parser.GetNotes();

        Debug.Log($"MidiStarGenerator initialized with {notes.Count} notes");
        Debug.Log($"Duration: {parser.GetTotalDuration():F2} seconds");
    }

    /// <summary>
    /// Get notes that should be spawned now based on current playback position and player state
    /// Stars are positioned so that player reaches them exactly at the note's timestamp
    /// </summary>
    /// <param name="currentTime">Current playback position in seconds</param>
    /// <param name="playerSpeed">Current player speed (units per second)</param>
    /// <param name="playerXPosition">Current player X position</param>
    /// <param name="cameraXPosition">Current camera X position</param>
    /// <returns>List of positions where stars should spawn</returns>
    public List<Vector2> GetStarsToSpawn(
        double currentTime,
        float playerSpeed,
        float playerXPosition,
        float cameraXPosition)
    {
        var starsToSpawn = new List<Vector2>();

        if (playerSpeed <= 0)
        {
            return starsToSpawn;
        }
        
        // Spawn window: stars should appear when they're just entering the screen (130-180 relative to camera)
        const float spawnWindowMin = 130f; // Start spawning slightly before screen edge
        const float spawnWindowMax = 180f; // End spawning slightly after (off-screen)

        // Find notes that should be spawned now
        for (var i = 0; i < notes.Count; i++)
        {
            // Skip already spawned notes
            if (spawnedNoteIndices.Contains(i))
            {
                continue;
            }

            var note = notes[i];
            var timeUntilNote = note.TimeInSeconds - currentTime;
            if (timeUntilNote < 0)
            {
                spawnedNoteIndices.Add(i);
                Debug.LogWarning($"Missed note at {note.TimeInSeconds:F3}s (already passed, current={currentTime:F3}s)");
                continue;
            }
            
            var idealStarX = playerXPosition + playerSpeed * (float)timeUntilNote;
            var relativeToCamera = idealStarX - cameraXPosition;
            if (relativeToCamera is >= spawnWindowMin and <= spawnWindowMax)
            {
                var yPosition = random.RandfRange(Parameters.MinStarHeight, Parameters.MaxStarHeight);
                //var yPosition = 0;
                var starPosition = new Vector2(idealStarX, yPosition);

                starsToSpawn.Add(starPosition);
                spawnedNoteIndices.Add(i);
            }
        }

        return starsToSpawn;
    }

    /// <summary>
    /// Reset spawned notes tracking (for looping or restart)
    /// </summary>
    public void Reset()
    {
        spawnedNoteIndices.Clear();
        Debug.Log("MidiStarGenerator reset");
    }

    /// <summary>
    /// Get total number of notes
    /// </summary>
    public int GetNoteCount() => notes.Count;

    /// <summary>
    /// Get number of spawned notes
    /// </summary>
    public int GetSpawnedCount() => spawnedNoteIndices.Count;
}
