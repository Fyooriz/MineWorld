using System.Numerics;

namespace MineWorld.Playable;

internal readonly record struct ChunkVertex(Vector3 Position, Vector3 Normal);

/// <summary>CPU-side render mesh. GPU upload remains renderer-owned.</summary>
internal sealed class ChunkMesh
{
    public ChunkMesh(ChunkKey key, IReadOnlyList<ChunkVertex> vertices)
    {
        Key = key;
        Vertices = vertices;
    }

    public ChunkKey Key { get; }
    public IReadOnlyList<ChunkVertex> Vertices { get; }
    public int VertexCount => Vertices.Count;
}

internal static class ChunkMesher
{
    private static readonly (int X, int Y, int Z, Vector3 Normal)[] Faces =
    {
        ( 1, 0, 0, Vector3.UnitX), (-1, 0, 0, -Vector3.UnitX),
        ( 0, 1, 0, Vector3.UnitY), ( 0,-1, 0, -Vector3.UnitY),
        ( 0, 0, 1, Vector3.UnitZ), ( 0, 0,-1, -Vector3.UnitZ)
    };

    private static readonly Vector3[][] FaceCorners =
    {
        new[] { new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1) },
        new[] { new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(0,0,0) },
        new[] { new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0), new Vector3(0,1,0) },
        new[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1) },
        new[] { new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1), new Vector3(0,0,1) },
        new[] { new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0) }
    };

    public static ChunkMesh Build(ChunkKey key, Func<int, int, int, bool> isSolid, int size = 16, int height = 32)
    {
        var vertices = new List<ChunkVertex>();
        for (var y = 0; y < height; y++)
        for (var z = 0; z < size; z++)
        for (var x = 0; x < size; x++)
        {
            if (!isSolid(x, y, z)) continue;
            var worldX = key.X * size + x;
            var worldZ = key.Z * size + z;
            for (var face = 0; face < Faces.Length; face++)
            {
                var f = Faces[face];
                if (isSolid(x + f.X, y + f.Y, z + f.Z)) continue;
                foreach (var corner in FaceCorners[face])
                    vertices.Add(new ChunkVertex(new Vector3(worldX, y, worldZ) + corner, f.Normal));
            }
        }
        return new ChunkMesh(key, vertices);
    }
}
