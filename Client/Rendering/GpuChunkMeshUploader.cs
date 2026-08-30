using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_cs;

namespace MineWorld.Playable;

/// <summary>Owns persistent GPU mesh lifetime on the render thread. CPU workers never touch Raylib.</summary>
internal sealed class GpuChunkMeshUploader : IDisposable
{
    private sealed class ResidentMesh
    {
        public Mesh Mesh;
        public Material Material;
        public bool Visible;
    }

    private readonly Dictionary<ChunkKey, ResidentMesh> _meshes = new();

    public int ResidentCount => _meshes.Count;
    public int VisibleCount => _meshes.Values.Count(m => m.Visible);

    public void Upload(ChunkKey key, ChunkMeshData data, Material material)
    {
        Remove(key);
        if (data.IsEmpty) return;

        var vertices = data.Vertices.ToArray();
        var indices = data.Indices.ToArray();
        if (vertices.Length > ushort.MaxValue)
            throw new InvalidOperationException("Chunk mesh exceeds the current 16-bit index limit.");

        var mesh = new Mesh
        {
            VertexCount = vertices.Length,
            TriangleCount = indices.Length / 3,
            Vertices = MemoryMarshal.Cast<Vector3, float>(vertices).ToArray(),
            Indices = indices.Select(i => checked((ushort)i)).ToArray()
        };

        Raylib.UploadMesh(ref mesh, false);
        _meshes[key] = new ResidentMesh { Mesh = mesh, Material = material, Visible = true };
    }

    public bool SetVisible(ChunkKey key, bool visible)
    {
        if (!_meshes.TryGetValue(key, out var resident)) return false;
        resident.Visible = visible;
        return true;
    }

    public void DrawVisible()
    {
        foreach (var resident in _meshes.Values)
        {
            if (!resident.Visible) continue;
            Raylib.DrawMesh(resident.Mesh, resident.Material, Matrix4x4.Identity);
        }
    }

    public void Remove(ChunkKey key)
    {
        if (!_meshes.Remove(key, out var resident)) return;
        Raylib.UnloadMesh(resident.Mesh);
    }

    public void Dispose()
    {
        foreach (var resident in _meshes.Values)
            Raylib.UnloadMesh(resident.Mesh);
        _meshes.Clear();
    }
}
