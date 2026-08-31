namespace MineWorld.Core.Entities;

/// <summary>
/// Owns live entity instances for the runtime simulation.
/// EntityRegistry remains responsible for entity definitions.
/// </summary>
public sealed class EntityRuntime
{
    private readonly Dictionary<EntityId, IEntity> _entities = new();

    public int Count => _entities.Count;

    public IReadOnlyList<IEntity> Snapshot() => _entities.Values.ToArray();

    public void Add(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (string.IsNullOrWhiteSpace(entity.Id.Value))
            throw new ArgumentException("Entity ID cannot be empty.", nameof(entity));
        if (!_entities.TryAdd(entity.Id, entity))
            throw new InvalidOperationException($"Entity '{entity.Id}' is already active.");
    }

    public bool Remove(EntityId id) => _entities.Remove(id);

    public bool TryGet(EntityId id, out IEntity entity) => _entities.TryGetValue(id, out entity!);

    public void Tick(EntityTickContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Snapshot the collection so a future lifecycle operation can add/remove entities safely
        // without invalidating the current simulation iteration.
        foreach (var entity in _entities.Values.ToArray())
            entity.Tick(context);
    }
}
