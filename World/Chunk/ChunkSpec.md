# MineWorld Chunk Runtime Specification

## Status
PARTIAL — deterministic data contract; executable chunk runtime comes next.

## Goals

- deterministic chunk identity from world seed + chunk coordinates
- bounded memory footprint
- independent generation and streaming lifecycle
- safe persistence boundary
- no renderer dependency in world data

## Chunk identity

`ChunkKey = (dimension_id, chunk_x, chunk_z)`

The same seed, dimension, generator version, and chunk coordinates must produce the same generated baseline.

## Lifecycle

`UNLOADED → GENERATING → GENERATED → LOADED → DIRTY → SAVING → LOADED → UNLOADED`

Generation and persistence must be cancellable where the runtime supports it. A failed generation/save operation must not silently mark the chunk as valid.

## Data ownership

Chunk owns:

- block state storage
- block metadata required by simulation
- generation version
- dirty state
- local entity references where applicable

Chunk does not own:

- renderer meshes
- audio instances
- UI state
- network connections

## Streaming

The world streamer decides which chunks are needed from player/view distance and priority. Generation, meshing, and persistence are separate jobs so a slow renderer does not block authoritative world simulation.

## Persistence

Saved chunk data must include a format version. Unknown future fields should be safely ignored where possible; incompatible versions must fail explicitly rather than corrupting state.
