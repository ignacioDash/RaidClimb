# Onboarding System & UI Changes
**Last updated:** April 2026

---

## Onboarding System Overview

A step-based tutorial that highlights UI elements with a 4-panel unmask, optional finger tap animation, and optional text. Steps persist in save data so they never replay.

### Core Classes

| File | Role |
|---|---|
| `Assets/Scripts/UI/OnboardingScreen.cs` | Controller — shows/hides panels, finger, text |
| `Assets/Scripts/Constants/OnboardingTexts.cs` | All onboarding string constants |
| `Assets/Scripts/Data/PlayerData.cs` | `OnboardingData` class with `CompletedSteps` list |

### OnboardingScreen Architecture

**Serialized fields:**
- `page` — root GameObject toggled on/off for the whole overlay
- `fullCoverButton` — transparent full-screen button for tap-to-advance steps
- `finger` — RectTransform with tap scale animation (1→0.75→1, looping)
- `panelTop/Bottom/Left/Right` — 4 stretch-anchored RectTransforms forming the unmask frame
- `textContainer` — hidden automatically when text is null/empty
- `onboardingText` — TextMeshProUGUI
- `targets` — `List<OnboardingTargets>` mapping `MaskTarget` enum → `RectTransform`
- `padding` — float (default 8f) extra space around highlighted element

**Key methods:**
```csharp
ShowInGameSteps()      // step 0 only
ShowSquadSteps()       // steps 1–2
ShowMainMenuSteps()    // step 3 only
ShowTowerSteps()       // steps 4–5
ShowMainMenuPlaySteps() // step 6 only
TryCompleteStep(int)   // completes only if page is active and currentStep matches
CompleteAndHide()      // completes current step and hides
Hide()                 // hides without completing
```

**Show() signature:**
```csharp
private void Show(MaskTarget maskTarget, string message, bool tapToDismiss = false, bool showFinger = true)
```
- `tapToDismiss = true` → enables `fullCoverButton`, hides finger
- `showFinger = false` → hides finger without enabling fullCoverButton
- `message = null` → hides `textContainer`

**FocusOn() panel math** (stretch anchors, origin at canvas center):
```csharp
panelTop.offsetMin    = new Vector2(0, localTR.y + halfH);   panelTop.offsetMax    = new Vector2(0, 0);
panelBottom.offsetMin = new Vector2(0, 0);                    panelBottom.offsetMax = new Vector2(0, localBL.y - halfH);
panelLeft.offsetMin   = new Vector2(0, localBL.y + halfH);   panelLeft.offsetMax   = new Vector2(localBL.x - halfW, localTR.y - halfH);
panelRight.offsetMin  = new Vector2(localTR.x + halfW, localBL.y + halfH); panelRight.offsetMax = new Vector2(0, localTR.y - halfH);
```

**Full-cover steps** (`ShowFullCover`): only `panelTop` is active (stretched full). Other 3 panels off. This avoids the "too dark" stacked-panel look.

---

## Step Table (TotalSteps = 7, indices 0–6)

| Step | Screen | MaskTarget | Text | Finger | Completed by |
|------|--------|------------|------|--------|--------------|
| 0 | InGame | Unit1 | "Tap to summon a Scout" | yes | dropping any unit |
| 1 | Squad | Raider | "You unlocked the Raider—equip now!" | yes | selecting Raider |
| 2 | Squad | Equip | *(none)* | yes | tapping Equip button |
| 3 | MainMenu | Castle | "Tap the Castle button to upgrade your defenses!" | yes | tapping tower button |
| 4 | Tower | CastleBuyTarget | "Buy traps for your castle!" | **no** | buying any castle slot |
| 5 | Tower | CastleQuit | *(none)* | yes | tapping quit button |
| 6 | MainMenu | PlayButton | "Let's raid!" | yes | tapping play button |

### Step Trigger Conditions
- **Step 0**: Only when `gamesPlayed == 0` (first session auto-starts into game). Shown after squad meter first reaches 1 charge (~3s). Guard flag `_inGameOnboardingShown` prevents repeat within same session.
- **Steps 1–2**: `ShowSquadSteps()` called from `SquadScreenUI.OpenScreen()` only if Raider is unlocked (`currentArena >= raiderButton.Config.ArenaUnlock`).
- **Step 3**: Called from `MainScreenUI.OpenScreen()` when `coins >= 25`.
- **Steps 4–5**: Called from `TowerScreen.OpenScreen()` when `coins >= CastleUpgradeCoinsThreshold (25)`.
- **Step 6**: Called from `MainScreenUI.OpenScreen()` only if step 3 is already completed (`onboardingData.IsStepCompleted(3)`).

---

## Per-Screen Wiring

### InGameScreen (`Assets/Scripts/UI/InGameScreen.cs`)
```csharp
[SerializeField] private OnboardingScreen onboardingScreen;
// In Update refill block:
if (_squadMeter == 1 && !_inGameOnboardingShown) { _inGameOnboardingShown = true; onboardingScreen?.ShowInGameSteps(); }
// After unit spawned:
onboardingScreen?.TryCompleteStep(0);
```

