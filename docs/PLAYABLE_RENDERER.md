# MineWorld Playable Renderer

## Status

**P0 — INTEGRATED**

The playable slice now separates frame orchestration, input sampling, rendering, and gameplay state.

## Runtime flow

```text
Window / GPU context
        |
        v
   InputState.Poll()
        |
        v
 PlayerController.Update()
        |
        v
 VoxelWorld.StreamAround()
        |
        v
 IRenderer.BeginFrame()
        |
        v
 IRenderer.RenderWorld()
        |
        v
 IRenderer.DrawHud()
        |
        v
 IRenderer.EndFrame()
```

## Renderer boundary

`IRenderer` is the application-facing renderer contract. `RaylibRenderer` is the current implementation and owns the graphics/window lifecycle.

This keeps the game loop from depending on concrete renderer calls and leaves room for a future lower-level renderer without rewriting gameplay orchestration.

## GPU path

Raylib creates the graphics context and submits 3D draw calls to the GPU. The current voxel path intentionally remains simple for P0: visible blocks are submitted as cube geometry. Chunk meshing and batched GPU buffers remain a later optimization milestone.

## Input boundary

`InputState` samples keyboard/mouse state once per frame. Player movement, camera look, jump, mining, placement, and save actions consume that snapshot rather than polling the window directly.

This establishes deterministic input ownership and makes later rebinding, controller support, replay/input recording, and server/client input transport easier to add.

## Verification note

The repository integration was completed through GitHub. Local compilation could not be executed in this environment because outbound DNS/network access to GitHub is unavailable, and no GitHub Actions run is configured for the resulting commit. Therefore this milestone is not claimed as build-verified here.
