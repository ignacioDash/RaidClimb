# RaidClimb — Unit & Trap Reference
**GDD Version:** 1.1 | April 2026

Read this file only when the task involves implementing or modifying units or traps.

---

## Unit Roster (18 planned, all types 1–13 have prefabs)

### MELEE — Climbs Tower · Engages Defences

| Unit | Squad Cost | Arena Unlock | Status |
|---|---|---|---|
| Scout (Melee) | 2 | Arena 1 | ✅ MeleeUnit |
| Raider (Raider) | 2 | Arena 2 | ✅ prefab exists |
| Berserk (Berserk) | 3 | Arena 5 | ✅ prefab exists |

**Scout — Level Stats:**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd | Climb Spd |
|---|---|---|---|---|---|---|
| 1 | 120 | 18 | 1.2/s | 1 | 2.2 | 1.8 |
| 2 | 144 | 22 | 1.2/s | 1 | 2.4 | 1.9 |
| 3 | 173 | 26 | 1.3/s | 1 | 2.5 | 2.0 |
| 4 | 208 | 31 | 1.3/s | 1 | 2.6 | 2.1 |
| 5 | 250 | 38 | 1.4/s | 1.1 | 2.8 | 2.3 |

**Raider — Level Stats:**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd | Climb Spd |
|---|---|---|---|---|---|---|
| 1 | 90 | 30 | 1.5/s | 1 | 2.5 | 1.5 |
| 2 | 108 | 36 | 1.5/s | 1 | 2.7 | 1.6 |
| 3 | 130 | 44 | 1.6/s | 1 | 2.8 | 1.7 |
| 4 | 156 | 53 | 1.7/s | 1 | 3.0 | 1.8 |
| 5 | 187 | 64 | 1.8/s | 1 | 3.2 | 1.9 |

**Berserk — Level Stats:**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd | Climb Spd |
|---|---|---|---|---|---|---|
| 1 | 220 | 12 | 0.9/s | 1 | 1.8 | 1.4 |
| 2 | 264 | 14 | 0.9/s | 1 | 1.9 | 1.5 |
| 3 | 317 | 17 | 1.0/s | 1 | 2.0 | 1.6 |
| 4 | 380 | 21 | 1.0/s | 1 | 2.1 | 1.7 |
| 5 | 456 | 25 | 1.1/s | 1.2 | 2.2 | 1.8 |

---

### RANGED — Stays at Spawn · Fires from Distance

| Unit | Squad Cost | Arena Unlock | Status |
|---|---|---|---|
| Archer (Ranged) | 3 | Arena 1 | ✅ RangeUnit |
| Hunter (Hunter) | 3 | Arena 3 | ✅ prefab exists |
| Deadeye (Deadeye) | 2 | Arena 6 | ✅ prefab exists |

**Archer — Level Stats:**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd |
|---|---|---|---|---|---|
| 1 | 65 | 22 | 1.0/s | 3.5 | 2.0 |
| 2 | 78 | 26 | 1.0/s | 3.5 | 2.0 |
| 3 | 94 | 32 | 1.1/s | 3.8 | 2.0 |
| 4 | 113 | 38 | 1.1/s | 4.0 | 2.1 |
| 5 | 135 | 46 | 1.2/s | 4.0 | 2.2 |

**Hunter — Level Stats:**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd |
|---|---|---|---|---|---|
| 1 | 55 | 35 | 0.7/s | 4.0 | 1.8 |
| 2 | 66 | 42 | 0.7/s | 4.0 | 1.8 |
| 3 | 79 | 51 | 0.8/s | 4.2 | 1.9 |
| 4 | 95 | 61 | 0.8/s | 4.5 | 1.9 |
| 5 | 114 | 73 | 0.9/s | 4.5 | 2.0 |

**Deadeye — Level Stats:**
| Lvl | HP | Dmg/Hit | Atk Rate | Range | Move Spd |
|---|---|---|---|---|---|
| 1 | 80 | 15 | 0.8/s | 4.5 | 1.7 |
| 2 | 96 | 18 | 0.8/s | 4.5 | 1.7 |
| 3 | 115 | 22 | 0.9/s | 4.8 | 1.8 |
| 4 | 138 | 26 | 0.9/s | 5.0 | 1.8 |
| 5 | 166 | 32 | 1.0/s | 5.0 | 1.9 |

---

### TANK — Slow Climber · High Damage to Defences

