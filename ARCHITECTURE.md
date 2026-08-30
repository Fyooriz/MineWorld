# MineWorld Architecture — P0

## Goals

- Keep simulation independent from rendering where practical.
- Keep domain modules small and testable.
- Make persistence and networking possible without rewriting the simulation model.
- Prefer data-driven definitions over hard-coded content.
- Establish explicit dependency direction.

## Layers

```text
Presentation
  UI / Audio / Renderer
        ↓
Application
  Player / Interaction / Game Services
        ↓
Simulation
  World / Chunks / Blocks / Entities / Inventory / Crafting
        ↓
Foundation
  Math / IDs / Serialization / Events / Time
```

Networking and persistence observe and transport simulation state; they must not silently become alternate gameplay implementations.

## P0 Module Responsibilities

### Engine
Lifecycle, configuration, service composition, fixed-step simulation loop, and global identifiers.

### World
World state, seed, block access, world bounds abstraction, and chunk ownership.

### Chunks
Chunk coordinates, storage, generation state, dirty state, and streaming boundaries.

### Blocks
Stable block IDs, block state, definitions, and interaction contracts.

### Player
Player state, input-independent gameplay actions, and interaction requests.

### Inventory
Stacks, slots, capacity rules, add/remove operations, and validation.

### Crafting
Recipe definitions and deterministic recipe evaluation.

### Entities
Stable entity IDs, transform/state model, lifecycle, and future AI/combat extension points.

### SaveSystem
Versioned serialization and world/player persistence boundaries.

### Tests
Unit and integration tests for deterministic core behavior.

## Dependency Rules

1. Renderer must not own authoritative world state.
2. UI must not mutate simulation state without going through application/simulation APIs.
3. Domain systems should not depend on concrete rendering classes.
4. Save/load operates on explicit serializable state models.
5. Network messages represent validated commands or state snapshots, not arbitrary object graphs.
6. Content definitions should be data-driven and versionable.

## P0 Vertical Slice Contract

The minimum useful loop is:

```text
Input → Player action → World validation → State mutation → Presentation update
                         ↓
                       Save
```

A future multiplayer server can own the same simulation contracts while clients provide presentation and input transport.

## Extension Points

The architecture reserves interfaces/services for:

- world generation
- chunk streaming
- block registry
- item registry
- entity registry
- recipes
- save providers
- network transport
- mod/event API
- MW-X01–MW-X30 feature modules

These are extension points, not claims that the associated systems are already implemented.
