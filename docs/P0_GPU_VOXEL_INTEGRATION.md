# P0 GPU Voxel Integration

## Goal

Bridge generated chunk data to a persistent CPU mesh cache, preparing the renderer to upload only changed chunk geometry to GPU resources.

## Current pipeline

World generation/storage
→ VoxelMesher
→ ChunkMeshCache
→ renderer GPU upload (next integration step)
→ draw

## Design decisions

- Chunk geometry is rebuilt independently from the render loop.
- Meshes are keyed by chunk coordinate.
- Dirty state is explicit so block edits can invalidate a chunk and its relevant neighbors.
- The cache owns CPU mesh lifetime; the renderer owns GPU resource lifetime.
- World simulation must not depend on graphics API objects.
- Empty meshes remain valid cache entries, allowing a chunk to be represented as intentionally non-renderable.

## Next integration

1. Connect `ChunkWorldService.LoadOrGenerate` output to `ChunkMeshCache`.
2. Add world-aware neighbor sampling across chunk boundaries.
3. Invalidate the six-neighbor relationship when a boundary block changes.
4. Add renderer-side GPU buffer ownership and upload/update/dispose lifecycle.
5. Draw cached chunk meshes with frustum-distance filtering.

## Verification

STATUS: PARTIAL

The cache implementation and unit-level state transitions are committed. Full runtime GPU verification requires an executable build environment with the Raylib graphics context.
