# Progression System — Ideas & Notes

## Known Constraints

- **Winning is the only source of coins** (no coin loss on defeat)
- **All 25 purchasable slots available from day 1** (no gating by arena/level)
- **Players start with 1 slot already unlocked** (likely a ~200 coin slot)
- **Tutorial gives bonus coins on first win**, directing the player to unlock another ~200 coin slot
- **Total castle cost: 5,225 coins** across 25 slots

---

## Castle Slot Cost Summary

| Price | Count | Subtotal |
|-------|-------|----------|
| 25    | 3     | 75       |
| 75    | 4     | 300      |
| 100   | 3     | 300      |
| 150   | 3     | 450      |
| 200   | 4     | 800      |
| 400   | 3     | 1,200    |
| 500   | 1     | 500      |
| 800   | 2     | 1,600    |
| **Total** | **25** | **5,225** |

Assuming player starts with one ~200 coin slot unlocked, **effective total to fully unlock = ~5,025 coins**.

---

## Proposed Coin Rewards Per Win

Since winning is the only income source, rewards need to feel meaningful without trivializing expensive slots too quickly.

### Option A — Flat Reward (Simple)
Give the same reward regardless of arena or opponent:

- **50 coins per win**
- Full castle unlock: ~100 wins
- Cheap slots (25–75): unlockable in 1 win
- Expensive slots (800): ~16 wins each

**Pros:** Easy to implement, predictable.  
**Cons:** No sense of progression scaling, late game may feel slow.

---

### Option B — Arena-Scaled Reward (Recommended starting point)
Reward scales with the arena tier the player is currently in:

| Arena Tier | Coins per Win |
|------------|--------------|
| 1–3        | 30           |
| 4–6        | 50           |
| 7–10       | 75           |
| 11–13      | 100          |
| 14–15      | 150          |

- Full castle unlock across a career: ~80–120 wins depending on pace
- Feels rewarding to climb arenas since rewards increase
- Players who stay in lower arenas progress more slowly (soft motivation to climb)

---

### Option C — Win Streak Bonus
Base reward (e.g. 40 coins) with a multiplier for consecutive wins:

- 1st win: 40
- 2nd consecutive: 50
- 3rd+: 60

**Pros:** Rewards skilled/engaged players, adds excitement.  
**Cons:** Slightly more complex to implement; needs a streak tracker in `PlayerData`.

---

## Tutorial Onboarding Flow (Proposed)

1. Player starts with 1 slot pre-unlocked (~200 coin value)
2. First win triggers tutorial bonus — award enough coins to unlock a second ~200 coin slot
3. Player is directed to the castle screen to make the purchase
4. This anchors the purchase loop early and sets expectations for future play

---

## Confirmed Decisions

- No coin loss on defeat — winning is the only income source
- No slot gating — all 25 slots purchasable from day 1
- No additional coin sinks planned beyond castle slots for now
- Scope is limited to what is described above; no win streak system or other mechanics planned at this stage
