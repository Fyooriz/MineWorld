# ADR-0002 — Chunk and Block Identity

- Status: ACCEPTED
- Date: 2026-08-31
- Scope: P0/P1

## Decision

MineWorld uses `ChunkCoordinate` + 16×16×16 `ChunkSection` storage as the canonical voxel primitive. A block is identified by `BlockId`; storage-local palette/index values are never global block identity.

## Problem

The prototype used byte constants in Playable while Core already defined stable `BlockId` values and section abstractions.

## Rationale

Typed identifiers prevent accidental conflation of storage indices and content identity. Section granularity keeps the storage model scalable while allowing P0 to remain 64 blocks high.

## Trade-offs

P0 still uses simple fixed-size arrays internally. Palette compression is deferred until measured memory pressure justifies it.

## Consequences

- Core block IDs are authoritative.
- Content registries define block semantics.
- Playable never assigns independent block IDs.
- Future block states can be layered over `BlockId` without changing the chunk contract.

## Verification condition

All world generation, mutation, persistence, tests, and future networking paths use the Core block identity contract.
