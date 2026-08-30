namespace MineWorld.Core.Blocks;

public sealed record BlockDefinition(
    byte NumericId,
    string Id,
    string DisplayName,
    string ItemId,
    bool Solid = true,
    bool Breakable = true);
