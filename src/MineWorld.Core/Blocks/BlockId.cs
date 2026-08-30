namespace MineWorld.Core.Blocks;

/// <summary>
/// Stable identifier for a block definition.
/// </summary>
public readonly record struct BlockId(int Value)
{
    public static readonly BlockId Air = new(0);

    public override string ToString() => Value.ToString();
}
