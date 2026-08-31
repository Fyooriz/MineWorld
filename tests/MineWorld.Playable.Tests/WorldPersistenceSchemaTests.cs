using System.Numerics;
using System.Text.Json;
using MineWorld.Core.Inventory;
using MineWorld.Core.Player;

namespace MineWorld.Playable.Tests;

public sealed class WorldPersistenceSchemaTests
{
    [Fact]
    public void SaveWritesCurrentFormatVersion()
    {
        var world = new VoxelWorld(seed: 77, renderDistance: 1);
        var path = Path.Combine(Path.GetTempPath(), $"mineworld-save-version-{Guid.NewGuid():N}.json");

        try
        {
            WorldPersistence.Save(world, path);
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(
                WorldPersistence.CurrentSaveVersion,
                document.RootElement.GetProperty("SaveVersion").GetInt32());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void LegacyPreVersionSaveRemainsReadable()
    {
        var path = Path.Combine(Path.GetTempPath(), $"mineworld-legacy-{Guid.NewGuid():N}.json");
        const string legacyJson = "{\"Seed\":77,\"Blocks\":[]}";

        try
        {
            File.WriteAllText(path, legacyJson);
            var loaded = WorldPersistence.LoadState(path, renderDistance: 1);

            Assert.Equal(77, loaded.World.Seed);
            Assert.Empty(loaded.Entities);
            Assert.Null(loaded.Player);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void UnsupportedSaveVersionIsRejected()
    {
        var path = Path.Combine(Path.GetTempPath(), $"mineworld-unsupported-{Guid.NewGuid():N}.json");
        const string json = "{\"SaveVersion\":999,\"Seed\":77,\"Blocks\":[]}";

        try
        {
            File.WriteAllText(path, json);
            var error = Assert.Throws<InvalidDataException>(() => WorldPersistence.LoadState(path, renderDistance: 1));
            Assert.Contains("999", error.Message, StringComparison.Ordinal);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void SavedPlayerRestoresIdentityHealthAndInventory()
    {
        var original = new PlayerState(4)
        {
            Name = "Builder",
            Health = 13.5f
        };
        Assert.True(original.Inventory.TryAdd(new ItemStack("core:dirt", 12)));
        Assert.True(original.Inventory.TryAdd(new ItemStack("core:stone", 3)));

        var restored = PlayerPersistence.Restore(PlayerPersistence.Capture(original));

        Assert.Equal(original.Id, restored.Id);
        Assert.Equal(original.Name, restored.Name);
        Assert.Equal(original.Health, restored.Health);
        Assert.Equal(original.Inventory.Count("core:dirt"), restored.Inventory.Count("core:dirt"));
        Assert.Equal(original.Inventory.Count("core:stone"), restored.Inventory.Count("core:stone"));
        Assert.Equal(original.Inventory.Capacity, restored.Inventory.Capacity);
    }

    [Fact]
    public void SaveAndLoadRoundTripsPlayerState()
    {
        var world = new VoxelWorld(seed: 42, renderDistance: 1);
        var player = new PlayerState
        {
            Name = "PersistentPlayer",
            Health = 17.25f
        };
        Assert.True(player.Inventory.TryAdd(new ItemStack("core:grass", 2)));

        var path = Path.Combine(Path.GetTempPath(), $"mineworld-player-{Guid.NewGuid():N}.json");
        try
        {
            WorldPersistence.Save(world, path, [], player);
            var loaded = WorldPersistence.LoadState(path, renderDistance: 1);

            Assert.NotNull(loaded.Player);
            var restored = PlayerPersistence.Restore(loaded.Player!);
            Assert.Equal(player.Id, restored.Id);
            Assert.Equal(player.Name, restored.Name);
            Assert.Equal(player.Health, restored.Health);
            Assert.Equal(2, restored.Inventory.Count("core:grass"));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
