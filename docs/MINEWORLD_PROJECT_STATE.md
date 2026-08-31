# MineWorld Project State

Snapshot date: 2026-08-31
Baseline branch: `main`
Working branch: `feature/p0-save-schema-player-restore`

## Canonical state vector

| Field | Status |
|---|---|
| Requested | CI/debug PR #3; deep P0 audit; start P1 world/chunk architecture under Ω-MINEWORLD FRONTIER v50.0 |
| Decided | Preserve the modular P0 architecture; fix correctness gaps before expanding gameplay; establish P1 world/chunk ownership and coordinate boundaries incrementally |
| Implemented | Save format v1 + legacy v0 compatibility; unsupported-version rejection; missing-block validation; atomic save replacement; player identity/health/inventory restore; player-position persistence; bounded runtime chunk streaming; canonical horizontal chunk conversion; generated-chunk in-flight deduplication until completion is consumed; P1 world/chunk architecture document; regression tests |
| Verified | Repository contents, PR #3 creation, PR diff, historical CI execution, successful build/test/performance jobs on earlier PR commits, and the recorded E2E failure modes |
| Tested | New regression tests are committed; latest head CI is queued and its final result has not yet been observed |
| Failed | Local clone/build remains unavailable because this execution environment could not resolve `github.com`; prior PR Runtime E2E failed during synthetic keyboard delivery/focus handling |
| Why | Local network/DNS limitation; CI failure was in the external input harness rather than build initialization, which succeeded |
| Remaining | Observe latest CI for the new changes; debug any real-input failure that remains; continue P1 chunk lifecycle/generation integration and verification |
| Blocked by | Final CI evidence for the latest head before claiming the branch verified or merging |
| Next | Inspect latest CI result; then implement/test P1 chunk lifecycle, deterministic generation, async cancellation, and persistence-safe unload semantics |

## P0 deep-audit findings

1. The previous runtime E2E failure was not a compile failure. The application built successfully and Raylib initialized under the virtual display. The failure occurred because the test did not reliably deliver/observe the `C` key through Raylib's input polling path.
2. The runtime world previously only accumulated chunks as the player moved. P1 now bounds the loaded set to the configured horizontal radius and removes disposable mesh entries for unloaded chunks.
3. World-to-chunk math is now centralized behind `HorizontalChunkCoordinate`, including negative-coordinate floor semantics.
4. The save boundary previously persisted player identity/health/inventory but not the player's runtime position. Position persistence is now explicit and optional for backward compatibility.
5. Background generation previously removed a chunk from `_inFlight` as soon as it entered the completion queue, allowing duplicate generation requests before commit. `_inFlight` now remains reserved until the completed result is consumed.

## Persistence decision

MineWorld save data carries `SaveVersion`.

- `1` is the current P0 format.
- `0` is accepted as the legacy pre-version JSON shape created before explicit versioning.
- Unknown versions are rejected.
- Save writes use a temporary file and replacement to reduce partial-file risk.
- Player identity, health, inventory, and (when present) runtime position are persisted/restored.

This remains a MineWorld-native save model; Minecraft Java/Bedrock references are treated as engineering evidence only and are not copied as proprietary implementation or game-specific formats.

## P1 status

`docs/P1_WORLD_CHUNK_ARCHITECTURE.md` establishes the current architecture baseline for:

- world/chunk ownership boundaries;
- horizontal chunk coordinate semantics;
- chunk lifecycle states;
- bounded streaming;
- authoritative mutation vs render cache separation;
- asynchronous generation/meshing commit boundaries;
- memory/resource governance;
- future extension points without finalizing unapproved mechanics.

`HorizontalChunkCoordinate` and P1 regression tests provide the first implementation slice.

**P1 status: ARCHITECTURE BASELINE + INITIAL IMPLEMENTATION / NOT VERIFIED**

## Verification boundary

The environment could not clone the repository directly and therefore did not produce a local build/test log. GitHub Actions is the external verifier.

The latest head has queued CI runs for `.NET`, Runtime Smoke, Player Runtime E2E, and MineWorld CI. Until their final conclusions are observed:

**Overall status: PARTIAL / NOT VERIFIED**
