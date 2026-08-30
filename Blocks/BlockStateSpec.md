# MineWorld Block State

**Status: PARTIAL** — state contract defined; executable storage/validation implementation follows.

## Principle

A block definition describes what a block is. A block state describes one legal runtime configuration of that definition.

Example concept:

`mineworld:oak_log` + `{axis: y}`

State values are typed, finite, schema-defined, canonicalized, and immutable after construction.

## State schema

Each property declares:

- stable property name
- scalar type (`bool`, `int`, `enum`, or bounded value)
- allowed values/range
- default value
- serialization order

## Canonical state

Properties are serialized in schema order. Missing properties resolve to defaults. Unknown properties and invalid values are rejected rather than silently discarded.

Equivalent logical states must produce identical canonical hashes.

## Runtime representation

Use compact state IDs internally where beneficial. A state ID is scoped to the registry/content manifest and is not a persistent world identifier.

Recommended path:

`BlockId + StateSchema → canonical state key → runtime StateId`

## Mutation

State objects are immutable. Block updates create a new state and enqueue the appropriate world update rather than mutating shared definitions.

## Persistence

Persistence must encode canonical block ID and state values/version. Never serialize memory addresses, renderer handles, or process-local runtime IDs as the sole source of truth.

## Examples

### Air

`mineworld:air`

No properties.

### Grass block

`mineworld:grass_block`

Potential future states:

- `snowy: false | true`

### Oak log

`mineworld:oak_log`

- `axis: x | y | z`

### Furnace-like functional block

Potential properties:

- `facing: north | south | east | west`
- `lit: false | true`

## Validation requirements

- schema exists
- every property has a legal value
- default values are legal
- no duplicate property names
- canonical ordering is deterministic
- state count is bounded before runtime expansion
