# MineWorld

**Build the world. Change the rules.**

MineWorld is an original, modular 3D voxel sandbox project designed around a deterministic simulation core, data-driven content, extensibility, and future authoritative multiplayer.

## Project Status

**P0 — Vertical Slice: IN PROGRESS**

The first milestone establishes the project architecture and a small playable-core foundation before the larger systems are added.

### P0 scope

1. Player
2. Camera
3. World
4. Chunk
5. Block
6. Mining / building
7. Inventory
8. Crafting
9. Basic entity
10. Save / load

A component is not marked complete until implementation, integration, tests, and verification exist.

## Architecture

The repository is organized by domain rather than by one giant game script:

```text
MineWorld/
├── Engine/
├── Renderer/
├── World/
├── Terrain/
├── Chunks/
├── Blocks/
├── Items/
├── Inventory/
├── Crafting/
├── Entities/
├── AI/
├── Combat/
├── Physics/
├── Fluids/
├── Farming/
├── Automation/
├── Dimensions/
├── Structures/
├── Quests/
├── Economy/
├── Factions/
├── Vehicles/
├── UI/
├── Audio/
├── Networking/
├── Server/
├── Modding/
├── Scripting/
├── SaveSystem/
├── Tools/
├── Tests/
└── MineWorldFeatures/
```

See `ARCHITECTURE.md` for the initial boundaries and dependency rules.

## Ruleset

`MINEWORLD_RULESET.md` is the canonical gameplay-rules document. Conflicting behavior must be resolved there and covered by automated tests.

## Originality

MineWorld may implement familiar sandbox gameplay concepts, but its source code, assets, identity, content, and technical implementation are developed independently. No proprietary third-party game source or assets are part of this repository.

## Development Workflow

```text
SPEC → ARCHITECT → IMPLEMENT → INTEGRATE → TEST → PROFILE → OPTIMIZE → DOCUMENT
```

## Milestones

| Milestone | Status |
|---|---|
| P0 Engine + playable voxel prototype | IN PROGRESS |
| P1 World + blocks + player + inventory | PLANNED |
| P2 Survival + crafting + entities | PLANNED |
| P3 AI + combat + progression | PLANNED |
| P4 Automation + structures + dimensions | PLANNED |
| P5 Multiplayer + dedicated server | PLANNED |
| P6 Modding + creator tools | PLANNED |
| P7 MW-X01–MW-X30 | PLANNED |
| P8 Optimization + QA | PLANNED |
| P9 Release preparation | PLANNED |

## License

License and contribution policy will be defined before external contributions are enabled.
