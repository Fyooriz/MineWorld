namespace MineWorld.Core.World;

public readonly record struct ChunkFace(int X, int Y, int Z, FaceDirection Direction, BlockId Block);

/// <summary>
/// Chunk-level meshing with neighbor-aware boundary culling.
/// A missing neighbor is treated as air, while a supplied neighbor is queried before emitting a boundary face.
/// </summary>
public static class ChunkMesher
{
    public static List<ChunkFace> Build(ChunkSection section, Func<int, int, int, BlockId>? getNeighborBlock = null)
    {
        ArgumentNullException.ThrowIfNull(section);
        var faces = new List<ChunkFace>();

        for (var y = 0; y < ChunkSection.Size; y++)
        for (var z = 0; z < ChunkSection.Size; z++)
        for (var x = 0; x < ChunkSection.Size; x++)
        {
            var block = section.GetBlock(x, y, z);
            if (block == BlockId.Air) continue;

            EmitIfVisible(x, y, z, FaceDirection.North, x, y, z - 1);
            EmitIfVisible(x, y, z, FaceDirection.South, x, y, z + 1);
            EmitIfVisible(x, y, z, FaceDirection.West, x - 1, y, z);
            EmitIfVisible(x, y, z, FaceDirection.East, x + 1, y, z);
            EmitIfVisible(x, y, z, FaceDirection.Down, x, y - 1, z);
            EmitIfVisible(x, y, z, FaceDirection.Up, x, y + 1, z);

            void EmitIfVisible(int bx, int by, int bz, FaceDirection direction, int nx, int ny, int nz)
            {
                BlockId neighbor;
                if (nx is >= 0 and < ChunkSection.Size && ny is >= 0 and < ChunkSection.Size && nz is >= 0 and < ChunkSection.Size)
                    neighbor = section.GetBlock(nx, ny, nz);
                else
                    neighbor = getNeighborBlock?.Invoke(nx, ny, nz) ?? BlockId.Air;

                if (neighbor == BlockId.Air)
                    faces.Add(new ChunkFace(bx, by, bz, direction, block));
            }
        }

        return faces;
    }
}
