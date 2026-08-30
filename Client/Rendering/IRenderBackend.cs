using MineWorld.World.Meshing;

namespace MineWorld.Client.Rendering;

public interface IRenderBackend
{
    void BeginFrame(CameraState camera);
    void Submit(VoxelMesh mesh);
    void EndFrame();
}
