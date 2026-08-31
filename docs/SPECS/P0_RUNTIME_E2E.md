# P0 Runtime E2E Specification

## Purpose

Verify the real desktop input path from OS event → Raylib/GLFW → `InputState` → player action → persistence.

## Preconditions

- Ubuntu runner with Xvfb/Openbox.
- Playable builds successfully.
- MineWorld runtime creates a window titled `MineWorld P0`.

## Test sequence

1. Start runtime in runtime-E2E mode.
2. Find and focus the MineWorld window.
3. Hold and release `C`; verify `craft=true` is observed.
4. Hold and release `F5`; verify `save=true` is observed.
5. Verify `saves/runtime-e2e.json` exists.
6. Verify player name `RuntimeE2E` is persisted.
7. Verify crafted `core:dirt` is persisted.

## Failure interpretation

- Window not found: startup/graphics failure.
- Window not active: focus/window-manager failure.
- Input not observed: OS-to-Raylib input delivery failure or runtime input polling failure.
- Input observed but state missing: gameplay/persistence failure.

## Anti-flake rule

For edge-triggered key tests, synthetic input uses explicit key-down and key-up with a short hold interval. This keeps the event present across multiple render/simulation polls instead of relying on a single instantaneous event dispatch.

## Verification status

The baseline run on 2026-08-31 reached an active window but failed at craft input observation. This is verified evidence from GitHub Actions, not an inferred application crash.
