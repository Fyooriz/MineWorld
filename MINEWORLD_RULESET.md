# MineWorld Ruleset

Version: 0.2.0
Status: ACTIVE BASELINE

## Purpose

This document defines the canonical behavior of MineWorld systems. It is the single source of truth when multiple inspirations or implementations could otherwise diverge.

## Core Principles

1. MineWorld is an original voxel sandbox game.
2. No proprietary source code, assets, names, branding, or other protected materials are copied from third-party games.
3. Simulation state is authoritative and deterministic where practical.
4. Gameplay rules are data-driven and extensible.
5. Server authority is the default for multiplayer-sensitive state.
6. Rules are implemented once and reused by client, server, tests, and tools where architecture permits.
7. Reference material is used to understand concepts and differences, not to reproduce third-party implementation details.

## World Baseline

- World coordinates use integer voxel coordinates for blocks.
- A chunk is a horizontal 16×16 column composed of 16×16×16 vertical sections.
- Section coordinates are local `x=0..15`, `y=0..15`, `z=0..15`.
- A world seed deterministically initializes terrain generation.
- Generation is reproducible for the same generator version, seed, and configuration.
- Negative coordinates use mathematical floor division/modulo when mapping world voxels to chunks.
- Chunk streaming is a runtime concern and must not redefine gameplay rules.

## Blocks

- A block has a stable identifier and optional state properties.
- Block definitions are data-driven.
- Compact section storage may use palette indices internally; palette indices are never treated as universal block identifiers.
- Mining and placement are validated against the active ruleset.
- Unknown block identifiers fail safely rather than corrupting world state.
- Runtime/render representations may differ from persistence representations.

## Player

- Player movement and interaction are simulation state, not presentation state.
- Mining and placement must eventually validate reach, target block, permissions, collision, and inventory where applicable.
- P0 uses a simple first-person controller; full collision and inventory-backed placement remain later milestone work.

## Simulation and Rendering

- Simulation state is authoritative and renderer-independent.
- Rendering consumes derived state and must not become an alternate world-state owner.
- Fixed-step simulation is the target architecture; frame rendering may run at a different rate.
- Chunk meshing/culling may rebuild derived render data when blocks change.

## Persistence

- Save data must include a format/version identifier.
- Loading validates version and required fields before applying state.
- Future schema migrations must be explicit and testable.
- Persistence formats are intentionally separated from runtime world objects.

## Multiplayer

- The authoritative server owns persistent world state and validates consequential player actions.
- Clients may predict presentation, but authoritative state wins on reconciliation.
- Network schemas will have explicit versions rather than assuming one immutable protocol.

## Data-Driven Content

- Blocks, items, recipes, entities, world-generation configuration, and future creator content should use versioned data definitions.
- Content loading is separate from world persistence parsing.
- Public modding contracts must be versioned independently from internal implementation details.

## Reference-Informed Decisions

The supplied Java and Bedrock technical references establish several useful engineering patterns that are now adopted by MineWorld:

1. `16×16×16` is the fundamental vertical section storage unit.
2. Chunk columns aggregate multiple vertical sections.
3. Palette compression is an implementation technique, not a block identity system.
4. Persistent, runtime, and network state should remain distinct representations.
5. Seeded world generation must include generator version/configuration in reproducibility rules.
6. Runtime world state and render state are separate layers.
7. Multiplayer should converge on server-authoritative validation.
8. Version-sensitive external formats/protocols must not be hardcoded as universal truths.
9. Negative coordinate mapping must use floor semantics.

See `docs/REFERENCE_MAPPING.md` for the associated design record.

## Conflict Resolution Procedure

When two behaviors conflict:

1. Record the conflicting behaviors.
2. Identify whether the difference is caused by version, platform, context, or actual design variation.
3. Select one canonical MineWorld behavior.
4. Document the rationale here.
5. Implement the behavior through the shared rules/service layer.
6. Add an automated regression test.

## Current Decisions

### Chunk sections
MineWorld P0 uses 16×16×16 sections inside a 16×16 chunk column. The current prototype world height is 64 blocks (four sections) to keep the playable slice small while preserving the scalable storage model.

### Block editing
P0 supports direct ray-based mining and placement using MineWorld-native block IDs. Drops, tool requirements, durability, permissions, and inventory-backed placement are intentionally deferred to later milestones.

### Rendering
P0 uses visible-voxel culling but still issues individual cube draw calls. A proper chunk mesh/GPU buffer path is required before large render distances are considered production-ready.

## Versioning

Ruleset changes that alter saved data, simulation outcomes, networking semantics, or public modding contracts require a ruleset version bump and regression coverage.
