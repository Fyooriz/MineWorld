namespace MineWorld.Core.Entities;

public readonly record struct EntityId(string Value)
{
    public override string ToString() => Value;
}

public enum EntityKind
{
    Passive,
    Neutral,
    Hostile,
    Npc,
    Boss
}

public sealed record EntityDefinition(
    EntityId Id,
    EntityKind Kind,
    float MaxHealth,
    float MoveSpeed);
