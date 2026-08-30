namespace MineWorld.Core.World;

/// <summary>
/// Minimal fixed-size voxel storage used by the P0 simulation core.
/// Rendering and streaming are intentionally outside this class.
/// </summary>
public sealed class Chunk
{
    public const int Size = 16;
    public const int Height = 64;

    private readonly BlockId[] _blocks = new BlockId[Size * Height * Size];

    public Chunk(ChunkCoordinate coordinate)
    {
        Coordinate = coordinate;
        Array.Fill(_blocks, BlockId.Air);
    }

    public ChunkCoordinate Coordinate { get; }

    public BlockId GetBlock(int x, int y, int z)
    {
        Validate(x, y, z);
        return _blocks[IndexOf(x, y, z)];
    }

    public void SetBlock(int x, int y, int z, BlockId block)
    {
        Validate(x, y, z);
        _blocks[IndexOf(x, y, z)] = block;
    }

    private static int IndexOf(int x, int y, int z) => (y * Size + z) * Size + x;

    private static void Validate(int x, int y, int z)
    {
        if (x is < 0 or >= Size || y is < 0 or >= Height || z is < 0 or >= Size)
            throw new ArgumentOutOfRangeException($"Voxel coordinate ({x}, {y}, {z}) is outside the chunk.");
    }
}
