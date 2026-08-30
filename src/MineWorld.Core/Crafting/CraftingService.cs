using InventoryModel = MineWorld.Core.Inventory.Inventory;
using MineWorld.Core.Inventory;

namespace MineWorld.Core.Crafting;

public sealed class CraftingService
{
    public bool TryCraft(InventoryModel inventory, RecipeDefinition recipe, int times = 1)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(recipe);
        if (times <= 0) throw new ArgumentOutOfRangeException(nameof(times));

        var required = recipe.Ingredients
            .GroupBy(static item => item.ItemId)
            .ToDictionary(static group => group.Key, static group => group.Sum(item => item.Count) * times);

        foreach (var requirement in required)
            if (inventory.Count(requirement.Key) < requirement.Value)
                return false;

        foreach (var requirement in required)
            if (!inventory.TryRemove(requirement.Key, requirement.Value))
                throw new InvalidOperationException("Inventory changed during crafting transaction.");

        if (!inventory.TryAdd(recipe.Result with { Count = recipe.Result.Count * times }))
            throw new InvalidOperationException("Crafting result cannot fit in inventory after ingredient removal.");

        return true;
    }
}
