# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Starry Night** (繁星降落的夜晚) is a Godot 4.5 game project using C# scripting. The project targets mobile platforms (Android/iOS) with cross-platform support.

## Build & Development Commands

### Running the Project
- Open project in Godot Editor: Launch `project.godot` in Godot 4.5
- Run from editor: Press F5 or click the Play button in Godot Editor

### Building for Android
- Export via Godot Editor: Project → Export → Android
- Output: `./Starry Night.apk`
- Uses Gradle build system
- Target architecture: arm64-v8a only
- Android export configured in `export_presets.cfg`

### C# Development
- Target Framework: .NET 8.0 (desktop), .NET 9.0 (Android)
- Assembly name: `starry-night`
- Root namespace: `starrynight`
- Build via Godot Editor (automatic C# compilation)
- The `android/` directory is excluded from compilation

## Architecture

### Event System
The project uses a custom event-driven architecture centered around `EventCenter`:

- **EventCenter** (`scripts/utils/event/EventCenter.cs`): Central event bus using type-safe events
  - Events must implement `IEvent` interface (struct constraint)
  - Methods: `AddListener<T>()`, `RemoveListener<T>()`, `Broadcast<T>()`
  - Uses Dictionary of LinkedLists to store handlers by event type
  - Located in `Plutono.Util` namespace

- **Usage pattern**:
  ```csharp
  // Define event
  public struct MyEvent : IEvent { }

  // Subscribe
  EventCenter.AddListener<MyEvent>(OnMyEvent);

  // Broadcast
  EventCenter.Broadcast(new MyEvent());
  ```

### Utility Systems

- **Debug** (`scripts/utils/Debug.cs`): Conditional debug logging wrapper
  - Only logs in DEBUG builds using `[Conditional("DEBUG")]`
  - Methods: `Log()`, `LogWarning()`, `LogError()`
  - Wraps Godot's `GD.Print()`, `GD.PushWarning()`, `GD.PushError()`

- **OsDetector** (`scripts/utils/OsDetector.cs`): Platform detection singleton
  - Detects platform at runtime: PC, Android, iOS, Web, GenericDevice
  - Access via `OsDetector.Platform` property
  - Initializes in `_Ready()` method

- **Parameters** (`scripts/utils/Parameters.cs`): Global game constants (currently empty/commented)

- **Singleton** (`scripts/utils/Singleton.cs`): Generic singleton pattern (currently commented out, not in use)

### Project Structure

```
scripts/
├── games/              # Game-specific logic
│   
└── utils/              # Shared utilities
    ├── event/          # Event system
    │   ├── EventCenter.cs
    │   └── IEvent.cs
    ├── Debug.cs
    ├── OsDetector.cs
    ├── Parameters.cs
    ├── Singleton.cs
    └── Enums.cs
```

### Namespaces
- Primary namespace: `Plutono.Util` (for event system)
- Utilities namespace: `Plutono.Scripts.Utils`
- Game namespace: `starrynight` (root namespace)

### Godot-Specific Notes
- Uses C# partial classes for Godot nodes (e.g., `public partial class PlayerController : Node`)
- Scene files: `.tscn` (prefabs and scenes)
- Main scene: Referenced as `uid://d4fuuhpybpvnp` in project.godot
- Rendering: Mobile renderer, nearest-neighbor texture filtering (pixel art)
- Localization: Configured for Chinese (CN) with locale filtering

### Android Build Details
- Min SDK/Target SDK: Not explicitly set (uses Godot defaults)
- Immersive mode enabled
- Debug symbols included in .NET builds
- Package: `com.example.$genname`
- Mono build artifacts stored in `android/build/` (excluded from git)

## Important Conventions

- Event structs should be defined as `public struct EventName : IEvent`
- All Godot node classes must be `partial` classes
- Debug logging should use the `Debug` class wrapper, not `GD` directly
- Platform detection should use `OsDetector.Platform`, not direct OS calls
- Use `var` as much as possible
- Write commit as less as possible expected for it's necessary as the code should clear enough to explains itself
