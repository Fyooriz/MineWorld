using System.Numerics;

namespace MineWorld.Playable;

/// <summary>Cheap CPU-side visibility gate used before issuing GPU draw calls.</summary>
internal sealed class ChunkVisibilityPolicy
{
    public ChunkVisibilityPolicy(int renderDistanceChunks = 8)
    {
        RenderDistanceChunks = Math.Max(1, renderDistanceChunks);
    }

    public int RenderDistanceChunks { get; }

    public bool IsVisible(ChunkKey chunk, Vector3 cameraPosition, int chunkSize)
    {
        var centerX = (chunk.X + 0.5f) * chunkSize;
        var centerZ = (chunk.Z + 0.5f) * chunkSize;
        var dx = centerX - cameraPosition.X;
        var dz = centerZ - cameraPosition.Z;
        var radius = RenderDistanceChunks * chunkSize;
        return dx * dx + dz * dz <= radius * radius;
    }
}
