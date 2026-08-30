using System;
using System.Collections.Generic;
using MineWorld.Blocks.Runtime;

namespace MineWorld.World.Chunks;

public static class ChunkGenerationPipelineTests
{
    public static void Run()
    {
        var registry = new BlockRegistry();
        registry.Register(new BlockDefinition("mineworld:air", "air", 0, Array.Empty<string>(), new Dictionary<string, string>()));
        registry.Register(new BlockDefinition("mineworld:stone", "stone", 1.5f, new[] { "solid" }, new Dictionary<string, string>()));
        registry.Register(new BlockDefinition("mineworld:dirt", "dirt", 0.6f, new[] { "solid" }, new Dictionary<string, string>()));
        registry.Register(new BlockDefinition("mineworld:grass", "grass", 0.6f, new[] { "solid", "natural" }, new Dictionary<string, string>()));
        registry.Freeze();

        var pipeline = new ChunkGenerationPipeline(registry);
        var a = pipeline.Generate(7, -3);
        var b = pipeline.Generate(7, -3);

        if (a.Width != 16 || a.Depth != 16) throw new Exception("Unexpected chunk dimensions.");
        for (var z = 0; z < a.Depth; z++)
        for (var x = 0; x < a.Width; x++)
        for (var y = 0; y < a.Height; y++)
            if (a.Get(x, y, z) != b.Get(x, y, z)) throw new Exception("Generation is not deterministic.");
    }
}
