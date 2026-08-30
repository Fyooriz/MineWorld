namespace MineWorld.Core.World;

/// <summary>Deterministic seed utilities for reproducible world generation.</summary>
public static class WorldSeed
{
    public static ulong Mix(ulong seed, ChunkCoordinate coordinate)
    {
        var value = seed;
        value ^= (ulong)(uint)coordinate.X * 0x9E3779B97F4A7C15UL;
        value ^= (ulong)(uint)coordinate.Y * 0xC2B2AE3D27D4EB4FUL;
        value ^= (ulong)(uint)coordinate.Z * 0x165667B19E3779F9UL;
        value += 0x9E3779B97F4A7C15UL;
        value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
        value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
        return value ^ (value >> 31);
    }
}
