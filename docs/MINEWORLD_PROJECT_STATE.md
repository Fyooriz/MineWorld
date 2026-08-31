# MineWorld Project State

Snapshot date: 2026-09-01
Baseline branch: `main`
Working branch: `feature/p0-save-schema-player-restore`

## Canonical state vector

| Field | Status |
|---|---|
| Requested | Resolve PR #3 runtime-input E2E; verify P0; continue P1 world/chunk architecture under Ω-MINEWORLD FRONTIER v50.0 |
| Decided | Keep the current modular runtime boundary; fix verified P0/P1 correctness gaps before adding gameplay breadth |
| Implemented | Versioned P0 save schema; legacy v0 compatibility; atomic save replacement; durable pre-replacement flush; player identity/health/inventory/position restore; bounded chunk streaming; canonical horizontal chunk coordinates; chunk lifecycle gate; cancellation-aware chunk generation; deterministic full-chunk snapshot matrix; persistence-safe streaming tests; adversarial lifecycle/generation failure/shutdown tests |
| Verified | Prior CI build/runtime smoke/performance and real-input E2E on the older merge commit; current-head static audit is complete |
| Current head | `55d50b36...` |
| Current-head CI | No workflow run/status is visible for the current implementation head; current-head CI remains unverified |
| Critical E2E finding | The earlier real-input gate observed focused-window craft/save input and persisted state successfully after extending the observation window; the original failure was a watchdog timing boundary |
| Persistence audit | Temporary saves now flush through the writer and underlying stream before replacement; crash-recovery journaling/backup generations remain deferred |
| Adversarial finding | Generation failures no longer kill the worker pool; dequeued chunk keys are retryable; lifecycle invalid transitions are explicitly rejected; shutdown cannot publish a late result |
| Remaining | Obtain CI evidence for `55d50b36...` or a later head; only then treat P0/P1 verification gates as green |
| Merge policy | Do not merge PR #3 while any critical verification gate is red or unverified |

## P0 observations

The repository contains a substantial P0 implementation including .NET 8 core/playable/test projects, fixed-step simulation, seeded voxel terrain, chunked storage, block interaction, inventory/crafting, entity runtime, persistence, rendering, and runtime CI workflows.

P0 save data carries `SaveVersion`; version `1` is current and legacy `0` remains readable. Unknown versions and missing block lists are rejected. Saves use a unique temporary path, write-through output, explicit disk flush, and replacement. Player state restores identity, health, inventory, and persisted position when present.

## P1 world/chunk progress

P1 has a canonical horizontal coordinate value object with mathematical floor semantics for negative coordinates, bounded runtime chunk streaming with eviction, an explicit lifecycle transition gate, cancellation-aware background generation, deterministic full-chunk regression tests, and persistence round-trip coverage after streaming eviction.

Generation failure handling now preserves worker capacity. Streaming dequeue removes the key from the retry-suppression set, allowing failed/obsolete requests to be scheduled again.

## Persistence audit boundary

The attached Java and Bedrock references were used as engineering evidence for dirty-state handling, logical serialization, interrupted/atomic writes, batching/flush behavior, backup/recovery concerns, and separation of persistent world facts from runtime/render caches. MineWorld keeps its native JSON save contract and does not copy Minecraft storage formats.

Current audit conclusions:

- **HARDENED:** explicit schema versioning and legacy read path.
- **HARDENED:** deterministic ordering of persistent collections.
- **HARDENED:** temporary-file replacement plus pre-replacement flush.
- **HARDENED:** persistent override state survives streaming eviction and reload.
- **NOT YET VERIFIED:** crash/fault injection recovery.
- **DEFERRED:** journal/backup generations and database-style dirty-chunk batching.

## Verification boundary

Local clone/build remains unavailable in this execution environment because direct repository resolution is not reliable. GitHub CI is therefore the external verifier.

Known CI evidence from the prior merge-commit run:

- runtime boot: PASS;
- real-input E2E: PASS;
- performance gate: PASS;
- Playable tests: 35/35 PASS;
- Core tests: 42/43 PASS, with the old single-voxel generator-version assertion failing because the selected sample was `Air` for both versions.

That old failure motivated the full-chunk snapshot matrix. It must not be treated as current-head verification.

## Current status

**P0 = IMPLEMENTED / AWAITING CURRENT-HEAD CI VERIFICATION**  
**P1 = HARDENED INCREMENTALLY / AWAITING CURRENT-HEAD CI VERIFICATION**  
**MERGE = BLOCKED UNTIL CRITICAL CI IS GREEN**
