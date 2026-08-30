# MineWorld Reference Mapping — P0

Status: ACTIVE

## Purpose

This document records architectural conclusions derived from the supplied Java and Bedrock technical references. It is a design record, not a compatibility claim and not copied source material.

## Confirmed design inputs

| Area | Reference observation | MineWorld decision |
|---|---|---|
| Voxel storage | 16×16×16 vertical section/subchunk is a useful storage unit | Use `ChunkSection` with 16³ cells. |
| Chunk model | Chunk columns aggregate vertical sections | Use `VoxelChunk` as a column of sections. |
| Block state | Palette indices represent compact references, not universal block IDs | Keep persistent/runtime/network block representations separate as the engine grows. |
| World generation | Seed + coordinates + generation configuration can be deterministic | Generator version and configuration are part of deterministic world identity. |
| Persistence | Storage format differs from runtime representation | Keep save format behind explicit persistence interfaces. |
| Server model | Multiplayer authority belongs on the server | Gameplay-affecting state will move toward authoritative simulation contracts. |
| Rendering | Runtime world and rendering representation are separate concerns | Renderer may build optimized derived data without owning authoritative world state. |
| Data-driven content | Pack/data definitions provide extensibility patterns | MineWorld content definitions will be data-driven and versioned. |
| Tick loop | Simulation and rendering have separate timing concerns | Keep fixed-step simulation independent from frame rendering. |
| Negative coordinates | Floor division/modulo must be used for chunk mapping | Use mathematical floor division/modulo for negative voxel coordinates. |

## Version-sensitivity rule

The supplied references explicitly emphasize that binary formats, network schemas, storage layouts, registries, and content schemas change over time.

MineWorld therefore does **not** target an undocumented universal compatibility layer. When interoperability becomes an actual requirement, the target version must be named and verified before implementation.

## P0 impact

The current playable prototype now uses:

```text
VoxelWorld
  └── VoxelChunk (16×16 column)
       └── ChunkSection (16×16×16 storage)
```

This preserves a clean path toward:

- palette-compressed storage
- chunk meshing
- chunk streaming
- asynchronous generation
- persistence
- multiplayer replication
- modding APIs

## Intellectual-property boundary

The references are used to understand technical concepts and behavior. MineWorld implementation, identifiers, assets, and architecture remain independently designed. No third-party source code or proprietary game assets are copied into this repository.
