namespace MineWorld.Core.World;

public sealed class BlockRegistry
{
    private readonly Dictionary<BlockId, BlockDefinition> _definitions = new();

    public IEnumerable<BlockDefinition> Definitions => _definitions.Values;

    public void Register(BlockDefinition definition)
    {
        if (definition.Id.Value == 0 && definition.Name != "air")
            throw new InvalidOperationException("Block ID 0 is reserved for air.");

        if (!_definitions.TryAdd(definition.Id, definition))
            throw new InvalidOperationException($"Block ID already registered: {definition.Id.Value}");
    }

    public BlockDefinition Get(BlockId id) =>
        _definitions.TryGetValue(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Unknown block ID: {id.Value}");

    public bool TryGet(BlockId id, out BlockDefinition? definition) =>
        _definitions.TryGetValue(id, out definition);

    public static BlockRegistry CreateDefault()
    {
        var registry = new BlockRegistry();
        registry.Register(new BlockDefinition(new BlockId(0), "air", false, false, false, 0));
        registry.Register(new BlockDefinition(new BlockId(1), "stone", true, true, true, 1.5f));
        registry.Register(new BlockDefinition(new BlockId(2), "dirt", true, true, true, 0.5f));
        registry.Register(new BlockDefinition(new BlockId(3), "grass", true, true, true, 0.6f));
        registry.Register(new BlockDefinition(new BlockId(4), "wood", true, true, true, 2f));
        return registry;
    }
}
