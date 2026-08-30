using System;
using System.Collections.Generic;
using MineWorld.Blocks.Runtime;
using MineWorld.World.Chunks;

namespace MineWorld.World.Meshing;

/// <summary>CPU-side cache of chunk meshes with boundary-aware invalidation.</summary>
public sealed class ChunkMeshCache
{
    private readonly ChunkMeshBuilder _builder;
    private readonly Dictionary<(int X, int Z), VoxelMesh> _meshes = new();
    private readonly HashSet<(int X, int Z)> _dirty = new();

    public ChunkMeshCache(Func<string, bool>? isOpaque = null)
    {
        _builder = new ChunkMeshBuilder(isOpaque);
    }

    public IReadOnlyDictionary<(int X, int Z), VoxelMesh> Meshes => _meshes;

    public void MarkDirty(int chunkX, int chunkZ) => _dirty.Add((chunkX, chunkZ));

    /// <summary>Marks the edited chunk and adjacent chunks when an edit touches a chunk boundary.</summary>
    public void MarkBlockChanged(int chunkX, int chunkZ, int localX, int localZ, int chunkWidth, int chunkDepth)
    {
        if (chunkWidth <= 0) throw new ArgumentOutOfRangeException(nameof(chunkWidth));
        if (chunkDepth <= 0) throw new ArgumentOutOfRangeException(nameof(chunkDepth));
        if ((uint)localX >= (uint)chunkWidth) throw new ArgumentOutOfRangeException(nameof(localX));
        if ((uint)localZ >= (uint)chunkDepth) throw new ArgumentOutOfRangeException(nameof(localZ));

        MarkDirty(chunkX, chunkZ);
        if (localX == 0) MarkDirty(chunkX - 1, chunkZ);
        if (localX == chunkWidth - 1) MarkDirty(chunkX + 1, chunkZ);
        if (localZ == 0) MarkDirty(chunkX, chunkZ - 1);
        if (localZ == chunkDepth - 1) MarkDirty(chunkX, chunkZ + 1);
    }

    public void MarkAllDirty(IEnumerable<(int X, int Z)> chunks)
    {
        ArgumentNullException.ThrowIfNull(chunks);
        foreach (var chunk in chunks) _dirty.Add(chunk);
    }

    public bool Rebuild(int chunkX, int chunkZ, ChunkBlockStorage chunk, Func<int, int, int, BlockState> sample)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(sample);

        var mesh = _builder.Build(chunk, sample);
        _meshes[(chunkX, chunkZ)] = mesh;
        _dirty.Remove((chunkX, chunkZ));
        return mesh.Indices.Count > 0;
    }

    public bool IsDirty(int chunkX, int chunkZ) => _dirty.Contains((chunkX, chunkZ));

    public void Remove(int chunkX, int chunkZ)
    {
        _meshes.Remove((chunkX, chunkZ));
        _dirty.Remove((chunkX, chunkZ));
    }

    public void Clear()
    {
        _meshes.Clear();
        _dirty.Clear();
    }
}
