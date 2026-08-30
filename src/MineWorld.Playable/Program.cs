namespace MineWorld.Playable;

internal static class Program
{
    private const string SavePath = "saves/p0-world.json";

    public static void Main()
    {
        var world = WorldPersistence.Load(SavePath, renderDistance: 3);
        var player = new PlayerController(world);
        var input = new InputState();

        using IRenderer renderer = new RaylibRenderer(1280, 720, "MineWorld P0");
        var loop = new GameLoop(renderer, input, world, player, SavePath);
        loop.Run();
    }
}
