namespace MineWorld.Core.Content;

public readonly record struct ItemId(string Value)
{
    public override string ToString() => Value;
}

public sealed record ItemDefinition(
    ItemId Id,
    string DisplayName,
    int MaxStackSize = 64,
    int MaxDurability = 0);
