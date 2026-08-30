namespace MineWorld.Core.Blocks;

public sealed class BlockRegistry
{
    private readonly Dictionary<byte, BlockDefinition> _byNumericId = new();
    private readonly Dictionary<string, BlockDefinition> _byId = new(StringComparer.Ordinal);

    public void Register(BlockDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (definition.NumericId == 0 || string.IsNullOrWhiteSpace(definition.Id))
            throw new ArgumentException("A block must have a non-zero numeric ID and a stable ID.", nameof(definition));
        if (_byNumericId.ContainsKey(definition.NumericId) || _byId.ContainsKey(definition.Id))
            throw new InvalidOperationException($"Block already registered: {definition.Id}");

        _byNumericId.Add(definition.NumericId, definition);
        _byId.Add(definition.Id, definition);
    }

    public BlockDefinition Get(byte numericId) => _byNumericId[numericId];
    public bool TryGet(byte numericId, out BlockDefinition? definition) => _byNumericId.TryGetValue(numericId, out definition);
    public bool TryGet(string id, out BlockDefinition? definition) => _byId.TryGetValue(id, out definition);
}
