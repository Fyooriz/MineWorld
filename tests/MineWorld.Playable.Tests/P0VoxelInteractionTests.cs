using System.Numerics;
using MineWorld.Core.Inventory;
using MineWorld.Playable;
using Raylib_cs;

namespace MineWorld.Playable.Tests;

public sealed class P0VoxelInteractionTests
{
    [Fact]
    public void MineThenPlaceRestoresBlockAndInventoryTransaction()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var inventory = new Inventory(capacity: 4);
        var surfaceY = world.GetSurfaceHeight(0, 0);
        var ray = new Ray(new Vector3(0.5f, surfaceY + 2f, 0.5f), -Vector3.UnitY);

        var original = world.GetBlock(0, surfaceY, 0);
        Assert.NotEqual(VoxelWorld.Air, original);

        Assert.True(world.Mine(ray, inventory));
        Assert.Equal(VoxelWorld.Air, world.GetBlock(0, surfaceY, 0));
        Assert.Equal(1, inventory.Count("core:grass"));

        Assert.True(world.Place(ray, inventory));
        Assert.Equal(VoxelWorld.Dirt, world.GetBlock(0, surfaceY, 0));
        Assert.Equal(0, inventory.Count("core:dirt"));
    }

    [Fact]
    public void MiningWithFullInventoryLeavesWorldUnchanged()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var inventory = new Inventory(capacity: 1, maxStackSize: 1);
        Assert.True(inventory.TryAdd(new ItemStack("core:stone", 1)));

        var surfaceY = world.GetSurfaceHeight(0, 0);
        var ray = new Ray(new Vector3(0.5f, surfaceY + 2f, 0.5f), -Vector3.UnitY);
        var original = world.GetBlock(0, surfaceY, 0);

        Assert.False(world.Mine(ray, inventory));
        Assert.Equal(original, world.GetBlock(0, surfaceY, 0));
        Assert.Equal(1, inventory.Count("core:stone"));
    }
}
