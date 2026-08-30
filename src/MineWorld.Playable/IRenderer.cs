using System.Numerics;

namespace MineWorld.Playable;

internal interface IRenderer : IDisposable
{
    bool ShouldClose { get; }
    int Width { get; }
    int Height { get; }
    void BeginFrame(Vector3 position, Vector3 target);
    void RenderWorld(VoxelWorld world);
    void DrawHud(PlayerController player, VoxelWorld world);
    void EndFrame();
}
