# MineWorld Ruleset

Version: 0.1.0
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

## Baseline Rules

### World
- World coordinates use integer voxel coordinates for blocks.
- Chunks are fixed-size simulation/streaming units.
- A world seed deterministically initializes terrain generation.
- Generation must be reproducible for the same generator version, seed, and configuration.

### Blocks
- A block has a stable identifier and optional state properties.
- Block definitions are data-driven.
- Mining and placement are validated against the active ruleset.
- Unknown block identifiers fail safely rather than corrupting world state.

### Player
- Player movement and interaction are simulation state, not presentation state.
- Mining and placement must validate reach, target block, permissions, and inventory where applicable.

### Persistence
- Save data must include a format/version identifier.
- Loading validates version and required fields before applying state.
- Future schema migrations must be explicit and testable.

### Multiplayer
- The authoritative server owns persistent world state and validates consequential player actions.
- Clients may predict presentation, but authoritative state wins on reconciliation.

## Conflict Resolution Procedure

When two behaviors conflict:

1. Record the conflicting behaviors.
2. Select one canonical MineWorld behavior.
3. Document the rationale here.
4. Implement the behavior through the shared rules/service layer.
5. Add an automated regression test.

## Current Decisions

No source-specific compatibility decisions have been locked yet. The vertical slice establishes MineWorld-native baseline behavior rather than reproducing another game's implementation.

## Versioning

Ruleset changes that alter saved data, simulation outcomes, networking semantics, or public modding contracts require a ruleset version bump and regression coverage.
