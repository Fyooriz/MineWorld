using System;
using MineWorld.Blocks.Runtime;

namespace MineWorld.World.Chunks;

public sealed class ChunkGenerationPipeline
{
    private readonly BlockRegistry _registry;
    private readonly int _seaLevel;

    public ChunkGenerationPipeline(BlockRegistry registry, int seaLevel = 32)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _registry.GetDefinition("mineworld:air");
        _registry.GetDefinition("mineworld:stone");
        _registry.GetDefinition("mineworld:dirt");
        _registry.GetDefinition("mineworld:grass");
        _seaLevel = seaLevel;
    }

    public ChunkBlockStorage Generate(int chunkX, int chunkZ, int width = 16, int height = 96, int depth = 16)
    {
        var air = _registry.CreateDefaultState("mineworld:air");
        var chunk = new ChunkBlockStorage(width, height, depth, air);

        for (var z = 0; z < depth; z++)
        for (var x = 0; x < width; x++)
        {
            var worldX = chunkX * width + x;
            var worldZ = chunkZ * depth + z;
            var surface = SurfaceHeight(worldX, worldZ, height);

            for (var y = 0; y < surface; y++)
            {
                var block = y == surface - 1
                    ? _registry.CreateDefaultState("mineworld:grass")
                    : y >= surface - 4
                        ? _registry.CreateDefaultState("mineworld:dirt")
                        : _registry.CreateDefaultState("mineworld:stone");
                chunk.Set(x, y, z, block);
            }
        }

        return chunk;
    }

    private int SurfaceHeight(int x, int z, int height)
    {
        var value = HashNoise(x, z);
        var rolling = HashNoise(x / 4, z / 4);
        var h = _seaLevel + (int)Math.Round(value * 6.0 + rolling * 10.0);
        return Math.Clamp(h, 4, height - 1);
    }

    private static double HashNoise(int x, int z)
    {
        unchecked
        {
            var n = x * 374761393 + z * 668265263;
            n = (n ^ (n >> 13)) * 1274126177;
            n ^= n >> 16;
            return (n & 0x7fffffff) / 1073741823.5 - 1.0;
        }
    }
}
