# Block Registry Tests

**Status: PARTIAL** — executable test harness is not established yet.

## Registry

- valid canonical IDs register successfully
- duplicate IDs are rejected
- malformed IDs are rejected
- runtime IDs are deterministic for identical manifests
- runtime IDs are not persisted as canonical identity
- registry lookup works in both directions
- registry rejects mutation after freeze
- registration order does not affect deterministic runtime assignment

## Block states

- defaults resolve correctly
- valid enum values are accepted
- invalid enum values are rejected
- boolean values are type-checked
- unknown properties are rejected
- canonical serialization is stable
- equivalent states produce the same canonical key
- state expansion is bounded

## Data validation

- malformed JSON fails clearly
- missing required fields fail validation
- invalid numeric values fail validation
- duplicate block definitions fail validation
- schema-version incompatibility is explicit
