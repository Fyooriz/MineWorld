using System;
using System.Collections.Generic;

namespace MineWorld.Blocks.Runtime;

public static class BlockInteractionServiceTests
{
    public static void Run()
    {
        var registry = new BlockRegistry();
        registry.Register(new BlockDefinition("mineworld:air", "air", 0, Array.Empty<string>(), new Dictionary<string, string>()));
        registry.Register(new BlockDefinition("mineworld:stone", "stone", 1.5f, new[] { "solid" }, new Dictionary<string, string>()));
        registry.Freeze();

        var service = new BlockInteractionService(registry);
        var air = registry.CreateDefaultState("mineworld:air");
        var stone = registry.CreateDefaultState("mineworld:stone");

        if (!service.CanPlace(air, "mineworld:stone")) throw new Exception("Stone should be placeable into air.");
        if (service.CanPlace(stone, "mineworld:stone")) throw new Exception("Stone should not replace solid block.");
        if (!service.CanMine(stone)) throw new Exception("Stone should be mineable.");
        if (service.Mine(stone).BlockId != "mineworld:air") throw new Exception("Mining should produce air state.");
    }
}
