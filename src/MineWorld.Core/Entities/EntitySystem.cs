namespace MineWorld.Core.Entities;

public readonly record struct EntityPosition(double X, double Y, double Z);

public interface IEntity
{
    EntityId Id { get; }
    EntityKind Kind { get; }
    EntityPosition Position { get; }
    void Tick(EntityTickContext context);
}

public sealed record EntityTickContext(long Tick, double DeltaSeconds);

public abstract class EntityBase(EntityId id, EntityKind kind, EntityPosition position) : IEntity
{
    public EntityId Id { get; } = id;
    public EntityKind Kind { get; } = kind;
    public EntityPosition Position { get; protected set; } = position;
    public abstract void Tick(EntityTickContext context);
}
