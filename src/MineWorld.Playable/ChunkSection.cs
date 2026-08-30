namespace MineWorld.Playable;

internal sealed class ChunkSection
{
    public const int Size = 16;
    public const int Volume = Size * Size * Size;

    private readonly byte[] _blocks = new byte[Volume];

    public byte Get(int x, int y, int z)
    {
        Validate(x, y, z);
        return _blocks[IndexOf(x, y, z)];
    }

    public void Set(int x, int y, int z, byte block)
    {
        Validate(x, y, z);
        _blocks[IndexOf(x, y, z)] = block;
    }

    public bool IsEmpty => _blocks.All(static block => block == 0);

    private static int IndexOf(int x, int y, int z) => (y * Size + z) * Size + x;

    private static void Validate(int x, int y, int z)
    {
        if (x is < 0 or >= Size || y is < 0 or >= Size || z is < 0 or >= Size)
            throw new ArgumentOutOfRangeException($"Section coordinate ({x}, {y}, {z}) is outside the section.");
    }
}
