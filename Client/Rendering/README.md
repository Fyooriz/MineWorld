# MineWorld Rendering — P0

The P0 renderer uses a strict CPU/GPU boundary:

```text
World / Chunk
    -> VoxelMesher
    -> VoxelMesh (CPU)
    -> IRenderBackend
    -> RaylibVoxelRenderBackend
    -> Raylib GPU mesh
    -> draw
```

## Rules

- World generation and meshing never call Raylib directly.
- GPU uploads and GPU resource destruction occur on the render thread.
- `IRenderBackend` keeps the engine independent from the concrete graphics API.
- The current concrete backend is Raylib because it gives P0 a small, testable path to a real GPU context.
- The renderer currently uses 16-bit indices. Chunk meshes must therefore be split before exceeding `ushort.MaxValue` vertices.
- Texture/material binding is intentionally minimal in P0; block appearance remains a later data-driven renderer layer.

## Next rendering milestones

1. Persistent per-chunk GPU buffers instead of one transient mesh.
2. Frustum visibility and chunk draw list.
3. Texture atlas/material binding from block definitions.
4. Greedy meshing and mesh rebuild scheduling.
5. Lighting/AO data in vertices.
6. Render diagnostics and GPU timing.
