using System.Numerics;
using System.Text.Json;
using MineWorld.Core.Player;
using MineWorld.Playable;

namespace MineWorld.Playable.Tests;

public sealed class P0PersistenceHardeningTests
{
    [Fact]
    public void SaveWritesCurrentVersionAndPlayerPosition()
    {
        var world = new VoxelWorld(seed: 9876, renderDistance: 1);
        var player = new PlayerState
        {
            Name = "PersistenceTest",
            Health = 17.5f
        };
        var position = new Vector3(12.5f, 34.25f, -8.75f);
        var path = CreateTempPath();

        try
        {
            WorldPersistence.Save(world, path, [], player, position);

            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement;
            Assert.Equal(WorldPersistence.CurrentSaveVersion, root.GetProperty("SaveVersion").GetInt32());
            Assert.Equal(player.Name, root.GetProperty("Player").GetProperty("Name").GetString());
            Assert.Equal(player.Health, root.GetProperty("Player").GetProperty("Health").GetSingle());
            Assert.Equal(position.X, root.GetProperty("Player").GetProperty("Position").GetProperty("X").GetSingle());
            Assert.Equal(position.Y, root.GetProperty("Player").GetProperty("Position").GetProperty("Y").GetSingle());
            Assert.Equal(position.Z, root.GetProperty("Player").GetProperty("Position").GetProperty("Z").GetSingle());
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    [Fact]
    public void LegacyVersionZeroSaveStillLoads()
    {
        var path = CreateTempPath();
        try
        {
            File.WriteAllText(path, "{\"Seed\":4242,\"Blocks\":[]}");

            var loaded = WorldPersistence.LoadState(path, renderDistance: 1);

            Assert.Equal(4242, loaded.World.Seed);
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    [Fact]
    public void UnsupportedSaveVersionIsRejected()
    {
        var path = CreateTempPath();
        try
        {
            File.WriteAllText(path, "{\"SaveVersion\":999,\"Seed\":42,\"Blocks\":[]}");

            var exception = Assert.Throws<InvalidDataException>(() => WorldPersistence.LoadState(path, renderDistance: 1));

            Assert.Contains("Unsupported MineWorld save version", exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    [Fact]
    public void MissingBlockListIsRejected()
    {
        var path = CreateTempPath();
        try
        {
            File.WriteAllText(path, "{\"SaveVersion\":1,\"Seed\":42}");

            var exception = Assert.Throws<InvalidDataException>(() => WorldPersistence.LoadState(path, renderDistance: 1));

            Assert.Contains("missing its block list", exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    [Fact]
    public void PlayerStateAndPositionRoundTripThroughWorldSave()
    {
        var world = new VoxelWorld(seed: 77, renderDistance: 1);
        var player = new PlayerState
        {
            Name = "RoundTrip",
            Health = 12.25f
        };
        player.Inventory.TryAdd(new MineWorld.Core.Inventory.ItemStack("core:grass", 2));
        var position = new Vector3(-4.5f, 38f, 9.25f);
        var path = CreateTempPath();

        try
        {
            WorldPersistence.Save(world, path, [], player, position);
            var loaded = WorldPersistence.LoadState(path, renderDistance: 1);

            Assert.NotNull(loaded.Player);
            var restored = PlayerPersistence.Restore(loaded.Player!);
            Assert.Equal(player.Id, restored.Id);
            Assert.Equal(player.Name, restored.Name);
            Assert.Equal(player.Health, restored.Health);
            Assert.Equal(2, restored.Inventory.Count("core:grass"));
            Assert.NotNull(loaded.Player!.Position);
            Assert.Equal(position.X, loaded.Player.Position!.X);
            Assert.Equal(position.Y, loaded.Player.Position.Y);
            Assert.Equal(position.Z, loaded.Player.Position.Z);
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    [Fact]
    public void SettingAnOverrideBackToGeneratedStateRemovesTheOverride()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        const int x = 0;
        const int z = 0;
        var y = world.GetSurfaceHeight(x, z);

        Assert.Equal(VoxelWorld.Grass, world.GetBlock(x, y, z));
        Assert.Empty(world.BlockOverrides);

        world.SetBlock(x, y, z, VoxelWorld.Dirt);
        Assert.Equal(VoxelWorld.Dirt, world.GetBlock(x, y, z));
        Assert.True(world.BlockOverrides.ContainsKey((x, y, z)));

        world.SetBlock(x, y, z, VoxelWorld.Grass);

        Assert.Equal(VoxelWorld.Grass, world.GetBlock(x, y, z));
        Assert.False(world.BlockOverrides.ContainsKey((x, y, z)));
    }

    private static string CreateTempPath()
        => Path.Combine(Path.GetTempPath(), $"mineworld-p0-save-{Guid.NewGuid():N}.json");

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}
