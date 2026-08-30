using System.Numerics;

namespace MineWorld.Playable;

/// <summary>CPU-side chunk visibility using render distance plus an optional camera frustum.</summary>
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
        if (DistanceSquaredXZ(center, cameraPosition) > radius * radius)
            return false;

        var toChunk = new Vector2(center.X - cameraPosition.X, center.Z - cameraPosition.Z);
        var forward = new Vector2(cameraForward.X, cameraForward.Z);
        if (toChunk.LengthSquared() < 0.0001f || forward.LengthSquared() < 0.0001f)
            return true;

        toChunk = Vector2.Normalize(toChunk);
        forward = Vector2.Normalize(forward);
        var halfFov = MathF.Max(5f, horizontalFovDegrees) * MathF.PI / 360f;
        return Vector2.Dot(toChunk, forward) >= MathF.Cos(halfFov);
    }

    public bool IsVisible(ChunkKey chunk, Frustum3D frustum, int chunkSize, int chunkHeight)
    {
        var min = new Vector3(chunk.X * chunkSize, 0f, chunk.Z * chunkSize);
        var max = new Vector3((chunk.X + 1) * chunkSize, Math.Max(1, chunkHeight), (chunk.Z + 1) * chunkSize);
        return frustum.IntersectsAabb(min, max);
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

/// <summary>Six-plane view frustum for conservative AABB visibility tests.</summary
internal readonly struct Frustum3D
{
    private readonly Plane[] _planes;

    private Frustum3D(Plane[] planes) => _planes = planes;

    public static Frustum3D FromViewProjection(Matrix4x4 matrix)
    {
        // System.Numerics uses row-vector transforms; clip inequalities are
        // therefore assembled from matrix columns.
        return new Frustum3D(new[]
        {
            Normalize(new Plane(matrix.M11 + matrix.M14, matrix.M21 + matrix.M24, matrix.M31 + matrix.M34, matrix.M41 + matrix.M44)),
            Normalize(new Plane(-matrix.M11 + matrix.M14, -matrix.M21 + matrix.M24, -matrix.M31 + matrix.M34, -matrix.M41 + matrix.M44)),
            Normalize(new Plane(matrix.M11 + matrix.M12, matrix.M21 + matrix.M22, matrix.M31 + matrix.M32, matrix.M41 + matrix.M42)),
            Normalize(new Plane(matrix.M11 - matrix.M12, matrix.M21 - matrix.M22, matrix.M31 - matrix.M32, matrix.M41 - matrix.M42)),
            Normalize(new Plane(matrix.M13 + matrix.M14, matrix.M23 + matrix.M24, matrix.M33 + matrix.M34, matrix.M43 + matrix.M44)),
            Normalize(new Plane(-matrix.M13 + matrix.M14, -matrix.M23 + matrix.M24, -matrix.M33 + matrix.M34, -matrix.M43 + matrix.M44))
        });
    }

    public bool IntersectsAabb(Vector3 min, Vector3 max)
    {
        foreach (var plane in _planes)
        {
            var positive = new Vector3(
                plane.Normal.X >= 0 ? max.X : min.X,
                plane.Normal.Y >= 0 ? max.Y : min.Y,
                plane.Normal.Z >= 0 ? max.Z : min.Z);

            if (Plane.DotCoordinate(plane, positive) < 0f)
                return false;
        }

        return true;
    }

    private static Plane Normalize(Plane plane)
    {
        var length = plane.Normal.Length();
        return length < 0.000001f ? plane : new Plane(plane.Normal / length, plane.D / length);
    }
}
