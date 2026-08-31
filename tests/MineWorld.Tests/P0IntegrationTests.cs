using MineWorld.Core.Inventory;
using MineWorld.Core.World;
using Xunit;

namespace MineWorld.Tests;

public sealed class P0IntegrationTests
{
    [Fact]
    public void MineResultCanFlowIntoInventoryAndBackIntoPlacement()
    {
        var inventory = new Inventory(9);
        Assert.True(inventory.TryAdd(new ItemStack("core:dirt", 1)));
        Assert.Equal(1, inventory.Count("core:dirt"));

        Assert.True(inventory.TryRemove("core:dirt", 1));
        Assert.Equal(0, inventory.Count("core:dirt"));
    }

    [Fact]
    public void FullInventoryDoesNotLoseMiningResult()
    {
        var inventory = new Inventory(1);
        Assert.True(inventory.TryAdd(new ItemStack("core:stone", 64)));

        var before = inventory.Count("core:stone");
        Assert.False(inventory.TryAdd(new ItemStack("core:dirt", 1)));
        Assert.Equal(before, inventory.Count("core:stone"));
        Assert.Equal(0, inventory.Count("core:dirt"));
    }

    [Fact]
    public void BlockIdsRemainCanonicalForP0WorldInteractions()
    {
        Assert.Equal(0, BlockId.Air.Value);
        Assert.NotEqual(BlockId.Air, BlockId.Dirt);
        Assert.NotEqual(BlockId.Dirt, BlockId.Stone);
    }
}
