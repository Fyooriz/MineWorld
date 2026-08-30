using Raylib_cs;

namespace MineWorld.Playable;

/// <summary>Renderer-thread GPU resource. CPU generation never creates this type.</summary>
internal sealed class GpuChunkMesh : IDisposable
{
    private Model _model;
    private bool _disposed;

    private GpuChunkMesh(Model model, int vertexCount)
    {
        _model = model;
        VertexCount = vertexCount;
    }

    public int VertexCount { get; }

    public static GpuChunkMesh Upload(ChunkMesh mesh)
    {
        if (mesh.VertexCount == 0) return new GpuChunkMesh(default, 0);

        var positions = new float[mesh.VertexCount * 3];
        var normals = new float[mesh.VertexCount * 3];
        for (var i = 0; i < mesh.VertexCount; i++)
        {
            var v = mesh.Vertices[i];
            positions[i * 3] = v.Position.X;
            positions[i * 3 + 1] = v.Position.Y;
            positions[i * 3 + 2] = v.Position.Z;
            normals[i * 3] = v.Normal.X;
            normals[i * 3 + 1] = v.Normal.Y;
            normals[i * 3 + 2] = v.Normal.Z;
        }

        var meshData = new Raylib_cs.Mesh
        {
            VertexCount = mesh.VertexCount,
            TriangleCount = mesh.VertexCount / 3,
            Vertices = positions,
            Normals = normals
        };
        Raylib.UploadMesh(ref meshData, false);
        return new GpuChunkMesh(Raylib.LoadModelFromMesh(meshData), mesh.VertexCount);
    }

    public void Draw()
    {
        if (_disposed || VertexCount == 0) return;
        Raylib.DrawModel(_model, System.Numerics.Vector3.Zero, 1f, Color.White);
    }

    public void Dispose()
    {
        if (_disposed || VertexCount == 0) return;
        Raylib.UnloadModel(_model);
        _disposed = true;
    }
}
