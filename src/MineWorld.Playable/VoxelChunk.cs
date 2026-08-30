namespace MineWorld.Playable;

internal sealed class VoxelChunk
{
    public const int Size = 16;
    public const int SectionCount = 4;
    public const int MinY = 0;
    public const int MaxYExclusive = SectionCount * ChunkSection.Size;

    private readonly ChunkSection[] _sections = Enumerable.Range(0, SectionCount)
        .Select(static _ => new ChunkSection())
        .ToArray();

    public VoxelChunk(int chunkX, int chunkZ)
    {
        ChunkX = chunkX;
        ChunkZ = chunkZ;
    }

    public int ChunkX { get; }
    public int ChunkZ { get; }

    public byte GetBlock(int localX, int y, int localZ)
    {
        Validate(localX, y, localZ);
        return _sections[y / ChunkSection.Size].Get(
            localX,
            y % ChunkSection.Size,
            localZ);
    }

    public void SetBlock(int localX, int y, int localZ, byte block)
    {
        Validate(localX, y, localZ);
        _sections[y / ChunkSection.Size].Set(
            localX,
            y % ChunkSection.Size,
            localZ,
            block);
    }

    public bool IsEmpty => _sections.All(static section => section.IsEmpty);

    private static void Validate(int x, int y, int z)
    {
        if (x is < 0 or >= Size || y is < MinY or >= MaxYExclusive || z is < 0 or >= Size)
            throw new ArgumentOutOfRangeException($"Chunk local coordinate ({x}, {y}, {z}) is outside the chunk.");
    }
}
