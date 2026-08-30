using System.Text.Json;
using MineWorld.Core.Entities;

namespace MineWorld.Playable;

public sealed record SavedEntity(string Id, EntityKind Kind, double X, double Y, double Z);

internal sealed record LoadedWorldState(VoxelWorld World, IReadOnlyList<SavedEntity> Entities);

internal static class EntityPersistence
{
    public static SavedEntity Capture(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new SavedEntity(entity.Id.Value, entity.Kind, entity.Position.X, entity.Position.Y, entity.Position.Z);
    }

    public static IReadOnlyList<SavedEntity> DeserializeEntities(string json)
    {
        var data = JsonSerializer.Deserialize<List<SavedEntity>>(json);
        return data ?? [];
    }
}
