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
    private readonly int _chunkSize;

    public GpuChunkMeshUploader(int chunkSize = 16)
    {
        _chunkSize = Math.Max(1, chunkSize);
    }

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

        if (indices.Any(i => i < 0 || i >= vertices.Length))
            throw new InvalidOperationException("Chunk mesh contains an out-of-range vertex index.");

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
        foreach (var pair in _meshes)
        {
            var resident = pair.Value;
            if (!resident.Visible) continue;

            var transform = Matrix4x4.CreateTranslation(
                pair.Key.X * _chunkSize,
                0f,
                pair.Key.Z * _chunkSize);
            Raylib.DrawMesh(resident.Mesh, resident.Material, transform);
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
