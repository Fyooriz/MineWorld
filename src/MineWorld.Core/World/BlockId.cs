namespace MineWorld.Core.World;

/// <summary>Stable identifier for a MineWorld block type.</summary>
public readonly record struct BlockId(string Value)
{
    public static readonly BlockId Air = new("core:air");

    public override string ToString() => Value;
}
