# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RaidClimb is a mobile PvP tower-defense/castle-defense game built with **Unity 6000.3.10f1** targeting Android. Players simultaneously defend their own castle and attack their opponent's castle in real-time. The project outputs an APK and there is a single scene (`MainScene`).

## How to Build & Run

- **Engine required:** Unity 6000.3.10f1 (IL2CPP, .NET Standard 2.1, C# 9.0)
- **Open project:** Load the root folder in Unity Hub, then open `Assets/Scenes/MainScene.unity`
- **Play in Editor:** Press the Play button in Unity Editor
- **Build APK:** File → Build Settings → Android → Build (or Build And Run)
- **Edit scripts:** Open `RaidClimb.sln` in JetBrains Rider or Visual Studio

There is no CLI build system, test framework, or `package.json`. All build/run operations go through the Unity Editor.

## Architecture

### Manager System

`GameManager` (singleton, `Assets/Scripts/GameManager.cs`) is the central orchestrator. It owns a `Dictionary<Type, IManager>` and initializes all subsystems concurrently via `Task.WhenAll` on startup.

All managers implement `IManager`:
```csharp
interface IManager {
    Task Init(object[] args);
    void Cleanup();
}
```

Access any manager from anywhere: `GameManager.Instance.GetManager<T>()`.

### Core Managers (`Assets/Scripts/Managers/`)

| Manager | Responsibility |
|---|---|
| `GameStateManager` | State machine: NotStarted → InGame → GameEnded |
| `UIManager` | Screen stack navigation |
| `UnitManager` | Spawning, tracking, and cleanup of all combat units |
| `CastleManager` / `PlayerCastleManager` / `OpponentCastleManager` | Castle slot layout, unit/trap placement, purchasing |
| `InputManager` | Touch input: press → hold/drag → release, emits `OnHoldRight`/`OnReleaseRight` events |
| `CameraManager` | Split-screen camera positioning (player vs. opponent views) |
| `TrapsManager` | Trap placement and trigger logic |
| `CurrencyManager` | Coins and trophies |
| `OpponentManager` | Opponent AI behaviour |
| `DataManager` | JSON save/load via Newtonsoft.Json to `Application.persistentDataPath/SaveData.txt` |

### Unit Hierarchy (`Assets/Scripts/Units/`)

`BaseUnit` (abstract) → `MeleeUnit`, `RangeUnit`, `TankUnit`, `DefenderUnit`, `KingUnit`

Each unit is composed of:
- `UnitHealthController` – health bars, damage intake
- `UnitVisualsController` – animation state machine
- `UnitTargetController` – target selection / attack range
- `DissolveController` – death dissolve effect

Unit stats come from `UnitBaseConfig` ScriptableObjects; prefab references from `UnitReferences`.

### UI (`Assets/Scripts/UI/`)

All screens extend `BaseScreen`. Active screens: `MainScreenUI`, `InGameScreen`, `GameEndScreen`, `SettingsScreen`, `LeaderboardScreen`, `CollectionScreen`, `TowerScreen`.

### Game Loop

1. `MainScreen` shown on launch
2. "Play" → `GameManager.StartGame()` → camera transitions, `InGameScreen` activates
3. Units spawn and fight; `KingUnit` death triggers game end
4. `GameEndScreen` → "Finish" → back to `MainScreen`

### Data Persistence

`PlayerData.cs` defines serializable POCOs (`UserData`, `CastleData`, `CastleSlot`). Saved as JSON after a win or explicit save call.

### Key Constants

Player/opponent IDs and spawn heights live in `Assets/Scripts/Constants/Keys.cs`.

## ClaudeContext Reading Strategy

Always read `ClaudeContext/ClaudeStartContext.md` at session start.
Read other ClaudeContext files only when directly relevant to the task:
- `UnitTrapReference.md` — only when implementing/modifying units or traps
- `OnboardingAndUIChanges.md` — only when working on onboarding or UI flow
- `BotDifficultyIdeas.md` — only when working on bot AI or difficulty
- `ProgressionIdeas.md` — only when working on economy or progression
- `instructions.md` — working rules and reading strategy

## Conventions

- Singleton access: `GameManager.Instance`
- Async init pattern: managers return `Task` from `Init()`; never block the main thread
- Partial classes used for large units (e.g., `BaseUnit` + `BaseUnit.UnitHandlers.cs`)
- ScriptableObjects for data-driven config (unit stats, prefab references)
