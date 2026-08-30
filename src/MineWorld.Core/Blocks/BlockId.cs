namespace MineWorld.Core.Blocks;

/// <summary>
/// Stable identifier for a block definition.
/// </summary>
public readonly record struct BlockId(string Value)
{
    public static readonly BlockId Air = new("core:air");

    public override string ToString() => Value;
}
