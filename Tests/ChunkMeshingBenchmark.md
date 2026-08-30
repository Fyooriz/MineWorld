# Chunk Meshing Benchmark

## Goal

Measure whether greedy meshing and asynchronous scheduling improve real workload behavior before making further optimization claims.

## Scenarios

1. Flat terrain with mostly identical blocks.
2. Mixed terrain with frequent material boundaries.
3. Cave-heavy terrain with high face exposure.
4. Rebuild storm: repeated edits to the same chunk.
5. Streaming window: many chunks entering/leaving the active radius.

## Metrics

- CPU meshing time per chunk (ms)
- P50/P95/P99 meshing latency
- chunks meshed per second
- render-thread time spent consuming completed meshes
- GPU upload time per chunk
- resident GPU mesh count
- managed allocations per chunk
- peak managed memory
- stale jobs discarded
- failed jobs
- frame-time impact while streaming

## Method

Run Release configuration with a fixed seed and fixed camera path. Warm up before measurement. Capture at least 30 seconds per scenario and compare the same workload with asynchronous meshing enabled and disabled.

## Acceptance gates

- No render-thread stalls caused by CPU meshing.
- No stale mesh may overwrite a newer chunk version.
- No index truncation.
- No GPU API calls from worker threads.
- Record actual measurements before changing worker counts or queue policy.

## Current status

STATUS: PARTIAL — benchmark specification is committed; runtime benchmark results are not claimed until executed in the target build environment.
