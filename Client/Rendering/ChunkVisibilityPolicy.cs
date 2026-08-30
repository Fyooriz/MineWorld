using System.Numerics;

namespace MineWorld.Playable;

/// <summary>CPU-side visibility gate. Distance is authoritative for P0; camera direction is accepted for future frustum culling.</summary>
internal sealed class ChunkVisibilityPolicy
{
    public ChunkVisibilityPolicy(int renderDistanceChunks = 8)
    {
        RenderDistanceChunks = Math.Max(1, renderDistanceChunks);
    }

    public int RenderDistanceChunks { get; }

    public bool IsVisible(ChunkKey chunk, Vector3 cameraPosition, int chunkSize)
    {
        var center = GetCenter(chunk, chunkSize);
        var radius = RenderDistanceChunks * Math.Max(1, chunkSize);
        return DistanceSquaredXZ(center, cameraPosition) <= radius * radius;
    }

    public bool IsVisible(ChunkKey chunk, Vector3 cameraPosition, Vector3 cameraForward, int chunkSize, float horizontalFovDegrees)
    {
        var center = GetCenter(chunk, chunkSize);
        var radius = RenderDistanceChunks * Math.Max(1, chunkSize);
        if (DistanceSquaredXZ(center, cameraPosition) > radius * radius) return false;

        var toChunk = new Vector2(center.X - cameraPosition.X, center.Z - cameraPosition.Z);
        var forward = new Vector2(cameraForward.X, cameraForward.Z);
        if (toChunk.LengthSquared() < 0.0001f || forward.LengthSquared() < 0.0001f) return true;

        toChunk = Vector2.Normalize(toChunk);
        forward = Vector2.Normalize(forward);
        var halfFov = MathF.Max(5f, horizontalFovDegrees) * MathF.PI / 360f;
        var cosLimit = MathF.Cos(halfFov);
        return Vector2.Dot(toChunk, forward) >= cosLimit;
    }

    private static Vector3 GetCenter(ChunkKey chunk, int chunkSize) =>
        new((chunk.X + 0.5f) * chunkSize, 0f, (chunk.Z + 0.5f) * chunkSize);

    private static float DistanceSquaredXZ(Vector3 a, Vector3 b)
    {
        var dx = a.X - b.X;
        var dz = a.Z - b.Z;
        return dx * dx + dz * dz;
    }
}
