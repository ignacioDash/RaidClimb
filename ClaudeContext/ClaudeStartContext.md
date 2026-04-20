# RaidClimb — Claude Start Context
**Game Title:** Raid, Climb & Selfie (aka Snap Squad)
**GDD Version:** 1.1 | April 2026
**Last context update:** April 2026

This document gives a Claude agent deep project context before making changes.
**Primary focus:** Creating and unlocking new units.

---

## Project Overview

- **Engine:** Unity 6000.3.10f1, Android target (iOS future), IL2CPP, .NET Standard 2.1, C# 9.0
- **Single scene:** `Assets/Scenes/MainScene.unity`
- **No CLI build system** — everything goes through Unity Editor
- **MCP:** UnityMCP is connected and active
- **Mode:** PvP vs Bot only (no live networking required; offline-capable)
- **Session length:** 2–4 minutes

---

## Core Design Pillars

- **One-handed, one-button** — entire match interaction is a single tap-and-hold gesture
- **Win condition:** Get a unit to reach the enemy King's SphereCollider at the top of their tower ("snap a selfie")
- **Spectacle** — units climbing walls, turrets firing, combat

---

## Gameplay Loop

### Match Flow
1. Player configures tower defense loadout and unit squad in main menu
2. Match begins — player holds buttons to spawn units; bot runs its AI routine
3. Defense mechanisms work automatically on both towers
4. Units move, climb, and fight autonomously
5. First King reached → that unit snaps selfie → match ends
6. Trophies awarded/deducted; coins granted; progress updated

### Tap-and-Hold Mechanic
Three unit buttons on the right of screen. Hold to fill a gen bar → unit spawns.
- **Light Unit** (Button 0): fills fastest
- **Mid Unit** (Button 1): fills at moderate speed
- **Heavy Unit** (Button 2): fills slowly

**Squad Power System** (NOT YET IMPLEMENTED in code):
- Max 10 squad power at any time
- Each unit costs power to spawn (see unit roster below)
- Power meter shown at bottom of HUD
- Player must judge cheap weak units vs. holding for powerful ones

---

## Unit System

### Unit Type Enum (`BaseUnit.UnitTypes`)
```csharp
None, Melee, Ranged, Tank, Swat, Sapper, King, Defender
// Swat and Sapper: declared in enum, NO concrete class/prefab/config yet
```

### Unit Class Hierarchy (`Assets/Scripts/Units/`)
```
BaseUnit (abstract, partial)
├── MeleeUnit     — Attack anim: "Sword"
├── RangeUnit     — Attack anim: "Arrow", _attackRotationOffset = (0,60,0)
├── TankUnit      — Attack anim: "Sword"
├── KingUnit      — Win condition; SphereCollider; always Idle state
├── DefenderUnit  — Static; triggers on enemy SphereCollider enter
// Swat, Sapper: no class yet
```

### BaseUnit Split (partial class)
- `BaseUnit.cs` — state machine, lifecycle, Init, CleanUp
- `BaseUnit.UnitHanlders.cs` — Update loop, trigger handlers, climb logic

### UnitState Machine
```
Idle → Moving → Attacking
              ↘ Climbing → (1s DelayedTarget) → Moving
Defending (Defenders only — SphereCollider enter/exit)
Dead (2s delay → onUnitDeath callback)
Won / Lost (terminal, camera victory/defeat)
```
`Won`/`Lost` cannot be overridden once set.

### UnitBaseConfig (`Assets/Scripts/Config/UnitBaseConfig.cs`)
ScriptableObject per unit:
```csharp
float Health, Damage, Range, MovementSpeed, ClimbSpeed, AttackSpeed
```

### Concrete Unit Init Pattern
```csharp
public override void Init(string playerId, UnitState startState, Action onUnitDeath)
{
    unitType = UnitTypes.Melee;
    Attack = Animator.StringToHash("AnimTriggerName");
    base.Init(playerId, startState, onUnitDeath);
    // _attackRotationOffset = new Vector3(0, 60, 0); // if needed
}
```

