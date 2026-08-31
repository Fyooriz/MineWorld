# MineWorld Chunk Runtime Specification

## Status
P1 — canonical lifecycle and deterministic data contract.

## Goals

- deterministic chunk identity from world seed + chunk coordinates
- bounded memory footprint
- independent generation and streaming lifecycle
- safe persistence boundary
- no renderer dependency in world data

## Chunk identity

`ChunkKey = (dimension_id, chunk_x, chunk_z)`

The same seed, dimension, generator version, and chunk coordinates must produce the same generated baseline.

## Canonical lifecycle

The executable runtime is authoritative for lifecycle state names:

`UNLOADED → REQUESTED → GENERATING → LOADED → DIRTY → MESHING → READY → UNLOADING → UNLOADED`

Allowed transitions are explicit and invalid transitions must be rejected. Cancellation/invalidation is represented by transitions back toward `UNLOADING` or by re-entering `DIRTY`; a stale asynchronous result must never become authoritative after ownership has changed.

Generation, meshing, and persistence are separate concerns. `READY` describes presentation readiness; it does not transfer ownership away from the authoritative chunk state.

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

## Determinism

Scheduling order and worker completion order may vary. They must not change generated chunk contents or authoritative simulation results. Re-evaluation of the same deterministic inputs must produce the same baseline data, including at chunk and coordinate boundaries.
