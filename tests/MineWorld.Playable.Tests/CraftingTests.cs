using MineWorld.Core.Crafting;
using MineWorld.Core.Inventory;
using Xunit;

namespace MineWorld.Playable.Tests;

public sealed class CraftingTests
{
    [Fact]
    public void TryCraft_ConsumesIngredientsAndAddsResult()
    {
        var inventory = new Inventory(4);
        inventory.TryAdd(new ItemStack("wood", 2));
        var recipe = new RecipeDefinition("plank", new[] { new ItemStack("wood", 2) }, new ItemStack("plank", 4));

        Assert.True(new CraftingService().TryCraft(inventory, recipe));
        Assert.Equal(0, inventory.Count("wood"));
        Assert.Equal(4, inventory.Count("plank"));
    }

    [Fact]
    public void TryCraft_DoesNotMutateInventoryWhenIngredientsAreMissing()
    {
        var inventory = new Inventory(4);
        inventory.TryAdd(new ItemStack("wood", 1));
        var recipe = new RecipeDefinition("plank", new[] { new ItemStack("wood", 2) }, new ItemStack("plank", 4));

        Assert.False(new CraftingService().TryCraft(inventory, recipe));
        Assert.Equal(1, inventory.Count("wood"));
        Assert.Equal(0, inventory.Count("plank"));
    }
}
