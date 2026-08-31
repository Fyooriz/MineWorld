# P0 Canonical World Specification

## Objective

Consolidate P0 around one authoritative simulation model while preserving the current playable vertical slice.

## Canonical ownership

| Concern | Owner |
|---|---|
| World/chunk/block state | `MineWorld.Core.World.WorldState` |
| Block identity | `MineWorld.Core.World.BlockId` |
| Terrain generation | `MineWorld.Core.World.IWorldGenerator` |
| Player state | Core player state |
| Entity runtime | Core entity runtime |
| Input sampling | Playable |
| Render-derived mesh/cache | Playable |
| Save serialization | Playable persistence boundary for P0 |

## Chunk contract

- 16×16 horizontal chunk footprint.
- 16×16×16 vertical storage sections are the canonical scalable primitive.
- P0 world height remains 64 blocks (four sections).
- Negative coordinates use floor semantics.
- A chunk may be unloaded from memory without losing its persistent delta.

## Block contract

- `BlockId` is the stable runtime identity.
- Raw palette/index representations are storage-local only.
- Unknown block identifiers fail safely.
- Content definitions remain data-driven and versionable.

## World mutation contract

A mutation follows:

`request → validate → mutate → mark derived state dirty`

Mining must not consume or remove a block unless the resulting item can be accepted by the inventory. Failed mutations must leave world and inventory state unchanged.

## Generation contract

Reproducibility is defined by:

`seed + generator ID + generator version + configuration + dimension`

The P0 generator is intentionally simple; architecture must not depend on its terrain richness.

## Streaming contract

Required states are at minimum:

`requested → generating → loaded → active → saving/unloading`

P0 may implement a simple synchronous generator, but the ownership boundary must permit later asynchronous generation and bounded queues.

## Rendering contract

Rendering consumes world state. Rendering must not create a competing authoritative world state.

## Persistence contract

P0 saves require a versioned header and explicit validation. Runtime objects are not the persistence schema.

## Non-goals

P0 does not implement full multiplayer, advanced AI, full physics, dimensions, advanced biome generation, or MW-X01–MW-X30 behavior.

## Done gate

P0 architecture is not considered complete until Core is the only authoritative voxel state owner, automated round-trip tests pass, runtime boot passes, and the real-input E2E workflow is green.
