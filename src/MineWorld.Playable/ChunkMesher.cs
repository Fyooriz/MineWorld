using System.Numerics;

namespace MineWorld.Playable;

/// <summary>Builds one indexed mesh per chunk from exposed voxel faces.</summary>
internal sealed class ChunkMesher
{
    private static readonly (int X, int Y, int Z)[] Directions =
    [
        (1, 0, 0), (-1, 0, 0), (0, 1, 0), (0, -1, 0), (0, 0, 1), (0, 0, -1)
    ];

    public ChunkMeshData Build(VoxelWorld world, VoxelChunk chunk)
    {
        var vertices = new List<Vector3>();
        var indices = new List<int>();
        var colors = new List<ColorRgba>();

        for (var y = 0; y < VoxelChunk.MaxYExclusive; y++)
        for (var z = 0; z < VoxelChunk.Size; z++)
        for (var x = 0; x < VoxelChunk.Size; x++)
        {
            var block = chunk.GetBlock(x, y, z);
            if (block == VoxelWorld.Air) continue;

            var wx = chunk.ChunkX * VoxelChunk.Size + x;
            var wz = chunk.ChunkZ * VoxelChunk.Size + z;
            for (var face = 0; face < Directions.Length; face++)
            {
                var d = Directions[face];
                if (world.GetBlock(wx + d.X, y + d.Y, wz + d.Z) != VoxelWorld.Air) continue;
                AddFace(vertices, indices, colors, wx, y, wz, face, block);
            }
        }

        return new ChunkMeshData(vertices.ToArray(), indices.ToArray(), colors.ToArray());
    }

    private static void AddFace(List<Vector3> v, List<int> i, List<ColorRgba> c, int x, int y, int z, int face, byte block)
    {
        var p = new Vector3(x, y, z);
        var corners = face switch
        {
            0 => new[] { p + new Vector3(1,0,0), p + new Vector3(1,1,0), p + new Vector3(1,1,1), p + new Vector3(1,0,1) },
            1 => new[] { p + new Vector3(0,0,1), p + new Vector3(0,1,1), p + new Vector3(0,1,0), p + new Vector3(0,0,0) },
            2 => new[] { p + new Vector3(0,1,1), p + new Vector3(1,1,1), p + new Vector3(1,1,0), p + new Vector3(0,1,0) },
            3 => new[] { p + new Vector3(0,0,0), p + new Vector3(1,0,0), p + new Vector3(1,0,1), p + new Vector3(0,0,1) },
            4 => new[] { p + new Vector3(1,0,1), p + new Vector3(1,1,1), p + new Vector3(0,1,1), p + new Vector3(0,0,1) },
            _ => new[] { p + new Vector3(0,0,0), p + new Vector3(0,1,0), p + new Vector3(1,1,0), p + new Vector3(1,0,0) }
        };

        var start = v.Count;
        v.AddRange(corners);
        i.Add(start); i.Add(start + 1); i.Add(start + 2);
        i.Add(start); i.Add(start + 2); i.Add(start + 3);
        var color = block switch
        {
            VoxelWorld.Grass => new ColorRgba(91, 160, 74, 255),
            VoxelWorld.Dirt => new ColorRgba(130, 88, 52, 255),
            VoxelWorld.Stone => new ColorRgba(112, 116, 122, 255),
            _ => new ColorRgba(255, 255, 255, 255)
        };
        c.Add(color); c.Add(color); c.Add(color); c.Add(color);
    }
}

internal readonly record struct ColorRgba(byte R, byte G, byte B, byte A);
internal sealed record ChunkMeshData(Vector3[] Vertices, int[] Indices, ColorRgba[] Colors)
{
    public bool IsEmpty => Vertices.Length == 0;
}