### UnitReferences (`Assets/Scripts/Config/UnitReferences.cs`)
ScriptableObject mapping `UnitTypes → BaseUnit prefab`.
`UnitManager.SpawnUnit` looks up the prefab here.
**Adding a new unit: register its prefab here in the Unity Inspector.**

---

## Full Unit Roster (GDD v1.1)

18 units planned across 5 archetypes. 9 defined so far.

### MELEE — All-rounder · Climbs Tower · Engages Defences

| Unit | Squad Cost | Arena Unlock | Status |
|---|---|---|---|
| 🗡️ Scout | 2 | Arena 1 (start) | ✅ MeleeUnit exists (maps to Scout) |
| ⚔️ Raider | 2 | Arena 2 | 🔴 No class yet |
| 🪓 Berserk | 3 | Arena 5 | 🔴 No class yet |

**Scout (Melee) — Level Stats:**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd | Climb Spd |
|---|---|---|---|---|---|---|
| 1 | 120 | 18 | 1.2/s | 1 | 2.2 | 1.8 |
| 2 | 144 | 22 | 1.2/s | 1 | 2.4 | 1.9 |
| 3 | 173 | 26 | 1.3/s | 1 | 2.5 | 2.0 |
| 4 | 208 | 31 | 1.3/s | 1 | 2.6 | 2.1 |
| 5 | 250 | 38 | 1.4/s | 1.1 | 2.8 | 2.3 |

**Raider (Melee) — Level Stats:**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd | Climb Spd |
|---|---|---|---|---|---|---|
| 1 | 90 | 30 | 1.5/s | 1 | 2.5 | 1.5 |
| 2 | 108 | 36 | 1.5/s | 1 | 2.7 | 1.6 |
| 3 | 130 | 44 | 1.6/s | 1 | 2.8 | 1.7 |
| 4 | 156 | 53 | 1.7/s | 1 | 3.0 | 1.8 |
| 5 | 187 | 64 | 1.8/s | 1 | 3.2 | 1.9 |

**Berserk (Melee) — Level Stats:**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd | Climb Spd |
|---|---|---|---|---|---|---|
| 1 | 220 | 12 | 0.9/s | 1 | 1.8 | 1.4 |
| 2 | 264 | 14 | 0.9/s | 1 | 1.9 | 1.5 |
| 3 | 317 | 17 | 1.0/s | 1 | 2.0 | 1.6 |
| 4 | 380 | 21 | 1.0/s | 1 | 2.1 | 1.7 |
| 5 | 456 | 25 | 1.1/s | 1.2 | 2.2 | 1.8 |

---

### RANGED — Defence Shredder · Stays at Spawn · Fires from Distance

| Unit | Squad Cost | Arena Unlock | Status |
|---|---|---|---|
| 🏹 Archer | 3 | Arena 1 (start) | ✅ RangeUnit exists (maps to Archer) |
| 🪃 Hunter | 3 | Arena 3 | 🔴 No class yet |
| 💀 Deadeye | 2 | Arena 6 | 🔴 No class yet |

**Archer (Ranged) — Level Stats:**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd | Climb Spd |
|---|---|---|---|---|---|---|
| 1 | 65 | 22 | 1.0/s | 3.5 | 2.0 | — |
| 2 | 78 | 26 | 1.0/s | 3.5 | 2.0 | — |
| 3 | 94 | 32 | 1.1/s | 3.8 | 2.0 | — |
| 4 | 113 | 38 | 1.1/s | 4.0 | 2.1 | — |
| 5 | 135 | 46 | 1.2/s | 4.0 | 2.2 | — |

**Hunter (Ranged) — Level Stats:**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd |
|---|---|---|---|---|---|
| 1 | 55 | 35 | 0.7/s | 4.0 | 1.8 |
| 2 | 66 | 42 | 0.7/s | 4.0 | 1.8 |
| 3 | 79 | 51 | 0.8/s | 4.2 | 1.9 |
| 4 | 95 | 61 | 0.8/s | 4.5 | 1.9 |
| 5 | 114 | 73 | 0.9/s | 4.5 | 2.0 |

