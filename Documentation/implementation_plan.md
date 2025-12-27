# Fast Harvest & UI Overhaul Plan

## Goal
Implement a "Fast Harvest" test mode to allow rapid verification of the game loop, and overhaul the UI to ensure the Harvest mechanic is visible and functional.

## Changes Implemented

### Game Logic (Fast Mode)
#### [MODIFY] [HurmaTree.cs](file:///c:/Users/tugba/Downloads/suheybindirlenler/hurma/HurmaOyunu/hurma/Assets/Scripts/HurmaTree.cs)
- **Growth Acceleration**: Constrained growth days to 1 (Sapling), 2 (Young), 3 (Mature) in `Start()`.
- **Water Logic**: Disabled water consumption (`WaterConsumptionPerDay = 0`) to prevent death during fast testing.
- **Flowering/Fruiting**: Bypassed strict Moon Phase requirements when in fast mode. Tree now transitions `Mature -> Flowering -> Fruiting` in consecutive days.
- **Visuals**: Added an orange color tint to the tree model when in `Fruiting` stage for visual feedback.

#### [MODIFY] [TimeManager.cs](file:///c:/Users/tugba/Downloads/suheybindirlenler/hurma/HurmaOyunu/hurma/Assets/Scripts/TimeManager.cs)
- **Day Cycle**: Reduced `dayDurationInSeconds` from 60s to 2s.

### User Interface (UI)
#### [MODIFY] [UIManager.cs](file:///c:/Users/tugba/Downloads/suheybindirlenler/hurma/HurmaOyunu/hurma/Assets/Scripts/UIManager.cs)
- **Hasat (Harvest) Button**: 
    - Auto-creates a "HASAT ET" button if missing.
    - Button appears only when tree is in `Fruiting` stage.
- **Inventory Display**:
    - Auto-creates "Rızık" (Inventory) text if missing.
    - Updates dynamically when harvest occurs.
    - Adjusted position to `(-20, -150)` to avoid overlapping the "Next Day" button.
- **Font Fix**: Replaced deprecated `Arial.ttf` with `LegacyRuntime.ttf` to resolve runtime errors.

## Verification
- **Fast Loop**: Game cycle completes in ~1 minute.
- **Visuals**: Tree turns orange when ready.
- **UI**: Harvest button appears, Rızık counter increments, no overlaps.
