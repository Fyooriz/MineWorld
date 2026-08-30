# Chunk Streaming

## Status
PARTIAL — scheduling contract; runtime worker implementation pending.

## Priority order

1. player-critical chunks
2. chunks needed for collision/interaction
3. nearby simulation chunks
4. visible terrain chunks
5. prefetch ring

## Rules

- Never block the main simulation tick on synchronous terrain generation.
- Deduplicate requests by `ChunkKey`.
- Cancel stale low-priority work when safe.
- Limit concurrent generation jobs using a runtime-configurable budget.
- Keep authoritative world state separate from render mesh lifetime.
- Unload only when no gameplay system requires the chunk.

## Backpressure

When generation falls behind, reduce prefetch before reducing player-critical coverage. When memory pressure rises, release render-only resources before unloading simulation-required chunks.

## Determinism

Scheduling order may vary, but generated chunk contents and authoritative simulation results must not depend on worker completion order.
