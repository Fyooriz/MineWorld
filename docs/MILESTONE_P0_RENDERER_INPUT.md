# P0 Milestone — GPU Renderer, Game Loop, Input

## Status

**IMPLEMENTED — BUILD VERIFICATION PENDING**

## Completed

- Added `IRenderer` abstraction.
- Added `RaylibRenderer` as the GPU-backed window/render implementation.
- Added `InputState` to snapshot keyboard/mouse input once per frame.
- Added `GameLoop` with clamped frame delta and 60 Hz fixed-step simulation.
- Routed `Program` through the new orchestration layer.
- Decoupled `PlayerController` from direct window polling.
- Added explicit mouse-delta consumption so a slow frame cannot apply the same mouse movement multiple times.

## Architecture

```text
Raylib window/input
       |
       v
  InputState
       |
       v
   GameLoop -----> VoxelWorld
       |
       v
PlayerController
       |
       v
  IRenderer
       |
       v
RaylibRenderer -> GPU
```

## Verification

Changes were committed to `main` through GitHub and the affected source files were re-read after integration. No local .NET build/test execution was available in the current tool environment, so this milestone is not marked build-verified.

## Next

Chunk-level mesh generation, dirty-mesh tracking, GPU upload/update caching, and frustum-aware render selection.
