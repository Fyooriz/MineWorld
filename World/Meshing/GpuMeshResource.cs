using System;

namespace MineWorld.World.Meshing;

/// <summary>Backend-neutral GPU resource handle for a chunk mesh.</summary>
public sealed class GpuMeshResource : IDisposable
{
    public int VertexCount { get; private set; }
    public int IndexCount { get; private set; }
    public bool IsUploaded { get; private set; }
    public int Generation { get; private set; }

    public void Upload(VoxelMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        VertexCount = mesh.Vertices.Count;
        IndexCount = mesh.Indices.Count;
        Generation++;
        IsUploaded = true;
    }

    public void Dispose()
    {
        VertexCount = 0;
        IndexCount = 0;
        IsUploaded = false;
    }
}
