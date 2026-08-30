using System.Numerics;
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

        var vertices = new Vector3[data.Vertices.Length];
        Array.Copy(data.Vertices, vertices, vertices.Length);
        var indices = data.Indices.ToArray();

        var mesh = new Mesh
        {
            VertexCount = vertices.Length,
            TriangleCount = indices.Length / 3,
            Vertices = MemoryMarshal.Cast<Vector3, float>(vertices).ToArray(),
            Indices = indices.Select(i => (ushort)i).ToArray()
        };

        Raylib.UploadMesh(ref mesh, false);
        _meshes[key] = mesh;
    }

    public void Draw(ChunkKey key)
    {
        if (_meshes.TryGetValue(key, out var mesh))
            Raylib.DrawMesh(mesh, new Material(), Matrix4x4.Identity);
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
