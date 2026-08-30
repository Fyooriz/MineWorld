using System;
using System.Collections.Generic;

namespace MineWorld.World.Meshing;

/// <summary>Owns GPU-facing resources separately from CPU voxel meshing.</summary>
public sealed class ChunkGpuMeshCache : IDisposable
{
    private readonly Dictionary<(int X, int Z), GpuMeshResource> _resources = new();

    public IReadOnlyDictionary<(int X, int Z), GpuMeshResource> Resources => _resources;

    public GpuMeshResource Upload(int chunkX, int chunkZ, VoxelMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        var key = (chunkX, chunkZ);
        if (!_resources.TryGetValue(key, out var resource))
        {
            resource = new GpuMeshResource();
            _resources.Add(key, resource);
        }

        resource.Upload(mesh);
        return resource;
    }

    public bool Remove(int chunkX, int chunkZ)
    {
        var key = (chunkX, chunkZ);
        if (!_resources.Remove(key, out var resource)) return false;
        resource.Dispose();
        return true;
    }

    public void Clear()
    {
        foreach (var resource in _resources.Values) resource.Dispose();
        _resources.Clear();
    }

    public void Dispose() => Clear();
}
