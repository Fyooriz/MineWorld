using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_cs;

namespace MineWorld.Playable;

/// <summary>Owns GPU mesh lifetime on the render thread. CPU workers never touch Raylib.</summary>
internal sealed class GpuChunkMeshUploader : IDisposable
{
    private readonly Dictionary<ChunkKey, Mesh> _meshes = new();

    public int ResidentCount => _meshes.Count;

    public void Upload(ChunkKey key, ChunkMeshData data)
    {
        if (data.IsEmpty)
        {
            Remove(key);
            return;
        }

        Remove(key);
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
        _meshes[key] = mesh;
    }

    public void Draw(ChunkKey key, Material material)
    {
        if (_meshes.TryGetValue(key, out var mesh))
            Raylib.DrawMesh(mesh, material, Matrix4x4.Identity);
    }

    public void Remove(ChunkKey key)
    {
        if (!_meshes.Remove(key, out var mesh)) return;
        Raylib.UnloadMesh(mesh);
    }

    public void Dispose()
    {
        foreach (var mesh in _meshes.Values)
            Raylib.UnloadMesh(mesh);
        _meshes.Clear();
    }
}