| Unit | Squad Cost | Arena Unlock | Status |
|---|---|---|---|
| Tank | 5 | Arena 1 | ✅ TankUnit |
| Ogre (Ogre) | 4 | Arena 4 | ✅ prefab exists |
| Golem (Golem) | 6 | Arena 7 | ✅ prefab exists — special regen behaviour |

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

### FUTURE ARCHETYPES (no class/prefab yet)
- **Swat** — Speed runners bypassing all defenses
- **Sapper** — Specialists targeting specific mechanisms

---

## Trap Roster (25 planned, 2 fully implemented)

Implemented: `SpikeTrap` (#03), `ThornHedgeTrap` (maps to #04 Shocker — needs rework).

### Types
| Type | Equips On |
|---|---|
| Turret | Floor — active ranged fire |
| Ground Trap | Floor — passive trigger |
| Wall Trap | Wall — passive trigger during climb |

### Trap Stats

**#01 Sawer (Wall, 25 coins) — one-time burst on climb; Tanks +50%**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 80 | 110 | 150 |
| Dmg | 55 | 82 | 115 |

**#02 Slingshot (Turret, 200 coins) — single-target ranged**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 160 | 220 | 300 |
| Dmg | 22 | 32 | 46 |
| RoF | 0.8/s | 1.0/s | 1.2/s |
| Range | 4 | 4.5 | 5 |

**#03 Spikes (Ground, 100 coins) ✅ — burst on walk-over; Tanks +50%**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 80 | 110 | 150 |
| Dmg | 55 | 82 | 115 |

**#04 Shocker (Wall, 75 coins) — continuous dmg/s; freezes unit**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 120 | 165 | 220 |
| Dmg | 18/s | 28/s | 40/s |
| Range | 1.5 | 1.5 | 1.5 |

**#05 King Cobra (Turret, 400 coins) — toxic shot + 6s poison DoT**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 140 | 195 | 265 |
| Dmg | 44 | 66 | 88 |
| RoF | 0.7/s | 0.9/s | 1.1/s |
| Range | 5.5 | 6 | 6.5 |

**#06 Tar Pit (Ground, 200 coins) — -50% Move Speed, no damage**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 70 | 100 | 140 |
| Range | 2 | 2.8 | 3.5 |

**#07 Hazer (Wall, 150 coins) — confusion haze; purple tint**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 100 | 140 | 190 |
| Dmg | 12 | 18 | 26 |
| Range | 1.5 | 2 | 2.5 |

**#08 Tesla Coil (Turret, 800 coins) — chain lightning; 0.5s stun**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 150 | 210 | 290 |
| Dmg | 45 (+3 jumps) | 65 (+3 jumps) | 90 (+4 jumps) |
| RoF | 0.6/s | 0.8/s | 1.0/s |
| Range | 4.5 | 5 | 5.5 |

**#09 Lava (Ground, 400 coins) — continuous burn DoT**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 60 | 80 | 110 |
| Dmg | 120/s | 180/s | 260/s |
| Range | 2.5 | 3 | 3.5 |

**#10 Pusher (Wall, 500 coins) — ejects climber to floor below; 20 fall dmg**
| Stat | Lvl 1 | Lvl 2 | Lvl 3 |
|---|---|---|---|
| HP | 120 | 170 | 230 |
| RoF | 1/10s | 1/8s | 1/7s |

---

## Adding a New Unit — Checklist

1. **Enum** — add to `BaseUnit.UnitTypes` if not already there
2. **Script** — create `Assets/Scripts/Units/UnitTypes/XUnit.cs`, set `unitType`, `Attack` hash, call `base.Init`
3. **UnitBaseConfig SO** — Assets → Create → Config → UnitBase; fill stats from tables above
4. **Prefab** — base on existing unit; attach script; assign config, controllers, NavMeshAgent, Animator; tag collider "Unit"; castle walls use "CastleWall" tag
5. **UnitReferences** — register `UnitType → prefab` in SO Inspector
6. **InGameScreen** — `GetUnitTypeForButton` maps button index → type (currently driven by `SquadData.EquippedUnits`)
7. **OpponentManager** — add case to `GetDelayForUnit` switch
8. **BotDifficultyConfig** — add to appropriate tier's `availableUnits` in Inspector
9. **Castle / unlock** — for defender-type, set `CastleSlotReference.unitType`; arena unlocks not yet implemented
