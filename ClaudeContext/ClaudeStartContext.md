# RaidClimb — Claude Start Context
**Game Title:** Raid, Climb & Selfie (aka Snap Squad)
**GDD Version:** 1.1 | April 2026

Read this file at the start of every session.
Read `ClaudeContext/UnitTrapReference.md` only when the task involves implementing or modifying units or traps.

---

## Project Overview

- **Engine:** Unity 6000.3.10f1, Android (iOS future), IL2CPP, .NET Standard 2.1, C# 9.0
- **Single scene:** `Assets/Scenes/MainScene.unity`
- **No CLI build system** — everything through Unity Editor
- **MCP:** UnityMCP connected and active
- **Mode:** PvP vs Bot only (offline-capable)
- **Session length:** 2–4 minutes

---

## Core Design Pillars

- **One-handed, one-button** — entire match is a single tap-and-hold gesture
- **Win condition:** Get a unit to the enemy King's SphereCollider ("snap a selfie")

---

## Unit System

### UnitTypes Enum
```
None, Melee, Ranged, Tank, King, Defender, Hunter, Raider, Ogre, Golem, Deadeye, Berserk, TeslaCoil, KingCobra
```
All types 1–13 have prefabs registered in `UnitReferences`. Swat/Sapper not in enum yet.

### Class Hierarchy (`Assets/Scripts/Units/`)
```
BaseUnit (abstract, partial)
├── MeleeUnit     — Attack anim: "Sword"
├── RangeUnit     — Attack anim: "Arrow", _attackRotationOffset = (0,60,0)
├── TankUnit      — Attack anim: "Sword"
├── KingUnit      — Win condition; SphereCollider; always Idle
└── DefenderUnit  — Static; triggers on enemy SphereCollider enter
```

### UnitState Machine
```
Idle → Moving → Attacking
              ↘ Climbing → (1s DelayedTarget) → Moving
Defending (Defenders only)
Dead (2s delay → onUnitDeath callback)
Won / Lost (terminal)
```

### Concrete Unit Init Pattern
```csharp
public override void Init(string playerId, UnitState startState, Action onUnitDeath)
{
    unitType = UnitTypes.Melee;
    Attack = Animator.StringToHash("AnimTriggerName");
    base.Init(playerId, startState, onUnitDeath);
}
```

---

## Spawning Flow

**Player:** `InGameScreen` → 3 `Button` components → `OnUnitButtonPressed(index)` → squad cost check → `SpawnUnit` → 1.5s coroutine → `FindNewTargetFor`

**Bot:** `OpponentManager.PlayingLoop()` → 3s initial delay → loop: pick unit from tier → `SpawnUnit` → `GetDelayForUnit` (charge simulation) → `GetCurrentSpawnInterval` (ramping gap) → repeat. Tier selected by current arena from `BotDifficultyConfig` SO.

`FindNewTargetFor` (opponent): searches `PlayerUnits` for `IsDefender || King` only.

---

## Castle / Tower System

Three levels (Stage 1–3) + King pinnacle. Floor and wall slots for traps per level.
`PlayerCastleManager` handles purchase flow and spawns on game start.
`OpponentCastleManager` populated from `CastleDataByLevel.GetCastleDataForLevel(level)` — only levels 1 and 5 defined.

---

## Data Persistence

Save: `Application.persistentDataPath/SaveData.txt` (Newtonsoft.Json via `DataManager`).
Default new game: 9999 coins, King + 1 Defender.
`UserData`: `UserLevel, coins, trophies, gamesPlayed, demotionShieldCharges, UserName, isFirstWin`
`OnboardingData`: `CompletedSteps` list (7 steps, 0–6).

---

## Economy / Arena

- 15 arenas by trophy count. `EconomyConfig` SO drives thresholds and coin rewards.
- Coins from winning only (no loss on defeat). All 25 castle slots purchasable from day 1.
- `demotionShieldChargesOnPromotion = 1` — GDD says 3. Known mismatch.

---

## Key Files

| File | Purpose |
|---|---|
| `Assets/Scripts/Units/UnitTypes/BaseUnit.cs` | Abstract base, state machine |
| `Assets/Scripts/Units/UnitTypes/BaseUnit.UnitHanlders.cs` | Update/trigger/climb logic |
| `Assets/Scripts/Config/UnitBaseConfig.cs` | Stats ScriptableObject |
| `Assets/Scripts/Config/UnitReferences.cs` | Prefab registry |
| `Assets/Scripts/Managers/UnitManager.cs` | Spawn, find target, death, retarget |
| `Assets/Scripts/Managers/OpponentManager.cs` | Bot spawn loop + difficulty ramp |
| `Assets/Scripts/UI/InGameScreen.cs` | Player buttons, squad meter |
| `Assets/Scripts/Castles/CastleDataByLevel.cs` | Bot castle presets per level |
| `Assets/Scripts/Data/PlayerData.cs` | Save data structure, CastleSlotId enum |
| `Assets/Scripts/Castles/PlayerCastleManager.cs` | Purchase flow |
| `Assets/Scripts/GameManager.cs` | Central orchestrator, game flow |

---

## Known TODOs / Gaps

| Area | Status |
|---|---|
| Squad power system (max 10, unit costs) | NOT implemented |
| Arena-based unit unlocks | NOT implemented — all units accessible |
| Unit levelling (5 levels per unit) | NOT implemented |
| Squad selection screen | `CollectionScreen` is empty shell |
| Demotion shield = 3 matches | Code has = 1 |
| Tournament system | NOT implemented |
| Wall traps (Sawer, Shocker, Hazer, Pusher) | Not implemented — only Spikes + ThornHedge exist |
| Turret traps (Slingshot, King Cobra, Tesla Coil) | Not implemented |
| Lava, Tar Pit ground traps | Not implemented |
| `DefenderUnit.OnTriggerExit` | Body is commented out |
| `CastleDataByLevel` | Only levels 1 and 5 defined |
| `RemoveTargeter` | Never called for non-defender units — `TargetedBy` accumulates stale refs |

---

## MCP Usage Notes

- Run `read_console` after any script change to catch compilation errors
- Check `editor_state.isCompiling` before testing
- Use `manage_scene → get_hierarchy` to inspect the scene
