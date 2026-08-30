using System.Text.Json;

namespace MineWorld.Playable;

internal sealed record SavedBlock(int X, int Y, int Z, byte Block);

internal sealed record WorldSaveData(int Seed, List<SavedBlock> Blocks);

internal static class WorldPersistence
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static void Save(VoxelWorld world, string path)
    {
        var blocks = world.BlockOverrides
            .Select(static entry => new SavedBlock(entry.Key.X, entry.Key.Y, entry.Key.Z, entry.Value))
            .OrderBy(static block => block.Y)
            .ThenBy(static block => block.X)
            .ThenBy(static block => block.Z)
            .ToList();

        var data = new WorldSaveData(world.Seed, blocks);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(data, Options));
    }

    public static VoxelWorld Load(string path, int renderDistance)
    {
        if (!File.Exists(path))
            return new VoxelWorld(12345, renderDistance);

        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<WorldSaveData>(json)
            ?? throw new InvalidDataException("MineWorld save file is empty or invalid.");

        var world = new VoxelWorld(data.Seed, renderDistance);
        foreach (var block in data.Blocks)
            world.ApplySavedBlock(block.X, block.Y, block.Z, block.Block);

        return world;
    }
}
