# Environment Context Tests

## Status
PARTIAL — test specification pending executable test framework.

## Required cases

1. Default context is deterministic.
2. Underground state suppresses sky-dependent ambience.
3. Submerged state selects water context.
4. Weather strength is clamped to its valid range.
5. Biome changes do not mutate world state.
6. Presentation randomness does not affect simulation state.
7. Missing optional biome/weather data uses deterministic fallback.
8. Context snapshots remain read-only to consumers.

## Acceptance

- All cases become automated tests when the runtime implementation exists.
- No test may depend on external network services or proprietary game assets.
