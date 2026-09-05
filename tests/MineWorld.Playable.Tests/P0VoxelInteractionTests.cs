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
        var miningRay = new Ray(new Vector3(0.5f, surfaceY + 2f, 0.5f), -Vector3.UnitY);

        var original = world.GetBlock(0, surfaceY, 0);
        Assert.Equal(VoxelWorld.Grass, original);

        Assert.True(world.Mine(miningRay, inventory));
        Assert.Equal(VoxelWorld.Air, world.GetBlock(0, surfaceY, 0));
        Assert.Equal(1, inventory.Count("core:grass"));

        Assert.True(inventory.TryAdd(new ItemStack("core:dirt", 1)));
        var placementRay = new Ray(new Vector3(0.5f, surfaceY + 2f, 0.5f), -Vector3.UnitY);
        Assert.True(world.Place(placementRay, inventory));
        Assert.Equal(VoxelWorld.Dirt, world.GetBlock(0, surfaceY, 0));
        Assert.Equal(0, inventory.Count("core:dirt"));
    }

    [Fact]
    public void SetBlockBackToGeneratedStateRemovesOverride()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var y = world.GetSurfaceHeight(0, 0);
        var key = (X: 0, Y: y, Z: 0);

        Assert.Equal(VoxelWorld.Grass, world.GetBlock(key.X, key.Y, key.Z));
        Assert.False(world.BlockOverrides.ContainsKey(key));

        world.SetBlock(key.X, key.Y, key.Z, VoxelWorld.Air);
        Assert.True(world.BlockOverrides.ContainsKey(key));
        Assert.Equal(VoxelWorld.Air, world.GetBlock(key.X, key.Y, key.Z));

        world.SetBlock(key.X, key.Y, key.Z, VoxelWorld.Grass);
        Assert.False(world.BlockOverrides.ContainsKey(key));
        Assert.Equal(VoxelWorld.Grass, world.GetBlock(key.X, key.Y, key.Z));
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