**Deadeye (Ranged) — Level Stats:**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd |
|---|---|---|---|---|---|
| 1 | 80 | 15 | 0.8/s | 4.5 | 1.7 |
| 2 | 96 | 18 | 0.8/s | 4.5 | 1.7 |
| 3 | 115 | 22 | 0.9/s | 4.8 | 1.8 |
| 4 | 138 | 26 | 0.9/s | 5.0 | 1.8 |
| 5 | 166 | 32 | 1.0/s | 5.0 | 1.9 |

---

### TANK — Bruiser · Slow Climber · High Damage to Defences

| Unit | Squad Cost | Arena Unlock | Status |
|---|---|---|---|
| 🛡️ Tank | 5 | Arena 1 (start) | ✅ TankUnit exists |
| 🧌 Ogre | 4 | Arena 4 | 🔴 No class yet |
| 🗿 Golem | 6 | Arena 7 | 🔴 No class yet — has special regen behaviour |

**Tank — Level Stats:**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd | Climb Spd |
|---|---|---|---|---|---|---|
| 1 | 420 | 50 | 0.6/s | 1.2 | 1.2 | 0.7 |
| 2 | 504 | 60 | 0.6/s | 1.2 | 1.2 | 0.8 |
| 3 | 605 | 72 | 0.7/s | 1.2 | 1.3 | 0.9 |
| 4 | 726 | 86 | 0.7/s | 1.2 | 1.4 | 1.0 |
| 5 | 871 | 104 | 0.8/s | 1.3 | 1.5 | 1.1 |

**Ogre — Level Stats:**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd | Climb Spd |
|---|---|---|---|---|---|---|
| 1 | 480 | 38 | 0.5/s | 1 | 1.0 | 0.6 |
| 2 | 576 | 46 | 0.5/s | 1 | 1.0 | 0.7 |
| 3 | 691 | 55 | 0.6/s | 1 | 1.1 | 0.8 |
| 4 | 829 | 66 | 0.6/s | 1 | 1.2 | 0.9 |
| 5 | 995 | 79 | 0.7/s | 1 | 1.3 | 1.0 |

**Golem — Level Stats (Special: regenerates 5 HP/s while not under attack):**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd | Climb Spd |
|---|---|---|---|---|---|---|
| 1 | 380 | 40 | 0.5/s | 1 | 0.9 | 0.5 |
| 2 | 456 | 48 | 0.5/s | 1 | 0.9 | 0.6 |
| 3 | 547 | 58 | 0.6/s | 1 | 1.0 | 0.7 |
| 4 | 656 | 70 | 0.6/s | 1 | 1.1 | 0.8 |
| 5 | 787 | 84 | 0.7/s | 1 | 1.2 | 0.9 |

---

### SWAT & SAPPER (Future Archetypes)
- **Swat** — Speed runners bypassing all defenses. `UnitTypes.Swat` declared, NO class/prefab/config.
- **Sapper** — Specialists targeting specific mechanisms. `UnitTypes.Sapper` declared, NO class/prefab/config.
- Each archetype will have multiple units across arenas (part of the 18 total roster).

---

## Spawning Flow (Player)

`InGameScreen` holds 3 `ButtonPointerComponent` buttons:
- Button 0 → `UnitTypes.Melee`
- Button 1 → `UnitTypes.Ranged`
- Button 2 → `UnitTypes.Tank`

`GetUnitTypeForButton(int index)` at `InGameScreen.cs:147` — **TODO: drive from config** (currently hardcoded).

Hold duration per button configured by `HoldRanges[]` on `InGameScreen`.
On fill complete → `SpawnUnit` → 1.5s delay → `FindNewTargetFor`.

---

## Spawning Flow (Bot / OpponentManager)

`Assets/Scripts/Managers/OpponentManager.cs`:
- `BotDifficultyConfig` ScriptableObject → `BotDifficultyTier` selected by current arena
- `BotDifficultyTier`: `minArena`, `minSpawnIntervalMs`, `maxSpawnIntervalMs`, `List<UnitTypes> availableUnits`
- 3s initial delay, then loop: spawn → `GetDelayForUnit` → random interval → repeat

