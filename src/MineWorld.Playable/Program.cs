using MineWorld.Core.Inventory;
using MineWorld.Core.Player;

namespace MineWorld.Playable;

internal static class Program
{
    private const string SavePath = "saves/p0-world.json";

    public static void Main()
    {
        var runtimeE2E = string.Equals(Environment.GetEnvironmentVariable("MINEWORLD_RUNTIME_E2E"), "1", StringComparison.Ordinal);
        var savePath = runtimeE2E ? "saves/runtime-e2e.json" : SavePath;
        var loaded = WorldPersistence.LoadState(savePath, renderDistance: runtimeE2E ? 1 : 3);
        var state = runtimeE2E
            ? CreateRuntimeE2EState()
            : loaded.Player is null
                ? new PlayerState()
                : PlayerPersistence.Restore(loaded.Player);
        var player = new PlayerController(loaded.World, state: state, initialLookDirection: System.Numerics.Vector3.UnitZ);
        var input = new InputState();

        using IRenderer renderer = new RaylibRenderer(1280, 720, "MineWorld P0");
        var loop = new GameLoop(renderer, input, loaded.World, player, savePath);
        var bootTest = string.Equals(Environment.GetEnvironmentVariable("MINEWORLD_RUNTIME_BOOT"), "1", StringComparison.Ordinal);
        int? maxFrames = null;
        if (runtimeE2E || bootTest)
        {
            var configuredFrames = Environment.GetEnvironmentVariable("MINEWORLD_RUNTIME_E2E_FRAMES");
            maxFrames = int.TryParse(configuredFrames, out var parsed) && parsed > 0 ? parsed : 180;
        }
        loop.Run(maxFrames);
    }

    private static PlayerState CreateRuntimeE2EState()
    {
        var state = new PlayerState();
        state.Name = "RuntimeE2E";
        state.Health = 19.5f;
        state.Inventory.TryAdd(new ItemStack("core:grass", 1));
        return state;
    }
}
