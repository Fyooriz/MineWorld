using System;
using System.Collections.Generic;
using System.IO;
using MineWorld.Blocks.Runtime;
using MineWorld.World.SaveSystem;
using MineWorld.World.Terrain;

namespace MineWorld.World.Chunks;

public static class ChunkWorldServiceTests
{
    public static void Run()
    {
        var registry = new BlockRegistry();
        foreach (var id in new[] { "mineworld:air", "mineworld:stone", "mineworld:dirt", "mineworld:grass", "mineworld:water" })
            registry.Register(new BlockDefinition(id, id[(id.IndexOf(':') + 1)..], 1f, Array.Empty<string>(), new Dictionary<string, string>()));
        registry.Freeze();

        var root = Path.Combine(Path.GetTempPath(), "mineworld-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var save = new WorldSaveService(root);
            var generator = new ChunkGenerationPipeline(registry, 12345L);
            var world = new ChunkWorldService(registry, generator, save);
            var first = world.LoadOrGenerate(0, 0);
            world.SetBlock(0, 0, 1, 40, 1, "mineworld:stone");
            world.Save(0, 0);
            world.Unload(0, 0, false);
            var loaded = world.LoadOrGenerate(0, 0);
            if (loaded.Get(1, 40, 1).BlockId != "mineworld:stone") throw new Exception("Saved block state was not restored.");
            if (!ReferenceEquals(loaded, world.LoadOrGenerate(0, 0))) throw new Exception("Loaded chunk cache is not reused.");
            _ = first;
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }
}
