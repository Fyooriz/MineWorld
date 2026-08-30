namespace MineWorld.Core.World;

/// <summary>
/// Fixed 16x16x16 voxel section. This is the canonical storage unit for vertical chunk data.
/// </summary>
public sealed class ChunkSection
{
    public const int Size = 16;
    private readonly BlockId[] _blocks = new BlockId[Size * Size * Size];

    public ChunkSection(int sectionY)
    {
        SectionY = sectionY;
        Array.Fill(_blocks, BlockId.Air);
    }

    public int SectionY { get; }

    public BlockId GetBlock(int x, int y, int z)
    {
        Validate(x, y, z);
        return _blocks[(y * Size + z) * Size + x];
    }

    public void SetBlock(int x, int y, int z, BlockId block)
    {
        Validate(x, y, z);
        _blocks[(y * Size + z) * Size + x] = block;
    }

    private static void Validate(int x, int y, int z)
    {
        if (x is < 0 or >= Size || y is < 0 or >= Size || z is < 0 or >= Size)
            throw new ArgumentOutOfRangeException($"Section coordinate ({x}, {y}, {z}) is outside the 16x16x16 section.");
    }
}
