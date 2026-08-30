using System;
using MineWorld.Blocks.Runtime;
using MineWorld.World.Chunks;

namespace MineWorld.World.Terrain;

public sealed class ChunkGenerationPipeline
{
    private readonly BlockRegistry _registry;
    private readonly long _seed;
    private readonly int _seaLevel;
    private readonly int _baseHeight;

    public ChunkGenerationPipeline(BlockRegistry registry, long seed, int seaLevel = 32, int baseHeight = 40)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _seed = seed;
        _seaLevel = seaLevel;
        _baseHeight = baseHeight;
    }

    public ChunkBlockStorage Generate(int chunkX, int chunkZ, int width = 16, int height = 96, int depth = 16)
    {
        var air = _registry.CreateDefaultState("mineworld:air");
        var stone = _registry.CreateDefaultState("mineworld:stone");
        var dirt = _registry.CreateDefaultState("mineworld:dirt");
        var grass = _registry.CreateDefaultState("mineworld:grass");
        var water = _registry.CreateDefaultState("mineworld:water");
        var chunk = new ChunkBlockStorage(width, height, depth, air);

        for (var z = 0; z < depth; z++)
        for (var x = 0; x < width; x++)
        {
            var terrain = TerrainHeight(chunkX * width + x, chunkZ * depth + z);
            for (var y = 0; y < height; y++)
            {
                if (y <= terrain)
                    chunk.Set(x, y, z, y == terrain ? grass : y >= terrain - 3 ? dirt : stone);
                else if (y <= _seaLevel)
                    chunk.Set(x, y, z, water);
            }
        }
        return chunk;
    }

    private int TerrainHeight(int x, int z)
    {
        var coarse = Noise(x >> 2, z >> 2);
        var detail = Noise(x, z);
        return Math.Clamp(_baseHeight + (int)Math.Round((coarse - 0.5) * 24 + (detail - 0.5) * 8), 4, 90);
    }

    private double Noise(int x, int z)
    {
        unchecked
        {
            long h = _seed;
            h ^= x * 0x9E3779B97F4A7C15L;
            h = (h ^ (h >> 30)) * 0xBF58476D1CE4E5B9L;
            h ^= z * 0x94D049BB133111EBL;
            h = (h ^ (h >> 27)) * 0xBF58476D1CE4E5B9L;
            h ^= h >> 31;
            return (h & 0x7FFFFFFFFFFFFFFF) / (double)long.MaxValue;
        }
    }
}
