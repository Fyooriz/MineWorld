# Chunk Generation Scheduler Tests

## Purpose

Regression specification for the P0 background chunk pipeline.

## Required checks

1. A chunk inside the configured view distance is eventually queued.
2. The same chunk is not queued twice while generation is in flight.
3. Worker execution produces a CPU-side `ChunkMesh` without accessing GPU APIs.
4. Completed results can be drained independently from the render thread.
5. Cancellation stops workers during scheduler disposal.
6. A generation exception must not permanently leave the chunk marked in-flight.
7. The scheduler remains deterministic for the same player position and loaded-chunk set.

## Verification status

STATUS: PARTIAL

The repository currently contains the scheduler implementation. These checks must be executed by the project test runner before the pipeline is marked VERIFIED.
