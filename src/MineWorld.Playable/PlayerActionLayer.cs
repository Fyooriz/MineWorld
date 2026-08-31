using MineWorld.Core.Crafting;
using MineWorld.Core.Inventory;

namespace MineWorld.Playable;

internal sealed class PlayerActionLayer
{
    private readonly CraftingService _crafting;
    private readonly RecipeDefinition _p0Recipe;

    public PlayerActionLayer(CraftingService crafting, RecipeDefinition p0Recipe)
    {
        _crafting = crafting ?? throw new ArgumentNullException(nameof(crafting));
        _p0Recipe = p0Recipe ?? throw new ArgumentNullException(nameof(p0Recipe));
    }

    public bool TryCraft(PlayerStateAdapter player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return _crafting.TryCraft(player.Inventory, _p0Recipe);
    }
}

internal interface PlayerStateAdapter
{
    Inventory Inventory { get; }
}
