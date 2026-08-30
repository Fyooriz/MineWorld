using MineWorld.Core.Inventory;

namespace MineWorld.Core.Crafting;

public sealed record RecipeDefinition(string Id, IReadOnlyList<ItemStack> Ingredients, ItemStack Result)
{
    public RecipeDefinition(string id, IEnumerable<ItemStack> ingredients, ItemStack result)
        : this(id, ingredients.ToArray(), result)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Recipe id is required.", nameof(id));
        if (Ingredients.Count == 0 || Ingredients.Any(static item => item.IsEmpty))
            throw new ArgumentException("A recipe requires non-empty ingredients.", nameof(ingredients));
        if (Result.IsEmpty) throw new ArgumentException("Recipe result must be non-empty.", nameof(result));
    }
}
