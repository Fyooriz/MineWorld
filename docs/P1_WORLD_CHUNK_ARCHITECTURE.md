# MineWorld P1 — World / Chunk Architecture

Status: ARCHITECTURE BASELINE — implementation starts incrementally
Date: 2026-08-31

## Purpose

Define the P1 world/chunk boundary without inventing final gameplay mechanics. The architecture must preserve the current MineWorld ruleset while allowing world height, biomes, structures, dimensions, and simulation tiers to evolve later.

## Source of Truth

- `MINEWORLD_RULESET.md` is authoritative for current world semantics.
- Existing runtime behavior is treated as implementation evidence, not as permission to freeze accidental behavior.
- Bedrock/Java references are external engineering evidence only; MineWorld does not reproduce their proprietary storage or runtime formats.

## Canonical Coordinate Model

A world block coordinate is represented as `(x, y, z)`.

Horizontal chunks are identified by `(chunkX, chunkZ)` and currently use a width/depth of 16 blocks. Local coordinates are normalized to `[0, 15]` using mathematical floor division so negative world coordinates remain consistent.

`y` is a world vertical coordinate. The current prototype uses `0 <= y < 64`; P1 must not hard-code that value into generic chunk-coordinate infrastructure.

## Ownership Boundaries

### World Runtime

Owns:
- world seed
- generated/persisted block state
- chunk lookup
- authoritative block reads/writes
- chunk lifecycle requests
- mesh invalidation notifications

Does not own:
- GPU resources
- UI state
- network transport
- player input polling

### Chunk

Owns:
- immutable chunk identity `(chunkX, chunkZ)`
- block storage for its vertical sections
- dirty/change state needed by persistence and rendering coordination
- lifecycle state needed by streaming

A chunk does not own its renderer, AI scheduler, or network connection.

### Chunk Generation

Input:
- world seed
- generator/ruleset version
- chunk coordinate

Output:
- generated chunk state

Generation must be deterministic wherever the ruleset requires deterministic results. Generation code must not depend on render state.

### Persistence

Persistence stores persistent world facts, not GPU/runtime caches. Save schema versioning and migration belong at the persistence boundary.

### Rendering

Rendering consumes chunk snapshots/mesh-ready data. Render caches are disposable and must never become the authoritative source of world state.

## Chunk Lifecycle

Target lifecycle:

`UNLOADED`
→ `REQUESTED`
→ `GENERATING`
→ `LOADED`
→ `DIRTY`
→ `MESHING`
→ `READY`
→ `UNLOADING`
→ `UNLOADED`

Exact runtime state names are implementation details and may evolve.

Important invariants:

1. A chunk has one stable coordinate identity.
2. A chunk is not visible to gameplay until its state is valid.
3. Rendering may lag behind authoritative world state.
4. Persistence must survive renderer failure.
5. Unloading a chunk must not lose modified persistent state.
6. Loading/unloading must not alter deterministic generation results.

## Streaming Policy

P1 streaming should maintain a bounded active set derived from player relevance and configured horizontal radius.

For the current prototype:

- retain the configured radius around the player;
- unload chunks outside the radius after state is safely represented by generation + overrides/persistence;
- invalidate disposable mesh cache entries for unloaded chunks;
- avoid loading arbitrary distant chunks merely because a renderer asks for them.

Future priority rings may use different generation/simulation fidelity, but those are not finalized here.

## Change Propagation

Authoritative block mutation:

`Validate`
→ `Apply to Chunk`
→ `Mark Chunk Dirty`
→ `Invalidate Affected Neighbor Meshes`
→ `Queue Persistence Change`
→ `Notify Runtime Consumers`

The world state change is authoritative before rendering catches up.

## Concurrency

Generation and meshing may run asynchronously, but authoritative world mutation remains serialized through an explicit ownership boundary.

Background jobs must not mutate live authoritative state without a defined commit step.

Recommended pattern:

`Request → Compute Off-Thread → Validate Result → Commit on World Thread`

Cancellation is expected for obsolete generation/meshing work.

## Memory / Resource Governance

Track at minimum:
- loaded chunk count
- generated chunk count
- pending generation jobs
- pending mesh jobs
- mesh-cache entries
- approximate chunk memory

Degradation order should prefer reducing optional/background work before breaking player-critical state.

## P1 Extension Points

The architecture intentionally leaves room for:
- multiple chunk storage implementations
- configurable vertical section counts
- biome/terrain generation stages
- structures and decorators
- block-state storage beyond the current byte prototype
- lighting data
- fluid state
- world events
- multiple dimensions
- server-authoritative world replication

These are extension points, not claims that those systems are implemented.

## Non-Goals

This document does not finalize:
- gameplay-specific biome mechanics
- final world height
- final terrain noise algorithm
- final lighting engine
- final save compression/container
- network packet format
- MW-X01..MW-X30 mechanics

## Verification Gate

P1 world/chunk architecture is considered verified only after:

- coordinate conversion tests, including negative coordinates;
- deterministic generation tests;
- chunk load/unload correctness tests;
- boundary mesh invalidation tests;
- persistence round-trip tests;
- failure/cancellation tests for asynchronous generation;
- measured memory/streaming behavior.

Until those tests exist and pass:

**STATUS: ARCHITECTURE BASELINE / NOT VERIFIED**
