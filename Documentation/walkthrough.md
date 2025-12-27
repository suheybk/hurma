# Fast Harvest & UI Update Walkthrough

## Overview
This update implements a "Fast Harvest" mode to facilitate rapid testing of the Hurma game loop and addresses several UI/UX issues to ensure the harvest mechanic is accessible.

## Key Changes
1.  **Fast Harvest Mode**:
    -   **Speed**: Game days now last 2 seconds (down from 60s).
    -   **Growth**: Trees reach maturity in 3 game days.
    -   **Logic**: Flowering and Fruiting stages trigger immediately after maturity, bypassing long Lunar Cycle waits.
    -   **Survival**: Water consumption disabled to prevent tree death during fast forwarding.

2.  **UI Enhancements**:
    -   **"HASAT ET" Button**: Automatically appears when the tree bears fruit.
    -   **Inventory "Rızık"**: A counter (e.g., "Rızık: 5 Hurma") now appears in the top-right corner to track harvested dates.
    -   **Overlap Fix**: Moved the Inventory text down to avoid overlapping the "Next Day" button.

3.  **Visual Feedback**:
    -   The Hurma Tree now tints **orange** when in the `Fruiting` stage, providing clear visual indication that it is ready for harvest.

4.  **Bug Fixes**:
    -   Fixed `ArgumentException` by identifying and replacing the missing `Arial.ttf` with `LegacyRuntime.ttf`.

## How to Test
1.  Open the **Hurma** scene in Unity.
2.  Press **Play**.
3.  Watch the tree grow rapidly over ~6-10 seconds.
4.  Observe the tree turning orange and the "HASAT ET" button appearing.
5.  Click "HASAT ET".
6.  Verify the "Rızık" counter increases and the tree resets to `Mature` state.

> [!NOTE]
> All changes are currently on the `test/fast-harvest` branch.
