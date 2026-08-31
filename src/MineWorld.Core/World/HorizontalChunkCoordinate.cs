namespace MineWorld.Core.World;

/// <summary>Canonical horizontal chunk coordinate using mathematical floor semantics for negative world positions.</summary>
public readonly record struct HorizontalChunkCoordinate(int X, int Z)
{
    public const int Size = 16;

    public static HorizontalChunkCoordinate FromWorld(int worldX, int worldZ)
        => new(FloorDiv(worldX, Size), FloorDiv(worldZ, Size));

    public (int X, int Z) ToLocal(int worldX, int worldZ)
    {
        if (FromWorld(worldX, worldZ) != this)
            throw new ArgumentException("World coordinate does not belong to this chunk.", nameof(worldX));

        return (FloorMod(worldX, Size), FloorMod(worldZ, Size));
    }

    public (int X, int Z) ToWorld(int localX, int localZ)
    {
        if (localX is < 0 or >= Size)
            throw new ArgumentOutOfRangeException(nameof(localX));
        if (localZ is < 0 or >= Size)
            throw new ArgumentOutOfRangeException(nameof(localZ));

        return (X * Size + localX, Z * Size + localZ);
    }

    private static int FloorDiv(int value, int divisor)
    {
        var quotient = value / divisor;
        var remainder = value % divisor;
        return remainder >= 0 ? quotient : quotient - 1;
    }

    private static int FloorMod(int value, int divisor)
    {
        var result = value % divisor;
        return result < 0 ? result + divisor : result;
    }
}
