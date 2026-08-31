# MineWorld Project State

Snapshot date: 2026-09-01
Baseline branch: `main`
Working branch: `feature/p0-save-schema-player-restore`

## Canonical state vector

| Field | Status |
|---|---|
| Requested | Resolve PR #3 runtime-input E2E; verify P0; continue P1 world/chunk architecture under Ω-MINEWORLD FRONTIER v50.0 |
| Decided | Keep the current modular runtime boundary; fix verified P0/P1 correctness gaps before adding gameplay breadth |
| Implemented | Versioned P0 save schema; legacy v0 compatibility; atomic save replacement; player identity/health/inventory/position restore; bounded chunk streaming; canonical horizontal chunk coordinates; chunk lifecycle gate; cancellation-aware chunk generation; deterministic/persistence-safe P1 regression coverage |
| Verified | Repository/PR state and prior CI build, MineWorld CI, Runtime Smoke, and performance-gate results |
| Current CI | Latest head `8a0c0428...` has new `.NET`, `MineWorld CI`, `Runtime Smoke`, and `Player Runtime E2E` runs currently in progress |
| Critical E2E finding | The real-input job did observe `craft=True`, but the marker arrived in the redirected log after the original 8-second watchdog, causing a false-negative timing boundary |
| E2E fix | Increased runtime frame budget and observation windows while keeping real window focus plus keydown/keyup input synthesis |
| Remaining | Obtain final CI result for latest head; only then treat P0/P1 verification gates as green |
| Merge policy | Do not merge PR #3 while any critical verification gate is red or unverified |

## P0 observations

The repository already contains a substantial P0 implementation including .NET 8 core/playable/test projects, fixed-step simulation, seeded voxel terrain, chunked storage, block interaction, inventory/crafting, entity runtime, persistence, rendering, and runtime CI workflows.

P0 save data carries `SaveVersion`; version `1` is current and legacy `0` remains readable. Unknown versions and missing block lists are rejected. Saves use temporary-file replacement. Player state restores identity, health, inventory, and persisted position when present.

## P1 world/chunk progress

P1 now has a canonical horizontal coordinate value object with mathematical floor semantics for negative coordinates, bounded runtime chunk streaming with eviction, an explicit lifecycle transition gate, cancellation-aware background generation, deterministic terrain regression tests, and persistence round-trip coverage after streaming eviction.

The architecture contract keeps authoritative world state separate from render/GPU state and defines the worker pattern as request → compute off-thread → validate → commit.

## Verification boundary

Local clone/build remains unavailable in this execution environment because direct repository resolution is not reliable. GitHub CI is therefore the external verifier.

Current status:

**P0 = IMPLEMENTED / AWAITING FINAL CI VERIFICATION**  
**P1 = INITIAL IMPLEMENTATION / AWAITING FINAL CI VERIFICATION**
