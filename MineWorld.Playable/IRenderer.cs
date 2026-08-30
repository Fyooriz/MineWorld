using System.Numerics;

namespace MineWorld.Playable;

internal interface IRenderer : IDisposable
{
    int Width { get; }
    int Height { get; }
    bool ShouldClose { get; }
    Camera3DState BeginFrame(Vector3 position, Vector3 target);
    void RenderWorld(VoxelWorld world);
    void DrawHud(PlayerController player, VoxelWorld world);
    void EndFrame();
}

internal readonly record struct Camera3DState(Vector3 Position, Vector3 Target, Vector3 Up, float FovY);