`GetDelayForUnit` (`OpponentManager.cs:90`):
```csharp
Melee → 2000ms, Ranged → 4000ms, Tank → 6000ms, _ → 2000ms
// Add new unit types here
```

### Bot Difficulty Tiers (target design)
| Arena | Spawn Interval | Available Units |
|---|---|---|
| 1 | 3–5s | Melee only |
| 2–3 | 2–4s | Melee, Ranged |
| 4–9 | 2–3s | Melee, Ranged, Tank |
| 10–12 | 1–3s | Melee, Ranged, Tank |
| 13–15 | 1–2s | Melee, Ranged, Tank |

Bot difficulty philosophy: lower tiers over-commit to single types and react slowly; mid-tiers mix unit types; high tiers play near-optimally. Bot never has unfair information access.

---

## Castle / Tower System

### Tower Structure
Three levels (Stage 1 ground, Stage 2 mid, Stage 3 top) + King pinnacle.
Each level has floor slots and wall slots for traps.

### CastleSlotId Enum (`PlayerData.cs`)
```
King, Stage3Turret1/2, Stage3Wall1/2/3, Stage3Floor1
Stage2Turret1/2, Stage2Floor1/2, Stage2Wall1/2/3/4
Stage1Turret1/2, Stage1Floor1/2/3, Stage1Wall1/2/3
BaseFloor1/2/3, KinWall (extra)
```

### CastleSlot
```csharp
CastleSlotId SlotId
BaseUnit.UnitTypes SlotUnit   // OR
BaseTrap.TrapTypes SlotTrap
```

### PlayerCastleManager
- `OnCastleScreenOpened()` — shows purchase buttons for unpurchased purchasable slots
- `OnPurchaseButton(slot)` — deducts coins, persists slot, spawns unit/trap immediately
- `OnGameStarted()` — spawns all saved slots

### OpponentCastleManager
- Populated from `CastleDataByLevel.GetCastleDataForLevel(int level)` — hardcoded static lookup
- Level 1: King + 1 Defender. Level 5: King + 2 Defenders + Spikes + ThornHedges
- `StartGame` always uses level 1 (**TODO: check user level**)

---

## Full Trap Roster (GDD v1.1)

