namespace MineWorld.Core.World;

public readonly record struct VoxelFace(int X, int Y, int Z, FaceDirection Direction, BlockId Block);

public enum FaceDirection
{
    North,
    South,
    West,
    East,
    Down,
    Up
}

/// <summary>
/// Builds the visible face list for a chunk section. Rendering remains a separate concern.
/// </summary>
public static class VoxelMesher
{
    private static readonly (int X, int Y, int Z, FaceDirection Direction)[] Neighbours =
    [
        (0, 0, -1, FaceDirection.North),
        (0, 0, 1, FaceDirection.South),
        (-1, 0, 0, FaceDirection.West),
        (1, 0, 0, FaceDirection.East),
        (0, -1, 0, FaceDirection.Down),
        (0, 1, 0, FaceDirection.Up)
    ];

    public static List<VoxelFace> Build(ChunkSection section)
    {
        ArgumentNullException.ThrowIfNull(section);
        var faces = new List<VoxelFace>();

        for (var y = 0; y < ChunkSection.Size; y++)
        for (var z = 0; z < ChunkSection.Size; z++)
        for (var x = 0; x < ChunkSection.Size; x++)
        {
            var block = section.GetBlock(x, y, z);
            if (block == BlockId.Air) continue;

            foreach (var neighbour in Neighbours)
            {
                var nx = x + neighbour.X;
                var ny = y + neighbour.Y;
                var nz = z + neighbour.Z;

                // Cross-section neighbors are intentionally emitted for now. The chunk-level
                // mesher will resolve them against adjacent sections/chunks in the next step.
                var exposed = nx is < 0 or >= ChunkSection.Size ||
                              ny is < 0 or >= ChunkSection.Size ||
                              nz is < 0 or >= ChunkSection.Size ||
                              section.GetBlock(nx, ny, nz) == BlockId.Air;

                if (exposed)
                    faces.Add(new VoxelFace(x, y, z, neighbour.Direction, block));
            }
        }

        return faces;
    }
}
