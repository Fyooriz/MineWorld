namespace MineWorld.Core.World;

public readonly record struct WorldGeneratorConfig(int Version, int SeaLevel = 32, int BaseHeight = 32, int HeightAmplitude = 16);

public interface IWorldGenerator
{
    BlockId GenerateBlock(int worldX, int worldY, int worldZ, long seed, WorldGeneratorConfig config);
}

/// <summary>Deterministic starter terrain generator for the P0 vertical slice.</summary>
public sealed class BasicWorldGenerator : IWorldGenerator
{
    public BlockId GenerateBlock(int worldX, int worldY, int worldZ, long seed, WorldGeneratorConfig config)
    {
        if (worldY < 0 || worldY >= 64) return BlockId.Air;

        var n = Hash(worldX, worldZ, seed ^ config.Version);
        var height = config.BaseHeight + (int)(n % (uint)(config.HeightAmplitude + 1));

        if (worldY > height) return BlockId.Air;
        if (worldY == height) return BlockId.Grass;
        if (worldY >= height - 3) return BlockId.Dirt;
        return BlockId.Stone;
    }

    private static uint Hash(int x, int z, long seed)
    {
        unchecked
        {
            ulong h = (ulong)seed + 0x9E3779B97F4A7C15UL;
            h ^= (ulong)(uint)x * 0xBF58476D1CE4E5B9UL;
            h ^= (ulong)(uint)z * 0x94D049BB133111EBUL;
            h ^= h >> 30;
            h *= 0xBF58476D1CE4E5B9UL;
            h ^= h >> 27;
            h *= 0x94D049BB133111EBUL;
            h ^= h >> 31;
            return (uint)h;
        }
    }
}
