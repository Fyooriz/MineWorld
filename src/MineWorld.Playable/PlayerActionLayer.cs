using MineWorld.Core.Crafting;
using MineWorld.Core.Inventory;
using MineWorld.Core.Player;

namespace MineWorld.Playable;

internal sealed class PlayerActionLayer
{
    private readonly CraftingService _crafting;
    private readonly RecipeDefinition _p0Recipe;

    public PlayerActionLayer(CraftingService? crafting = null, RecipeDefinition? p0Recipe = null)
    {
        _crafting = crafting ?? new CraftingService();
        _p0Recipe = p0Recipe ?? new RecipeDefinition(
            "p0:dirt-from-grass",
            [new ItemStack("core:grass", 1)],
            new ItemStack("core:dirt", 1));
    }

    public bool TryCraft(PlayerState player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return _crafting.TryCraft(player.Inventory, _p0Recipe);
    }
}