### SquadScreenUI (`Assets/Scripts/UI/SquadScreenUI.cs`)
```csharp
[SerializeField] private OnboardingScreen onboardingScreen;
// OpenScreen: if raider unlocked → onboardingScreen?.ShowSquadSteps()
// OnUnitSelected: if Raider → onboardingScreen?.TryCompleteStep(1)
// OnEquip (before slot assignment): onboardingScreen?.TryCompleteStep(2)
```

### MainScreenUI (`Assets/Scripts/UI/MainScreenUI.cs`)
```csharp
[SerializeField] private OnboardingScreen onboardingScreen;
[SerializeField] private UnitCamerasController unitCamerasController;
// OpenScreen: coins >= 25 → ShowMainMenuSteps(); isStepCompleted(3) → ShowMainMenuPlaySteps()
// OnCastleButton: TryCompleteStep(3) before SetButtons(false)
// OnPlayButton: TryCompleteStep(6) before SetButtons(false)
// OnMatchmakingStarted: ShowRandomFullBodyUnit(equipped) + HideAllFullBodyUnits after delay
```

### TowerScreen (`Assets/Scripts/UI/TowerScreen.cs`)
```csharp
[SerializeField] private OnboardingScreen onboardingScreen;
// OnEnable: subscribe PlayerCastleManager.OnSlotPurchased += OnSlotPurchased
// OnDisable: unsubscribe
// OnSlotPurchased(): TryCompleteStep(4)
// OnExit(): TryCompleteStep(5) then proceed with navigation
```

### PlayerCastleManager (`Assets/Scripts/Castles/PlayerCastleManager.cs`)
```csharp
public Action OnSlotPurchased;
// Fired inside OnPurchaseButton() after SpawnSlot
```

---

## MaskTarget Enum (OnboardingScreen.cs)
```csharp
None, PlayButton, Unit1, Unit2, Unit3, SquadMeter,
Raider, Equip, Castle, CastleBuyTarget, CastleQuit
```
Each entry needs a matching `OnboardingTargets` entry in the Inspector targets list.

---

## OnboardingData (PlayerData.cs)
```csharp
public class OnboardingData {
    public List<int> CompletedSteps = new();
    public bool IsStepCompleted(int step) => CompletedSteps.Contains(step);
    public void CompleteStep(int step) { if (!IsStepCompleted(step)) CompletedSteps.Add(step); }
}
// On PlayerData: public OnboardingData OnboardingData = new();
```
Saved as part of the normal JSON save after each step completion.

---

## Other UI / Gameplay Changes (same session)

### Squad meter starts empty
`_squadMeter = 0` on `InGameScreen.OnEnable()`. Refills at 1 charge per 3 seconds (capped at `squadMeterImages.Length`).

### First game auto-starts into play mode
In `GameManager.SetUp()`:
```csharp
if (_dataManager.PlayerData.UserData.gamesPlayed == 0)
    await StartGame();
else
    await uiManager.NavigateTo(UIManager.Screens.MainScreen);
```

### In-game unit buttons changed from ButtonPointerComponent to plain Button
`unit1Pointer, unit2Pointer, unit3Pointer` are `Button` (not `ButtonPointerComponent`). Cost check and squad meter deduction happen in `OnUnitButtonPressed(int index)`.

### Progress bar throttled to 10Hz
Moved from `FixedUpdate` to `Update` with a `_progressTimer` accumulator (`>= 0.1f`).

### UnitCamerasController additions
```csharp
// Shows a random full-body unit from the given list and fires the "Look" animator trigger
public void ShowRandomFullBodyUnit(List<BaseUnit.UnitTypes> fromUnits)
// Hides all full-body unit GameObjects
public void HideAllFullBodyUnits()
```
The `Look` trigger is on the `IdleCharacters` animator. Called during matchmaking in `MainScreenUI.OnMatchmakingStarted()`.

---

## Gotchas / Lessons Learned

- **Panel offsetMin/offsetMax are relative to the anchor edges, not the canvas center.** With stretch anchors the effective edge positions are `left = -halfW + offsetMin.x`, etc. Getting this wrong causes all 4 panels to center on the target instead of framing around it.
- **`replace_all` on `SetText` caused infinite recursion** — it replaced `onboardingText.text = message` inside the method itself with `SetText(message)`. Always use targeted edits for method internals.
- **Full-cover steps**: only `panelTop` is activated (stretched to cover everything). Activating all 4 panels makes the screen too dark; activating none makes it too light.
- **Step independence**: each screen only calls its own range method with explicit `_maxStep`. Steps don't bleed between screens. Completing a later step never affects earlier uncompleted ones.
- **`ShowMainMenuPlaySteps` guard**: check `IsStepCompleted(3)` before calling it, otherwise it fires after the very first game before the castle tutorial has run.
