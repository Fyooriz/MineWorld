using System;
using MineWorld.Blocks.Runtime;
using MineWorld.World.Chunks;

namespace MineWorld.World.Terrain;

public sealed class ChunkGenerator
{
    private readonly BlockRegistry _blocks;
    private readonly int _seaLevel;
    private readonly int _baseHeight;

    public ChunkGenerator(BlockRegistry blocks, int seaLevel = 32, int baseHeight = 40)
    {
        _blocks = blocks;
        _seaLevel = seaLevel;
        _baseHeight = baseHeight;
    }

    public ChunkBlockStorage Generate(long seed, int chunkX, int chunkZ, int width = 16, int height = 96, int depth = 16)
    {
        var air = _blocks.CreateDefaultState("mineworld:air");
        var chunk = new ChunkBlockStorage(width, height, depth, air);
        var stone = _blocks.CreateDefaultState("mineworld:stone");
        var dirt = _blocks.CreateDefaultState("mineworld:dirt");
        var grass = _blocks.CreateDefaultState("mineworld:grass");
        var water = _blocks.CreateDefaultState("mineworld:water");

        for (var z = 0; z < depth; z++)
        for (var x = 0; x < width; x++)
        {
            var worldX = chunkX * width + x;
            var worldZ = chunkZ * depth + z;
            var terrain = GetTerrainHeight(seed, worldX, worldZ);

            for (var y = 0; y < height; y++)
            {
                if (y <= terrain)
                {
                    var state = y == terrain ? grass : y >= terrain - 3 ? dirt : stone;
                    chunk.Set(x, y, z, state);
                }
                else if (y <= _seaLevel)
                {
                    chunk.Set(x, y, z, water);
                }
            }
        }

        return chunk;
    }

    private int GetTerrainHeight(long seed, int x, int z)
    {
        var n = Hash(seed, x, z);
        var smooth = (Hash(seed ^ 0x51ED270BL, x / 4, z / 4) & 1023) / 1023.0;
        var variation = ((n & 1023) / 1023.0 - 0.5) * 10.0;
        return Math.Clamp(_baseHeight + (int)Math.Round((smooth - 0.5) * 24.0 + variation), 4, 90);
    }

    private static long Hash(long seed, int x, int z)
    {
        unchecked
        {
            long h = seed;
            h ^= x * 0x9E3779B97F4A7C15L;
            h = (h ^ (h >> 30)) * 0xBF58476D1CE4E5B9L;
            h ^= z * 0x94D049BB133111EBL;
            h = (h ^ (h >> 27)) * 0xBF58476D1CE4E5B9L;
            return h ^ (h >> 31);
        }
    }
}
