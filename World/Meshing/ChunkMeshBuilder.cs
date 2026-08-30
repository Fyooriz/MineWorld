using System;
using MineWorld.Blocks.Runtime;
using MineWorld.World.Chunks;

namespace MineWorld.World.Meshing;

/// <summary>Builds a renderable voxel mesh from a chunk and a world-aware block sampler.</summary>
public sealed class ChunkMeshBuilder
{
    private readonly VoxelMesher _mesher;

    public ChunkMeshBuilder(Func<string, bool>? isOpaque = null)
    {
        _mesher = new VoxelMesher(isOpaque);
    }

    public VoxelMesh Build(ChunkBlockStorage chunk, Func<int, int, int, BlockState> sample)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(sample);
        return _mesher.Build(chunk, sample);
    }
}
