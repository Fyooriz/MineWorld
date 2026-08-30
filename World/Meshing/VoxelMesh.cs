using System;
using System.Collections.Generic;
using MineWorld.Blocks.Runtime;
using MineWorld.World.Chunks;

namespace MineWorld.World.Meshing;

public readonly record struct MeshVertex(float X, float Y, float Z, float Nx, float Ny, float Nz);

public sealed class VoxelMesh
{
    public List<MeshVertex> Vertices { get; } = new();
    public List<int> Indices { get; } = new();

    public void Clear()
    {
        Vertices.Clear();
        Indices.Clear();
    }
}

public sealed class VoxelMesher
{
    private readonly Func<string, bool> _isOpaque;

    public VoxelMesher(Func<string, bool>? isOpaque = null)
    {
        _isOpaque = isOpaque ?? (id => !id.Equals("mineworld:air", StringComparison.Ordinal));
    }

    public VoxelMesh Build(ChunkBlockStorage chunk, Func<int, int, int, BlockState> sample)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(sample);

        var mesh = new VoxelMesh();
        for (var y = 0; y < chunk.Height; y++)
        for (var z = 0; z < chunk.Depth; z++)
        for (var x = 0; x < chunk.Width; x++)
        {
            var state = chunk.Get(x, y, z);
            if (!_isOpaque(state.BlockId)) continue;

            AddVisibleFace(mesh, x, y, z, 0, -1, 0, sample(x, y - 1, z));
            AddVisibleFace(mesh, x, y, z, 0, 1, 0, sample(x, y + 1, z));
            AddVisibleFace(mesh, x, y, z, -1, 0, 0, sample(x - 1, y, z));
            AddVisibleFace(mesh, x, y, z, 1, 0, 0, sample(x + 1, y, z));
            AddVisibleFace(mesh, x, y, z, 0, 0, -1, sample(x, y, z - 1));
            AddVisibleFace(mesh, x, y, z, 0, 0, 1, sample(x, y, z + 1));
        }
        return mesh;
    }

    private void AddVisibleFace(VoxelMesh mesh, int x, int y, int z, int nx, int ny, int nz, BlockState neighbor)
    {
        if (_isOpaque(neighbor.BlockId)) return;

        var baseIndex = mesh.Vertices.Count;
        foreach (var vertex in FaceVertices(x, y, z, nx, ny, nz))
            mesh.Vertices.Add(vertex);
        mesh.Indices.Add(baseIndex);
        mesh.Indices.Add(baseIndex + 1);
        mesh.Indices.Add(baseIndex + 2);
        mesh.Indices.Add(baseIndex);
        mesh.Indices.Add(baseIndex + 2);
        mesh.Indices.Add(baseIndex + 3);
    }

    private static MeshVertex[] FaceVertices(int x, int y, int z, int nx, int ny, int nz)
    {
        var x0 = x; var x1 = x + 1;
        var y0 = y; var y1 = y + 1;
        var z0 = z; var z1 = z + 1;

        if (nx < 0) return Quad(x0, y0, z1, x0, y1, z1, x0, y1, z0, x0, y0, z0, nx, ny, nz);
        if (nx > 0) return Quad(x1, y0, z0, x1, y1, z0, x1, y1, z1, x1, y0, z1, nx, ny, nz);
        if (ny < 0) return Quad(x0, y0, z0, x1, y0, z0, x1, y0, z1, x0, y0, z1, nx, ny, nz);
        if (ny > 0) return Quad(x0, y1, z1, x1, y1, z1, x1, y1, z0, x0, y1, z0, nx, ny, nz);
        if (nz < 0) return Quad(x1, y0, z0, x0, y0, z0, x0, y1, z0, x1, y1, z0, nx, ny, nz);
        return Quad(x0, y0, z1, x1, y0, z1, x1, y1, z1, x0, y1, z1, nx, ny, nz);
    }

    private static MeshVertex[] Quad(float ax, float ay, float az, float bx, float by, float bz, float cx, float cy, float cz, float dx, float dy, float dz, int nx, int ny, int nz) =>
        new[]
        {
            new MeshVertex(ax, ay, az, nx, ny, nz),
            new MeshVertex(bx, by, bz, nx, ny, nz),
            new MeshVertex(cx, cy, cz, nx, ny, nz),
            new MeshVertex(dx, dy, dz, nx, ny, nz)
        };
}
