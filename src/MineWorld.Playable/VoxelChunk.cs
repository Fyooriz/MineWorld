using MineWorld.Core.World;

namespace MineWorld.Playable;

/// <summary>Presentation adapter over the canonical Core chunk. No voxel storage is duplicated here.</summary>
internal sealed class VoxelChunk
{
    public const int Size = Chunk.Size;
    public const int SectionCount = Chunk.Height / ChunkSection.Size;
    public const int MinY = 0;
    public const int MaxYExclusive = Chunk.Height;

    private readonly MineWorld.Core.World.Chunk _coreChunk;

    public VoxelChunk(MineWorld.Core.World.Chunk coreChunk)
    {
        _coreChunk = coreChunk ?? throw new ArgumentNullException(nameof(coreChunk));
        ChunkX = coreChunk.Coordinate.X;
        ChunkZ = coreChunk.Coordinate.Z;
    }

    public int ChunkX { get; }
    public int ChunkZ { get; }
    public MineWorld.Core.World.Chunk CoreChunk => _coreChunk;

    public byte GetBlock(int localX, int y, int localZ)
    {
        Validate(localX, y, localZ);
        return checked((byte)_coreChunk.GetBlock(localX, y, localZ).Value);
    }

    public void SetBlock(int localX, int y, int localZ, byte block)
    {
        Validate(localX, y, localZ);
        _coreChunk.SetBlock(localX, y, localZ, new BlockId(block));
    }

    public bool IsEmpty
    {
        get
        {
            for (var y = 0; y < MaxYExclusive; y++)
            for (var z = 0; z < Size; z++)
            for (var x = 0; x < Size; x++)
                if (_coreChunk.GetBlock(x, y, z) != BlockId.Air)
                    return false;
            return true;
        }
    }

    private static void Validate(int x, int y, int z)
    {
        if (x is < 0 or >= Size || y is < MinY or >= MaxYExclusive || z is < 0 or >= Size)
            throw new ArgumentOutOfRangeException($"Chunk local coordinate ({x}, {y}, {z}) is outside the chunk.");
    }
}
