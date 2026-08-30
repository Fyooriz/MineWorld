namespace MineWorld.Core.Items;

public readonly record struct ItemId(int Value);
public sealed record ItemDefinition(ItemId Id, string Name, int MaxStack = 64, int MaxDurability = 0);

public sealed class ItemRegistry
{
    private readonly Dictionary<ItemId, ItemDefinition> _definitions = new();

    public void Register(ItemDefinition definition)
    {
        if (definition.Id.Value < 0) throw new ArgumentOutOfRangeException(nameof(definition));
        if (definition.MaxStack is < 1 or > 999) throw new ArgumentOutOfRangeException(nameof(definition));
        if (!_definitions.TryAdd(definition.Id, definition))
            throw new InvalidOperationException($"Item ID already registered: {definition.Id.Value}");
    }

    public ItemDefinition Get(ItemId id) => _definitions.TryGetValue(id, out var item)
        ? item : throw new KeyNotFoundException($"Unknown item ID: {id.Value}");
}

public readonly record struct ItemStack(ItemId Item, int Count, int Durability = 0);
