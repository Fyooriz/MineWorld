using System.Numerics;
using MineWorld.Core.Inventory;
using MineWorld.Playable;
using Raylib_cs;

namespace MineWorld.Playable.Tests;

public sealed class P0IntegrationTests
{
    [Fact]
    public void MineThenPlaceDirtPreservesWorldInventoryInvariant()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var inventory = new Inventory(capacity: 4);
        var y = world.GetSurfaceHeight(0, 0);
        var miningRay = new Ray(new Vector3(0.5f, y + 2f, 0.5f), -Vector3.UnitY);

        Assert.Equal(VoxelWorld.Grass, world.GetBlock(0, y, 0));
        Assert.True(world.Mine(miningRay, inventory));
        Assert.Equal(VoxelWorld.Air, world.GetBlock(0, y, 0));
        Assert.Equal(1, inventory.Count("core:grass"));

        Assert.True(inventory.TryAdd(new ItemStack("core:dirt", 1)));
        var placementRay = new Ray(new Vector3(0.5f, y + 0.5f, 0.5f), Vector3.UnitX);
        Assert.True(world.Place(placementRay, inventory));
        Assert.Equal(VoxelWorld.Dirt, world.GetBlock(0, y, 0));
        Assert.Equal(0, inventory.Count("core:dirt"));
    }

    [Fact]
    public void MiningWithFullInventoryLeavesWorldUnchanged()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var inventory = new Inventory(capacity: 1, maxStackSize: 1);
        Assert.True(inventory.TryAdd(new ItemStack("core:stone", 1)));

        var y = world.GetSurfaceHeight(0, 0);
        var ray = new Ray(new Vector3(0.5f, y + 2f, 0.5f), -Vector3.UnitY);
        var original = world.GetBlock(0, y, 0);

        Assert.False(world.Mine(ray, inventory));
        Assert.Equal(original, world.GetBlock(0, y, 0));
        Assert.Equal(1, inventory.Count("core:stone"));
    }

    [Fact]
    public void SaveReloadPreservesModifiedBlock()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var x = 0;
        var z = 0;
        var y = world.GetSurfaceHeight(x, z);
        world.SetBlock(x, y, z, VoxelWorld.Air);

        var path = Path.Combine(Path.GetTempPath(), $"mineworld-p0-{Guid.NewGuid():N}.json");
        try
        {
            WorldPersistence.Save(world, path);
            var reloaded = WorldPersistence.Load(path, renderDistance: 1);
            Assert.Equal(VoxelWorld.Air, reloaded.GetBlock(x, y, z));
            Assert.Equal(world.Seed, reloaded.Seed);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
