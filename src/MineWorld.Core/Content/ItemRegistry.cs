namespace MineWorld.Core.Content;

public sealed class ItemRegistry
{
    private readonly Dictionary<ItemId, ItemDefinition> _definitions = new();

    public void Register(ItemDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (definition.Id.Value.Length == 0) throw new ArgumentException("Item ID cannot be empty.", nameof(definition));
        if (!_definitions.TryAdd(definition.Id, definition))
            throw new InvalidOperationException($"Item '{definition.Id}' is already registered.");
    }

    public bool TryGet(ItemId id, out ItemDefinition definition) => _definitions.TryGetValue(id, out definition!);
}
