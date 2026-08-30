# MineWorld Block Registry

**Status: PARTIAL** — architecture and data contracts are implemented; runtime registry remains the next executable step.

## Goals

The registry is the authoritative mapping between stable MineWorld block identifiers and immutable block definitions. Chunk storage must reference compact numeric runtime IDs while persistence and network-facing data use stable namespaced IDs.

## Identifier format

`mineworld:<path>`

Rules:
- lowercase ASCII
- namespace and path separated by `:`
- path uses `[a-z0-9._/-]+`
- identifiers are immutable once published
- aliases may resolve legacy names but must not become canonical IDs

Examples:
- `mineworld:air`
- `mineworld:stone`
- `mineworld:dirt`
- `mineworld:grass_block`
- `mineworld:oak_log`

## Registry responsibilities

- register definitions during bootstrap
- reject duplicate canonical IDs
- assign deterministic runtime IDs
- resolve canonical ID → definition
- resolve runtime ID → definition
- validate block states
- expose read-only definitions after bootstrap
- support data-pack/mod registration through a controlled extension phase
- freeze the registry before world simulation begins

## Runtime ID policy

Runtime IDs are process-local optimization handles. They must never be treated as persistent identifiers. Saves store canonical IDs plus validated state properties; network protocols may negotiate runtime mappings per connection/session.

## Definition shape

A block definition contains:

- `id`
- `display_name`
- `hardness`
- `requires_tool`
- `tool_class`
- `solid`
- `opaque`
- `replaceable`
- `flammable`
- `gravity_affected`
- `light_emission`
- `light_filter`
- `collision`
- `interaction_profile`
- `state_schema`
- `tags`
- `version`

Definitions are data-driven and renderer-independent.

## Lifecycle

`DISCOVER → VALIDATE → REGISTER → ASSIGN_RUNTIME_IDS → FREEZE → SERVE`

Registration after `FREEZE` is rejected except through an explicit future content-reload transaction.

## Determinism

Given the same content manifest and registry version, canonical IDs receive the same sorted runtime-ID assignment. Registry ordering must not depend on filesystem enumeration order, hash-map iteration order, or thread scheduling.
