# ADR-0001 — Canonical Simulation Boundary

- Status: ACCEPTED
- Date: 2026-08-31
- Scope: P0/P1

## Decision

`MineWorld.Core` is the canonical simulation authority. Playable, persistence, networking, rendering, tools, and future server code must consume Core contracts rather than maintaining alternate gameplay/world implementations.

## Problem

The repository contained a Core world model and a separate Playable `VoxelWorld`/`VoxelChunk` model. This creates two potential truths for blocks, chunks, and generation.

## Rationale

A single simulation model is required for deterministic behavior, future authoritative multiplayer, consistent tests, and safe persistence. Bedrock/Java references are used as engineering evidence for separation of representations, not copied implementations.

## Trade-offs

The Playable prototype must be refactored now. This creates short-term migration cost but prevents larger divergence later.

## Consequences

- Core owns world/chunk/block state.
- Playable may own render-derived state and input collection only.
- Persistence uses explicit DTOs.
- Network uses validated command/snapshot DTOs.
- Render state is derived and disposable.

## Verification condition

No second authoritative voxel world implementation may exist after P0 consolidation.
