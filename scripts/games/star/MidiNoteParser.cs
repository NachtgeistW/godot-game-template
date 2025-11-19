using Godot;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Plutono.Scripts.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace starrynight;

/// <summary>
/// Represents a parsed MIDI note with timing information
/// </summary>
public struct MidiNoteData(double time, int pitch, double duration)
{
    public readonly double TimeInSeconds = time;
    public readonly int Pitch = pitch;
    public readonly double DurationInSeconds = duration;
}

/// <summary>
/// Parses MIDI files and extracts notes for star generation
/// Filters notes based on duration (only keeps notes <= 1/8 note length)
/// </summary>
public class MidiNoteParser
{
    private readonly List<MidiNoteData> notes = new();
    private readonly TempoMap tempoMap;

    /// <summary>
    /// Parse MIDI file and extract filtered notes
    /// </summary>
    /// <param name="midiFilePath">Path to MIDI file (Godot res:// path)</param>
    public MidiNoteParser(string midiFilePath)
    {
        using var file = Godot.FileAccess.Open(midiFilePath, Godot.FileAccess.ModeFlags.Read);
        if (file == null)
        {
            var error = Godot.FileAccess.GetOpenError();
            GD.PushError($"Failed to open MIDI file: {midiFilePath}, Error: {error}");
            throw new System.Exception($"Failed to open MIDI file: {midiFilePath}, Error: {error}");
        }

        // Read file into memory and parse with DryWetMidi
        var bytes = file.GetBuffer((long)file.GetLength());
        using var memoryStream = new MemoryStream(bytes);
        var midiFile = MidiFile.Read(memoryStream);
        tempoMap = midiFile.GetTempoMap();
        var allNotes = midiFile.GetNotes();
        Debug.Log($"Total notes found: {allNotes.Count}");

        // Calculate quarter note duration at the first tempo
        var quarterNoteDuration = GetQuarterNoteDurationInSeconds();
        var eighthNoteDuration = quarterNoteDuration / 2.0;

        Debug.Log($"Quarter note duration: {quarterNoteDuration:F3}s");
        Debug.Log($"Eighth note duration (filter threshold): {eighthNoteDuration:F3}s");
        
        // Convert notes to our data structure and filter
        var filteredCount = 0;
        foreach (var note in allNotes)
        {
            var timeSpan = note.TimeAs<MetricTimeSpan>(tempoMap);
            var timeInSeconds = timeSpan.TotalMicroseconds / 1_000_000.0;
        
            var lengthSpan = note.LengthAs<MetricTimeSpan>(tempoMap);
            var durationInSeconds = lengthSpan.TotalMicroseconds / 1_000_000.0;
        
            // Filter: only keep notes with duration <= 1/8 note
            //if (durationInSeconds <= eighthNoteDuration)
            //{
                notes.Add(new MidiNoteData(timeInSeconds, note.NoteNumber, durationInSeconds));
                //filteredCount++;
            //}
        }
        filteredCount = allNotes.Count;

        // Sort by time
        notes.Sort((a, b) => a.TimeInSeconds.CompareTo(b.TimeInSeconds));

        Debug.Log($"Notes after filtering (duration <= 1/8 note): {filteredCount}");
        Debug.Log($"Note time range: {notes.FirstOrDefault().TimeInSeconds:F2}s to {notes.LastOrDefault().TimeInSeconds:F2}s");
    }

    /// <summary>
    /// Get all parsed notes
    /// </summary>
    public List<MidiNoteData> GetNotes() => notes;

    /// <summary>
    /// Get duration of a quarter note in seconds based on tempo
    /// </summary>
    private double GetQuarterNoteDurationInSeconds()
    {
        // Get tempo at time zero (start of song)
        var tempo = tempoMap.GetTempoAtTime((MidiTimeSpan)0);

        // Tempo is in microseconds per quarter note
        // Convert to seconds per quarter note
        return tempo.MicrosecondsPerQuarterNote / 1_000_000.0;
    }

    /// <summary>
    /// Get total duration of the MIDI file in seconds
    /// </summary>
    public double GetTotalDuration()
    {
        if (notes.Count == 0) return 0;

        // Return time of last note + its duration
        var lastNote = notes[^1];
        return lastNote.TimeInSeconds + lastNote.DurationInSeconds;
    }
}
