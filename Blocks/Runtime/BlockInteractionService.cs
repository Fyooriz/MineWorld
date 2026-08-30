using System;

namespace MineWorld.Blocks.Runtime;

public sealed class BlockInteractionService
{
    private readonly BlockRegistry _registry;

    public BlockInteractionService(BlockRegistry registry) => _registry = registry;

    public bool CanPlace(BlockState existing, string blockId)
    {
        if (string.Equals(blockId, "mineworld:air", StringComparison.Ordinal)) return false;
        _registry.GetDefinition(blockId);
        return string.Equals(existing.BlockId, "mineworld:air", StringComparison.Ordinal);
    }

    public BlockState Place(string blockId)
    {
        _registry.GetDefinition(blockId);
        return _registry.CreateDefaultState(blockId);
    }

    public bool CanMine(BlockState state) =>
        !string.Equals(state.BlockId, "mineworld:air", StringComparison.Ordinal);

    public BlockState Mine(BlockState state)
    {
        if (!CanMine(state)) throw new InvalidOperationException("Air cannot be mined.");
        return _registry.CreateDefaultState("mineworld:air");
    }
}