25 traps planned (10 defined). 3 upgrade levels each. Two existing: `Spikes` (#03), `ThornHedge` (maps to #04 Shocker area).

### Trap Types
| Type | Equips On | Key Trait |
|---|---|---|
| Turret | Floor | Active ranged fire; consistent DPS or effects |
| Ground Trap (H-trap) | Floor | Passive trigger when units walk over |
| Wall Trap (V-trap) | Wall | Passive trigger when units climb |

### Trap Stats (all traps)
HP, Rate of Fire, Range, Damage per Hit. Scales with upgrade level.
Attackable = yes (by any unit) or no (only by Sappers — future unit).

### Defined Traps

| # | Name | Type | Cost | Effect |
|---|---|---|---|---|
| #01 | 🪚 Sawer | Wall | 25 coins | One-time burst damage when unit climbs it; Tanks take 50% extra |
| #02 | 🪨 Slingshot | Turret | 200 coins | Fires at nearest ground-level unit; single-target ranged damage |
| #03 | 🪤 Spikes | Ground | 100 coins | Burst damage when unit walks over (one-time); Tanks +50% |
| #04 | ⚠️⚡ Shocker | Wall | 75 coins | Constant electric damage/s to climbing unit; freezes unit when shocked |
| #05 | 🐍 King Cobra | Turret | 400 coins | Toxic shot + 6s poison burn DoT; green particles on affected units |
| #06 | 🍯 Tar Pit | Ground | 200 coins | -50% Move Speed while inside range; no damage |
| #07 | 🪻 Hazer | Wall | 150 coins | Purple haze cloud every X sec; confused units wander aimlessly; purple tint |
| #08 | 🪩⚡ Tesla Coil | Turret | 800 coins | Chain lightning; jumps to nearby units at 60% per jump; 0.5s stun |
| #09 | 🔥 Lava | Ground | 400 coins | Continuous burn DoT while in contact; fire particles on unit |
| #10 | 🫸 Pusher | Wall | 500 coins | Wall protrudes every X sec; ejects climbing unit to floor below; 20 fall dmg |

### Trap Stats Detail

**#01 Sawer (Wall, 25 coins)**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 80 | 110 | 150 |
| Dmg | 55 | 82 | 115 |
| Range | 1 | 1 | 1 |

**#02 Slingshot (Turret, 200 coins)**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 160 | 220 | 300 |
| Dmg | 22 | 32 | 46 |
| RoF | 0.8/s | 1.0/s | 1.2/s |
| Range | 4 | 4.5 | 5 |

**#03 Spikes (Ground, 100 coins)** ✅ Implemented as `SpikeTrap`
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 80 | 110 | 150 |
| Dmg | 55 | 82 | 115 |
| Range | 1 | 1 | 1 |

**#04 Shocker (Wall, 75 coins)** — partially maps to `ThornHedgeTrap` but needs full rework
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 120 | 165 | 220 |
| Dmg | 18/s | 28/s | 40/s |
| Range | 1.5 | 1.5 | 1.5 |

**#05 King Cobra (Turret, 400 coins)**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 140 | 195 | 265 |
| Dmg | 44 | 66 | 88 |
| RoF | 0.7/s | 0.9/s | 1.1/s |
| Range | 5.5 | 6 | 6.5 |

**#06 Tar Pit (Ground, 200 coins)**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 70 | 100 | 140 |
| Dmg | 0 | 0 | 0 |
| Range | 2 | 2.8 | 3.5 |

**#07 Hazer (Wall, 150 coins)**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 100 | 140 | 190 |
| Dmg | 12 | 18 | 26 |
| Range | 1.5 | 2 | 2.5 |

**#08 Tesla Coil (Turret, 800 coins)**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 150 | 210 | 290 |
| Dmg | 45 (+3 jumps) | 65 (+3 jumps) | 90 (+4 jumps) |
| RoF | 0.6/s | 0.8/s | 1.0/s |
| Range | 4.5 | 5 | 5.5 |

**#09 Lava (Ground, 400 coins)**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 60 | 80 | 110 |
| Dmg | 120/s | 180/s | 260/s |
| Range | 2.5 | 3 | 3.5 |

**#10 Pusher (Wall, 500 coins)**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 120 | 170 | 230 |
| Dmg | 20 (fall) | 20 (fall) | 20 (fall) |
| RoF | 1/10s | 1/8s | 1/7s |

---

## Progression System

### Unit Unlocks
- Units unlock when the player reaches the required arena
- **Squad section** in main menu: view collection, equip units into 3 playing slots
- Not yet implemented in code

### Currency
| Currency | Earned By | Used For |
|---|---|---|
| Coins | Match victories (primary); small amount from defeats | Buy traps; future: upgrade units/traps |

- **No coin loss on defeat** — only trophies are affected
- Coin amount scales with arena (see EconomyConfig)

### Trophies & Arenas
- 15 arenas total
- Win → gain trophies (varies by arena); Lose → lose trophies
- **Demotion Shield:** Cannot be demoted below arena entry threshold during first **3 matches** in a new arena
  - ⚠️ Code currently has `demotionShieldChargesOnPromotion = 1` — GDD says 3 matches. Needs update.
- Trophies determine arena and global leaderboard rank

### Tournaments
- Timed competitive periods (1 week)
- Trophy accumulation above thresholds → prizes (Bronze/Silver/Gold/Diamond tiers)
- Trophies reset at tournament end
- Not yet implemented in code

### Castle Slot Cost Summary (25 slots, total ~5,225 coins)
| Price | Count |
|---|---|
| 25 | 3 |
| 75 | 4 |
| 100 | 3 |
| 150 | 3 |
| 200 | 4 |
| 400 | 3 |
| 500 | 1 |
| 800 | 2 |

---

## Data Persistence

`DataManager` (`Assets/Scripts/Managers/DataManager.cs`):
- Save file: `Application.persistentDataPath/SaveData.txt` (JSON via Newtonsoft.Json)
- Default new game: 9999 coins, King + 1 Defender in castle
- `Initialized` flag — other managers `await Task.Yield()` until true

`UserData` fields:
```csharp
int UserLevel, coins, trophies, gamesPlayed, demotionShieldCharges
string UserName
bool isFirstWin
```

---

## Economy / Arena

`EconomyConfig` ScriptableObject:
- `trophiesPerWin`, `trophiesPerLoss`
- `arenaTrophyThresholds` — list of int thresholds; arena = thresholds surpassed + 1
- `arenaRewardTiers` — coins per win based on trophy count
- `newPlayerShieldGames` — no trophy loss for first N games
- `demotionShieldChargesOnPromotion` — GDD says 3, currently set to 1

---

## Architecture — Manager System

`GameManager` (singleton, `Assets/Scripts/GameManager.cs`):
- Holds `Dictionary<Type, IManager>` — access via `GameManager.Instance.GetManager<T>()`
- All managers implement `IManager` (Init/Cleanup)
- Init is async (`Task.WhenAll`) — never block main thread

### Manager Registration Order
```
DataManager → UIManager → UnitManager → GameStateManager →
PlayerCastleManager → OpponentCastleManager → TrapsManager →
CameraManager → OpponentManager → CurrencyManager
```

### Game Flow
1. App starts → `DataManager` loads save → all managers init → `MainScreen`
2. Play → `GameManager.StartGame()` → camera moves → `InGameScreen` activates
3. `PlayerCastleManager.OnGameStarted()` spawns player castle
4. `OpponentCastleManager.SetUpOpponent(CastleDataByLevel.GetCastleDataForLevel(1))` spawns bot castle
5. `OpponentManager.OnGameStarted()` kicks off bot loop
6. `KingUnit.OnTriggerEnter` → `GameManager.GameEnded(winnerId)`
7. Rewards, save, camera, `GameEndScreen`
8. Finish → `GameManager.FinishGame()` → `MainScreen`

---

## Scene Hierarchy (MainScene)

```
GameManager           — GameManager.cs
SpawnArea             — BoxCollider for opponent spawn
Managers/
  CamerasManager      — CameraManager.cs
  UnitManager         — UnitManager.cs
  TrapsManager        — TrapsManager.cs
  OpponentManager     — OpponentManager.cs
Main Camera / Unit1Camera (x3) / PlayerCamera
CameraPositions/      — 7 Transform anchors for camera animation
Directional Light
MainCanvas/           — UIManager.cs + 8 screen GameObjects
EventSystem
PlayArea/
OpponentCastle/       — OpponentCastleManager + Stage1/2/3/King/Extra
PlayerCastle/         — PlayerCastleManager + WorldCanvas + Stage1/2/3/King/Extra
World/ + Mist (inactive)
```

---

## Adding a New Unit — Complete Checklist

### 1. Add to Enum (if not already)
`BaseUnit.cs` → `UnitTypes`. (`Swat`, `Sapper` already declared.)

### 2. Create the Script
`Assets/Scripts/Units/UnitTypes/RaiderUnit.cs`:
```csharp
using System;
using UnityEngine;

namespace Units.UnitTypes
{
    public class RaiderUnit : BaseUnit
    {
        public override void Init(string playerId, UnitState startState, Action onUnitDeath)
        {
            unitType = UnitTypes.Raider; // add Raider to enum first
            Attack = Animator.StringToHash("Sword");
            base.Init(playerId, startState, onUnitDeath);
        }
        // Override HandleMoveToTarget / HandleAttackTarget / OnTriggerEnter for custom behaviour
        // For Golem: override Update to add HP regen when not under attack
    }
}
```

### 3. Create UnitBaseConfig ScriptableObject
Unity: Assets → Create → Config → UnitBase. Use stat tables above.

### 4. Create the Prefab
- Base on existing unit prefab
- Attach concrete unit script
- Assign: `UnitBaseConfig`, `UnitHealthController`, `UnitVisualsController`, `UnitTargetController`, `NavMeshAgent`, `Animator`
- Tag main collider **"Unit"** (required by KingUnit and DefenderUnit detection)
- Castle wall triggers climb via **"CastleWall"** tag

### 5. Register in UnitReferences
`UnitReferences` ScriptableObject → add `UnitType = X`, `UnitPrefab = <prefab>`

### 6. Wire into InGameScreen (player buttons)
`InGameScreen.cs:147` `GetUnitTypeForButton(int index)` — map button index to type.

### 7. Wire into OpponentManager
- `GetDelayForUnit` switch (`OpponentManager.cs:90`) — add a case with ms delay
- `BotDifficultyConfig` SO in Inspector → add to appropriate tier's `availableUnits`

### 8. Wire into Castle / unlock system
- For defender-type: `CastleSlotPurchase.unitType` on the slot's `CastleSlotReference`
- For arena-unlock: needs unlock system (NOT YET IMPLEMENTED — unit unlock by arena is future work)

### 9. Unit Unlock Logic (current state)
No separate unlock flag exists. Owning the castle slot IS the unlock.
Arena-based unlocks (as per GDD) are **not yet implemented** — all units currently accessible.

---

## Key Files for Unit Work

| File | Purpose |
|---|---|
| `Assets/Scripts/Units/UnitTypes/BaseUnit.cs` | Abstract base, state machine |
| `Assets/Scripts/Units/UnitTypes/BaseUnit.UnitHanlders.cs` | Update/trigger/climb logic |
| `Assets/Scripts/Config/UnitBaseConfig.cs` | Stats ScriptableObject |
| `Assets/Scripts/Config/UnitReferences.cs` | Prefab registry |
| `Assets/Scripts/Managers/UnitManager.cs` | Spawn, find target, death |
| `Assets/Scripts/Managers/OpponentManager.cs` | Bot spawn loop + unit pool |
| `Assets/Scripts/UI/InGameScreen.cs` | Player button → unit type mapping |
| `Assets/Scripts/Castles/CastleDataByLevel.cs` | Bot castle presets per level |
| `Assets/Scripts/Data/PlayerData.cs` | Save data structure, CastleSlotId enum |
| `Assets/Scripts/Managers/CastleManager.cs` | Base castle spawn logic |
| `Assets/Scripts/Castles/PlayerCastleManager.cs` | Purchase flow |

---

## Known TODOs / Gaps (code vs. GDD)

| Area | Status |
|---|---|
| `InGameScreen.GetUnitTypeForButton` | Hardcoded — TODO: drive from config |
| Squad power system (max 10, unit costs) | **NOT implemented** |
| Arena-based unit unlocks | **NOT implemented** — all units accessible |
| Unit levelling (5 levels per unit) | **NOT implemented** |
| Squad selection screen | `CollectionScreen` is empty shell |
| Demotion shield = 3 matches | Code has `= 1` — mismatches GDD |
| Tournament system | **NOT implemented** |
| Raider, Berserk, Hunter, Deadeye, Ogre, Golem | **No class, prefab, or config** |
| Swat, Sapper archetypes | **No class, prefab, or config** |
| Wall traps (Sawer, Shocker, Hazer, Pusher) | **Not implemented** — only Spikes + ThornHedge exist |
| Turret traps (Slingshot, King Cobra, Tesla Coil) | **Not implemented** |
| Lava, Tar Pit ground traps | **Not implemented** |
| `DefenderUnit.OnTriggerExit` | Body is commented out |
| `CastleDataByLevel` | Only levels 1 and 5 defined; default returns empty |
| Bot always uses level 1 castle | TODO: check user level |
| `BaseUnit` Defending state | TODO: play defending animation |

---

## MCP Usage Notes

- Use `read_console` after any script change to check for compilation errors
- Use `manage_scene → get_hierarchy` to inspect GameObjects
- Use `find_gameobjects` by component to locate managers
- Use `validate_script` before testing logic changes
- Check `editor_state.isCompiling` after changes before testing
