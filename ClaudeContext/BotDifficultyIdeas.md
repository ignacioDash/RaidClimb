# Bot Difficulty — Analysis & Design Ideas

## Current Bot Behaviour

The `OpponentManager` runs a loop that:
1. Waits 3 seconds before starting
2. Picks a random unit from `[Melee, Ranged, Tank]`
3. Spawns it at a random position in the spawn area
4. Waits for the unit's charge delay (`Melee: 2s`, `Ranged: 4s`, `Tank: 6s`)
5. Waits an additional random interval of **2–5 seconds**
6. Repeats

There is a `_gameplayLevel` field that reads from `UserData.UserLevel`, but it is never actually used anywhere — all values are hardcoded. There is also a **bug** in `GetRandomUnit`: `Random.Range(0, opponentUnits.Count - 1)` — Unity's integer `Random.Range` is exclusive of the upper bound, so Tank is **never** spawned. Should be `Random.Range(0, opponentUnits.Count)`.

---

## Design Philosophy

The difficulty system should tie directly to **arena progression**, since arenas already represent how much the player has invested in their castle. A player in Arena 1 might have only their starting 200-coin defensive unit; a player in Arena 6 has likely unlocked several more slots. The bot's threat level should match that curve.

The per-unit charge delay (Melee 2s, Ranged 4s, Tank 6s) simulates the player's hold mechanic and **should not vary much** between difficulty levels — it's a game feel constant, not a difficulty lever. The real knobs are:

- **Which units** the bot is allowed to spawn (unit pool)
- **How often** it spawns (inter-spawn interval)
- **How long before** it starts (initial delay)

A dumb bot that only spawns Melee units is naturally slower than one that also spawns Ranged and Tank, because Melee has the shortest charge delay. So restricting the unit pool early has a natural double effect: fewer threat types AND faster spawning rhythm — which is fine, since early game is deliberately forgiving.

---

## Linking Difficulty to Progression

Rather than using `UserLevel` (which is never updated), bot difficulty should be driven by the **player's current arena**, derived from trophies via `CurrencyManager.GetArenaForTrophies()`. This is already the natural measure of how far the player has progressed and how developed their castle should be.

---

## Proposed ScriptableObject: `BotDifficultyConfig`

A single SO with a list of tiers, each kicking in at a minimum arena. The `OpponentManager` picks the highest tier whose `minArena` is ≤ the player's current arena — same lookup pattern as `EconomyConfig.arenaRewardTiers`.

```csharp
[CreateAssetMenu(fileName = "BotDifficultyConfig", menuName = "Config/BotDifficultyConfig")]
public class BotDifficultyConfig : ScriptableObject
{
    public List<BotDifficultyTier> tiers;
}

[Serializable]
public class BotDifficultyTier
{
    public int minArena;
    public int minSpawnIntervalMs;
    public int maxSpawnIntervalMs;
    public List<BaseUnit.UnitTypes> availableUnits;
}
```

---

## Proposed Difficulty Tiers

| Arena | Label | Spawn Interval | Available Units |
|-------|-------|----------------|-----------------|
| 1     | Beginner | 3–5s | Melee |
| 2–3   | Easy | 2–4s | Melee, Ranged |
| 4–9   | Medium | 2–3s | Melee, Ranged, Tank |
| 10–12 | Hard | 1–3s | Melee, Ranged, Tank |
| 13–15 | Expert | 1–2s | Melee, Ranged, Tank |

*More unit types will be added to the pool as new units are implemented.*

**Key observations:**
- Arena 1 bot only spawns Melee. Combined with the Melee charge delay (2s) and the spawn interval (6–10s), total time between units is **8–12 seconds** — very slow, gives new players room to breathe
- Ranged introduced at Arena 2 (~3–4 wins), a quick first spike that players will notice early
- Tank introduced at Arena 4 (~10 wins), by which point the player should have a couple of castle upgrades
- Hard/Expert tiers compress the interval significantly — at 1–3s + unit delay, the bot becomes genuinely threatening

---

## Integration Points in `OpponentManager`

- `OnGameStarted()` → resolve current arena → store matching `BotDifficultyTier`
- `PlayingLoop()` → replace `Random.Range(2000, 5000)` with tier values; initial `3000ms` delay stays as a constant
- `GetRandomUnit()` → pick from `tier.availableUnits` instead of a hardcoded list; fix the `Count - 1` bug
- Per-unit delays in `GetDelayForUnit()` stay unchanged

---

## Open Questions

- Should the bot difficulty be set at game start and stay fixed for the whole match, or could it ramp up mid-game? (Fixed per match is simpler and more predictable for the player.)
- Should the bot in very late arenas ever spawn more than one unit type at a time (multi-lane pressure)? Out of current scope but worth noting.
