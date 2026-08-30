using MineWorld.Core.Inventory;

namespace MineWorld.Tests;

public sealed class InventoryTests
{
    [Fact]
    public void AddSplitsAcrossStacks()
    {
        var inventory = new Inventory(2, 64);
        Assert.True(inventory.TryAdd(new ItemStack("stone", 100)));
        Assert.Equal(64, inventory.GetSlot(0).Count);
        Assert.Equal(36, inventory.GetSlot(1).Count);
    }

    [Fact]
    public void AddFailsWithoutMutatingWhenCapacityIsInsufficient()
    {
        var inventory = new Inventory(1, 64);
        Assert.False(inventory.TryAdd(new ItemStack("stone", 65)));
        Assert.True(inventory.GetSlot(0).IsEmpty);
        Assert.Equal(0, inventory.Count("stone"));
    }

    [Fact]
    public void AddFailsAtomicallyWhenExistingStackCannotFitWholeRequest()
    {
        var inventory = new Inventory(2, 64);
        Assert.True(inventory.TryAdd(new ItemStack("stone", 60)));
        Assert.True(inventory.TryAdd(new ItemStack("dirt", 64)));

        Assert.False(inventory.TryAdd(new ItemStack("stone", 5)));
        Assert.Equal(60, inventory.GetSlot(0).Count);
        Assert.Equal("dirt", inventory.GetSlot(1).ItemId);
        Assert.Equal(60, inventory.Count("stone"));
    }

    [Fact]
    public void RemoveCanConsumeAcrossStacks()
    {
        var inventory = new Inventory(2, 64);
        Assert.True(inventory.TryAdd(new ItemStack("stone", 100)));
        Assert.True(inventory.TryRemove("stone", 70));
        Assert.Equal(30, inventory.Count("stone"));
    }
}
