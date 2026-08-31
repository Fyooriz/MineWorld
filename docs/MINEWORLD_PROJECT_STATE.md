# MineWorld Project State

Snapshot date: 2026-09-01
Baseline branch: `main`
Working branch: `feature/p0-save-schema-player-restore`

## Canonical state vector

| Field | Status |
|---|---|
| Requested | Verify CI on the current head; harden persistence-safe streaming tests; expand deterministic generation coverage to full-chunk snapshots under Ω-MINEWORLD FRONTIER v50.0 |
| Decided | Verification takes precedence over feature breadth; keep authoritative world state separate from render/runtime cache state |
| Implemented | Versioned P0 save schema; legacy v0 compatibility; atomic save replacement; player identity/health/inventory/position restore; bounded chunk streaming; canonical horizontal chunk coordinates; chunk lifecycle gate; cancellation-aware chunk generation; full-chunk deterministic snapshot matrix; persistence-safe streaming regression coverage |
| Verified | Prior CI evidence: Playable build/runtime boot, MineWorld runtime smoke, performance gate, and real-input E2E all passed on the previously observed PR merge commit `3e92afe...` |
| Current head | `82ed92ccb6c076c1e0093973a898277895cf05ad` |
| Current CI | No workflow run or combined status is currently exposed for head `82ed92c...`; therefore current-head CI is **NOT VERIFIED** |
| Latest known CI defect | Core test failure came from a single-voxel generator-version assertion; the sample returned `Air` for both versions, so the test did not validly observe a version difference |
| E2E status | Previous E2E run passed with focused window, craft input, save input, and persisted state; it was executed against merge commit `3e92afe...`, not the current head |
| Merge status | PR #3 remains open, unmerged, and GitHub currently reports `mergeable=false`; compare against `main` shows `behind_by=0`, so this is not caused by the branch being behind base |
| Remaining | Obtain a CI run for the current head; verify the full deterministic matrix and persistence-safe streaming suite; do not merge before critical gates are green |
| Merge policy | Do not merge PR #3 while any critical verification gate is red or unverified |

## P0 observations

The repository contains a substantial P0 implementation including .NET 8 core/playable/test projects, fixed-step simulation, seeded voxel terrain, chunked storage, block interaction, inventory/crafting, entity runtime, persistence, rendering, and runtime CI workflows.

P0 save data carries `SaveVersion`; version `1` is current and legacy `0` remains readable. Unknown versions and missing block lists are rejected. Saves use temporary-file replacement. Player state restores identity, health, inventory, and persisted position when present.

## P1 world/chunk progress

P1 has a canonical horizontal coordinate value object with mathematical floor semantics for negative coordinates, bounded runtime chunk streaming with eviction, an explicit lifecycle transition gate, cancellation-aware background generation, full-chunk deterministic snapshot regression coverage, and persistence-safe streaming coverage.

The current runtime contract keeps authoritative world state separate from render/GPU state. Background work follows request → compute off-thread → validate → commit, with stale/cancelled work prevented from becoming authoritative.

## Deterministic generation matrix

A full chunk snapshot currently covers `16 × 64 × 16 = 16,384` block samples. The test matrix covers repeated evaluation, negative and positive chunk coordinates, chunk boundaries, generator-version variation, and evaluation-order independence.

Generator-version tests compare full snapshots instead of relying on one arbitrary voxel sample.

## Persistence-safe streaming matrix

Coverage includes:

- multiple modified boundary chunks surviving memory eviction;
- persisted overrides surviving eviction plus disk save/load;
- unmodified chunks regenerating to the same deterministic baseline after eviction.

Tests use actual `VoxelWorld` and `WorldPersistence` behavior rather than mock storage contracts.

## Verification boundary

Local clone/build remains unavailable in this execution environment because direct repository resolution is not reliable. GitHub CI is therefore the external verifier.

Current status:

**P0 = IMPLEMENTED / CURRENT-HEAD VERIFICATION PENDING**  
**P1 = INITIAL IMPLEMENTATION + TEST COVERAGE / CURRENT-HEAD VERIFICATION PENDING**
