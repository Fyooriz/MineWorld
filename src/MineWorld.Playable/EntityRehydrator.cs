using MineWorld.Core.Entities;

namespace MineWorld.Playable;

internal static class EntityRehydrator
{
    public static IEntity Rehydrate(SavedEntity snapshot, Func<SavedEntity, IEntity> factory)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(factory);
        if (string.IsNullOrWhiteSpace(snapshot.Id))
            throw new InvalidDataException("Saved entity ID cannot be empty.");

        var entity = factory(snapshot)
            ?? throw new InvalidDataException($"Entity factory returned null for '{snapshot.Id}'.");

        if (entity.Id.Value != snapshot.Id)
            throw new InvalidDataException($"Entity factory returned ID '{entity.Id.Value}' for '{snapshot.Id}'.");
        if (entity.Kind != snapshot.Kind)
            throw new InvalidDataException($"Entity factory returned kind '{entity.Kind}' for '{snapshot.Id}'.");
        if (entity.Position != new EntityPosition(snapshot.X, snapshot.Y, snapshot.Z))
            throw new InvalidDataException($"Entity factory returned the wrong position for '{snapshot.Id}'.");

        return entity;
    }

    public static IReadOnlyList<IEntity> RehydrateAll(
        IEnumerable<SavedEntity> snapshots,
        Func<SavedEntity, IEntity> factory)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        ArgumentNullException.ThrowIfNull(factory);

        var result = new List<IEntity>();
        var ids = new HashSet<EntityId>();
        foreach (var snapshot in snapshots)
        {
            var entity = Rehydrate(snapshot, factory);
            if (!ids.Add(entity.Id))
                throw new InvalidDataException($"Duplicate entity ID '{entity.Id}'.");
            result.Add(entity);
        }

        return result;
    }
}
