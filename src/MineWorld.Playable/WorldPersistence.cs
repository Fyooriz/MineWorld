using System.Text.Json;
using MineWorld.Core.Entities;
using MineWorld.Core.Player;

namespace MineWorld.Playable;

internal sealed record SaveHeader(
    int FormatVersion,
    string RulesetVersion,
    string GeneratorId,
    int GeneratorVersion);

internal sealed record SavedBlock(int X, int Y, int Z, byte Block);
internal sealed record WorldSaveData(
    SaveHeader Header,
    int Seed,
    List<SavedBlock> Blocks,
    List<SavedEntity>? Entities = null,
    SavedPlayerState? Player = null);

internal static class WorldPersistence
{
    public const int FormatVersion = 1;
    public const string RulesetVersion = "0.2.0";
    public const string GeneratorId = "mineworld:basic";
    public const int GeneratorVersion = 1;

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static void Save(VoxelWorld world, string path)
        => Save(world, path, [], null);

    public static void Save(VoxelWorld world, string path, IEnumerable<IEntity> entities)
        => Save(world, path, entities, null);

    public static void Save(VoxelWorld world, string path, IEnumerable<IEntity> entities, PlayerState? player)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(entities);

        var blocks = world.BlockOverrides
            .Select(static value => new SavedBlock(value.X, value.Y, value.Z, value.Block))
            .OrderBy(static block => block.Y)
            .ThenBy(static block => block.X)
            .ThenBy(static block => block.Z)
            .ToList();

        var savedEntities = entities
            .Select(EntityPersistence.Capture)
            .OrderBy(static entity => entity.Id, StringComparer.Ordinal)
            .ToList();

        var data = new WorldSaveData(
            new SaveHeader(FormatVersion, RulesetVersion, GeneratorId, GeneratorVersion),
            world.Seed,
            blocks,
            savedEntities,
            player is null ? null : PlayerPersistence.Capture(player));

        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var tempPath = $"{fullPath}.{Guid.NewGuid():N}.tmp";
        try
        {
            File.WriteAllText(tempPath, JsonSerializer.Serialize(data, Options));
            File.Move(tempPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public static VoxelWorld Load(string path, int renderDistance)
        => LoadState(path, renderDistance).World;

    public static LoadedWorldState LoadState(string path, int renderDistance)
    {
        if (!File.Exists(path))
            return new LoadedWorldState(new VoxelWorld(12345, renderDistance), [], null);

        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<WorldSaveData>(json)
            ?? throw new InvalidDataException("MineWorld save file is empty or invalid.");

        ValidateHeader(data.Header);

        var world = new VoxelWorld(data.Seed, renderDistance);
        foreach (var block in data.Blocks ?? [])
            world.ApplySavedBlock(block.X, block.Y, block.Z, block.Block);

        var entities = EntityPersistence.DeserializeEntities(JsonSerializer.Serialize(data.Entities ?? []));
        return new LoadedWorldState(world, entities, data.Player);
    }

    private static void ValidateHeader(SaveHeader? header)
    {
        if (header is null)
            throw new InvalidDataException("MineWorld save file is missing its schema header.");
        if (header.FormatVersion != FormatVersion)
            throw new InvalidDataException($"Unsupported MineWorld save format: {header.FormatVersion}.");
        if (!string.Equals(header.RulesetVersion, RulesetVersion, StringComparison.Ordinal))
            throw new InvalidDataException($"Unsupported MineWorld ruleset version: {header.RulesetVersion}.");
        if (!string.Equals(header.GeneratorId, GeneratorId, StringComparison.Ordinal))
            throw new InvalidDataException($"Unsupported MineWorld generator: {header.GeneratorId}.");
        if (header.GeneratorVersion != GeneratorVersion)
            throw new InvalidDataException($"Unsupported MineWorld generator version: {header.GeneratorVersion}.");
    }
}
