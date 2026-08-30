using System;
using MineWorld.Blocks.Runtime;

namespace MineWorld.Blocks.Tests;

public static class BlockRegistryTests
{
    public static void Run()
    {
        var registry = new BlockRegistry();
        registry.Register(new BlockDefinition("mineworld:stone", "stone", 1.5f, Array.Empty<string>(), new Dictionary<string, string>()));
        registry.Register(new BlockDefinition("mineworld:air", "air", 0f, Array.Empty<string>(), new Dictionary<string, string>()));
        registry.Freeze();

        if (registry.GetRuntimeId("mineworld:air") != 0) throw new Exception("Runtime IDs must be deterministic.");
        if (registry.GetRuntimeId("mineworld:stone") != 1) throw new Exception("Runtime IDs must be sorted by canonical ID.");
        if (registry.CreateDefaultState("mineworld:stone").BlockId != "mineworld:stone") throw new Exception("Default state mismatch.");

        try
        {
            registry.Register(new BlockDefinition("mineworld:dirt", "dirt", 0.5f, Array.Empty<string>(), new Dictionary<string, string>()));
            throw new Exception("Frozen registry accepted a new block.");
        }
        catch (InvalidOperationException) { }
    }
}
