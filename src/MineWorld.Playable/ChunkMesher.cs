using System.Numerics;

namespace MineWorld.Playable;

/// <summary>Builds compact chunk meshes by merging adjacent exposed faces.</summary>
internal sealed class ChunkMesher
{
    private readonly record struct FaceCell(bool Visible, byte Block);

    public ChunkMeshData Build(VoxelWorld world, VoxelChunk chunk)
    {
        var vertices = new List<Vector3>();
        var indices = new List<int>();
        var colors = new List<ColorRgba>();

        for (var face = 0; face < 6; face++)
            BuildDirection(world, chunk, face, vertices, indices, colors);

        return new ChunkMeshData(vertices.ToArray(), indices.ToArray(), colors.ToArray());
    }

    private static void BuildDirection(
        VoxelWorld world, VoxelChunk chunk, int face,
        List<Vector3> vertices, List<int> indices, List<ColorRgba> colors)
    {
        var sliceCount = face is 2 or 3 ? VoxelChunk.MaxYExclusive : VoxelChunk.Size;
        var width = VoxelChunk.Size;
        var height = face is 0 or 1 or 4 or 5 ? VoxelChunk.MaxYExclusive : VoxelChunk.Size;

        for (var slice = 0; slice < sliceCount; slice++)
        {
            var mask = new FaceCell[width, height];

            for (var v = 0; v < height; v++)
            for (var u = 0; u < width; u++)
            {
                var (x, y, z) = face switch
                {
                    0 or 1 => (slice, v, u),
                    2 or 3 => (u, slice, v),
                    _ => (u, v, slice)
                };

                var block = chunk.GetBlock(x, y, z);
                if (block == VoxelWorld.Air)
                    continue;

                var wx = chunk.ChunkX * VoxelChunk.Size + x;
                var wz = chunk.ChunkZ * VoxelChunk.Size + z;
                var (dx, dy, dz) = face switch
                {
                    0 => (1, 0, 0),
                    1 => (-1, 0, 0),
                    2 => (0, 1, 0),
                    3 => (0, -1, 0),
                    4 => (0, 0, 1),
                    _ => (0, 0, -1)
                };

                if (world.GetBlock(wx + dx, y + dy, wz + dz) == VoxelWorld.Air)
                    mask[u, v] = new FaceCell(true, block);
            }

            GreedyMerge(mask, width, height, face, slice, chunk, vertices, indices, colors);
        }
    }

    private static void GreedyMerge(
        FaceCell[,] mask, int width, int height, int face, int slice, VoxelChunk chunk,
        List<Vector3> vertices, List<int> indices, List<ColorRgba> colors)
    {
        for (var v = 0; v < height; v++)
        for (var u = 0; u < width; u++)
        {
            var cell = mask[u, v];
            if (!cell.Visible)
                continue;

            var runWidth = 1;
            while (u + runWidth < width && Same(mask[u + runWidth, v], cell))
                runWidth++;

            var runHeight = 1;
            while (v + runHeight < height)
            {
                var valid = true;
                for (var x = 0; x < runWidth; x++)
                {
                    if (!Same(mask[u + x, v + runHeight], cell))
                    {
                        valid = false;
                        break;
                    }
                }
                if (!valid)
                    break;
                runHeight++;
            }

            for (var y = v; y < v + runHeight; y++)
            for (var x = u; x < u + runWidth; x++)
                mask[x, y] = default;

            AddQuad(face, slice, u, v, runWidth, runHeight, cell.Block, chunk, vertices, indices, colors);
        }
    }

    private static bool Same(FaceCell a, FaceCell b) =>
        a.Visible && b.Visible && a.Block == b.Block;

    private static void AddQuad(
        int face, int slice, int u, int v, int width, int height, byte block, VoxelChunk chunk,
        List<Vector3> vertices, List<int> indices, List<ColorRgba> colors)
    {
        var ox = chunk.ChunkX * VoxelChunk.Size;
        var oz = chunk.ChunkZ * VoxelChunk.Size;
        var s = slice;

        Vector3[] corners = face switch
        {
            0 =>
            [new(ox + s + 1, v, oz + u), new(ox + s + 1, v + height, oz + u),
             new(ox + s + 1, v + height, oz + u + width), new(ox + s + 1, v, oz + u + width)],
            1 =>
            [new(ox + s, v, oz + u + width), new(ox + s, v + height, oz + u + width),
             new(ox + s, v + height, oz + u), new(ox + s, v, oz + u)],
            2 =>
            [new(ox + u, s + 1, oz + v + height), new(ox + u + width, s + 1, oz + v + height),
             new(ox + u + width, s + 1, oz + v), new(ox + u, s + 1, oz + v)],
            3 =>
            [new(ox + u, s, oz + v), new(ox + u + width, s, oz + v),
             new(ox + u + width, s, oz + v + height), new(ox + u, s, oz + v + height)],
            4 =>
            [new(ox + u + width, v, oz + s + 1), new(ox + u + width, v + height, oz + s + 1),
             new(ox + u, v + height, oz + s + 1), new(ox + u, v, oz + s + 1)],
            _ =>
            [new(ox + u, v, oz + s), new(ox + u, v + height, oz + s),
             new(ox + u + width, v + height, oz + s), new(ox + u + width, v, oz + s)]
        };

        var start = vertices.Count;
        vertices.AddRange(corners);
        indices.Add(start); indices.Add(start + 1); indices.Add(start + 2);
        indices.Add(start); indices.Add(start + 2); indices.Add(start + 3);

        var color = block switch
        {
            VoxelWorld.Grass => new ColorRgba(91, 160, 74, 255),
            VoxelWorld.Dirt => new ColorRgba(130, 88, 52, 255),
            VoxelWorld.Stone => new ColorRgba(112, 116, 122, 255),
            _ => new ColorRgba(255, 255, 255, 255)
        };
        colors.Add(color); colors.Add(color); colors.Add(color); colors.Add(color);
    }
}

internal readonly record struct ColorRgba(byte R, byte G, byte B, byte A);
internal sealed record ChunkMeshData(Vector3[] Vertices, int[] Indices, ColorRgba[] Colors)
{
    public bool IsEmpty => Vertices.Length == 0;
}
