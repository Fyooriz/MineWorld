using System.Numerics;

namespace MineWorld.Client.Rendering;

public readonly record struct CameraState(
    Vector3 Position,
    float Yaw,
    float Pitch,
    float FieldOfView,
    float AspectRatio,
    float NearClip,
    float FarClip);
