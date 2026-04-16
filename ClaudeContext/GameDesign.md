# Raid, Climb & Selfie — Game Design Document

## Core Concept

Fast-paced mobile RTS where two players control towers spawning autonomous attack units. The entire game mechanic revolves around a **single tap-and-hold gesture** anywhere on the arena surface. Victory requires getting a unit to the enemy tower's top to "snap a selfie."

---

## Gameplay Mechanics

### Tap-and-Hold System
Players hold the screen, filling a generation bar that cycles through four slots:

| Slot | Unit Type | Charge Speed |
|------|-----------|--------------|
| 1 | Light Unit | Fastest |
| 2 | Mid Unit | Moderate |
| 3 | Heavy Unit | Slow |
| 4 | Upgrade/Special | Slowest |

Releasing at any point spawns the corresponding unit or effect.

---

## Unit Types (18 Total)

Five archetypes with distinct roles:

- **Melee** — balanced all-rounders who climb and engage defenses
- **Ranged** — stationary damage dealers firing from spawn
- **Tank** — slow bruisers with high durability
- **Swat** — speed runners bypassing all defenses
- **Sapper** — specialists targeting specific mechanisms

Each unit has **5 stat levels** and **2 evolutions** providing unique abilities.

---

## Defense System (25 Traps)

Three types of defense mechanisms on towers:

- **Turrets** — active ranged fire with various effects
- **Ground Traps** — triggered by unit proximity
- **Wall Traps** — activated during climbing

Traps have HP, damage, range, and rate-of-fire stats upgradeable across **3 levels**.

---

## Progression

Players advance through **15 arenas** by accumulating trophies:
- Victories grant trophies; defeats cost them
- A **demotion shield** prevents frustrating drops immediately after promotion

### Item Acquisition
- Soft currency → standard summons
- Hard currency → premium pulls
- Duplicate summons increase item levels, improving stats

---

## Monetization (F2P)

- Hard currency packs
- Optional season passes
- Cosmetic items
- Ad-supported currency boosts
- Designed to reward consistent play **without pay-to-win mechanics**

---

## Technical Scope

- **Platforms:** iOS & Android
- **Session Length:** 2–4 minutes
- **Networking:** None required; offline-capable with asynchronous leaderboards
