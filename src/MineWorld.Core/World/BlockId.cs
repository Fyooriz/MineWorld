namespace MineWorld.Core.World;

/// <summary>Stable numeric identifier for a MineWorld block type.</summary>
public readonly record struct BlockId(int Value)
{
    public static readonly BlockId Air = new(0);
    public static readonly BlockId Stone = new(1);
    public static readonly BlockId Dirt = new(2);
    public static readonly BlockId Grass = new(3);
    public static readonly BlockId Wood = new(4);

    public override string ToString() => Value.ToString();
}
