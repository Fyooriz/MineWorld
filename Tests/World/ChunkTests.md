# Chunk Tests

## Status
PARTIAL — specification pending executable test framework.

## Required tests

- identical seed + generator version + coordinates produce identical baseline chunk data
- different chunk coordinates produce independently addressable chunks
- lifecycle transitions reject invalid state changes
- duplicate streaming requests resolve to one active job
- stale low-priority requests can be cancelled safely
- worker completion order does not change authoritative results
- dirty chunks enter persistence flow before unload
- failed generation never becomes `GENERATED`
- failed save never clears dirty state
- version mismatch is reported explicitly

## Performance checks

Measure generation throughput, active chunk count, memory usage, queue depth, and main-thread blocking time once runtime exists.
