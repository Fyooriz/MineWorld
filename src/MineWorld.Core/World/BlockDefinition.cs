namespace MineWorld.Core.World;

public sealed record BlockDefinition(
    BlockId Id,
    string Name,
    bool IsSolid = true,
    bool IsOpaque = true,
    bool IsCollidable = true,
    float Hardness = 1f);
