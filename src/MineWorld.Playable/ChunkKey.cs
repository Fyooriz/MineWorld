namespace MineWorld.Playable;

/// <summary>Stable horizontal chunk identity used by streaming and generation.</summary>
internal readonly record struct ChunkKey(int X, int Z);
