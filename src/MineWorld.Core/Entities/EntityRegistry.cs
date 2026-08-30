namespace MineWorld.Core.Entities;

public sealed class EntityRegistry
{
    private readonly Dictionary<EntityId, EntityDefinition> _definitions = new();

    public void Register(EntityDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (definition.Id.Value.Length == 0) throw new ArgumentException("Entity ID cannot be empty.", nameof(definition));
        if (definition.MaxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(definition), "Max health must be positive.");
        if (!_definitions.TryAdd(definition.Id, definition))
            throw new InvalidOperationException($"Entity '{definition.Id}' is already registered.");
    }

    public bool TryGet(EntityId id, out EntityDefinition definition) => _definitions.TryGetValue(id, out definition!);
}
