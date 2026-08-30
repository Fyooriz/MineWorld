using System.Numerics;

namespace MineWorld.Playable;

internal readonly struct Frustum
{
    private readonly Plane _left;
    private readonly Plane _right;
    private readonly Plane _bottom;
    private readonly Plane _top;
    private readonly Plane _near;
    private readonly Plane _far;

    private Frustum(Plane left, Plane right, Plane bottom, Plane top, Plane near, Plane far)
    {
        _left = left; _right = right; _bottom = bottom; _top = top; _near = near; _far = far;
    }

    public static Frustum Create(Vector3 position, Vector3 target, Vector3 up, float fovDegrees, float aspect, float near, float far)
    {
        var forward = Vector3.Normalize(target - position);
        var right = Vector3.Normalize(Vector3.Cross(forward, up));
        var correctedUp = Vector3.Normalize(Vector3.Cross(right, forward));
        var fov = fovDegrees * MathF.PI / 180f;
        var halfV = MathF.Tan(fov * 0.5f) * near;
        var halfH = halfV * aspect;
        var nearCenter = position + forward * near;
        var farCenter = position + forward * far;
        var farV = MathF.Tan(fov * 0.5f) * far;
        var farH = farV * aspect;

        return new Frustum(
            Plane.CreateFromPoints(position, nearCenter + correctedUp * halfV, nearCenter - right * halfH),
            Plane.CreateFromPoints(position, nearCenter - correctedUp * halfV, nearCenter + right * halfH),
            Plane.CreateFromPoints(position, nearCenter - right * halfH, nearCenter - correctedUp * halfV),
            Plane.CreateFromPoints(position, nearCenter + correctedUp * halfV, nearCenter + right * halfH),
            new Plane(forward, -Vector3.Dot(forward, nearCenter)),
            new Plane(-forward, Vector3.Dot(forward, farCenter)));
    }

    public bool Intersects(BoundingBox box)
        => Inside(_left, box) && Inside(_right, box) && Inside(_bottom, box)
        && Inside(_top, box) && Inside(_near, box) && Inside(_far, box);

    private static bool Inside(Plane plane, BoundingBox box)
    {
        var p = new Vector3(
            plane.Normal.X >= 0 ? box.Max.X : box.Min.X,
            plane.Normal.Y >= 0 ? box.Max.Y : box.Min.Y,
            plane.Normal.Z >= 0 ? box.Max.Z : box.Min.Z);
        return Vector3.Dot(plane.Normal, p) + plane.D >= 0;
    }
}

internal readonly record struct BoundingBox(Vector3 Min, Vector3 Max);
