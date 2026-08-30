using System;
using MineWorld.Client.Player;
using MineWorld.World.Chunks;
using MineWorld.World.Meshing;

namespace MineWorld.Client.Rendering;

/// <summary>Coordinates the P0 path: player transform -> camera -> visible chunk meshes -> renderer.</summary>
public sealed class WorldRenderCoordinator
{
    private readonly IRenderBackend _renderer;
    private readonly PlayerCameraBridge _cameraBridge;
    private readonly ChunkMeshBuilder _meshBuilder;

    public WorldRenderCoordinator(
        IRenderBackend renderer,
        PlayerCameraBridge cameraBridge,
        ChunkMeshBuilder meshBuilder)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _cameraBridge = cameraBridge ?? throw new ArgumentNullException(nameof(cameraBridge));
        _meshBuilder = meshBuilder ?? throw new ArgumentNullException(nameof(meshBuilder));
    }

    public void RenderChunk(
        ChunkBlockStorage chunk,
        Func<int, int, int, MineWorld.Blocks.Runtime.BlockState> sample,
        float fieldOfView,
        float aspectRatio)
    {
        var camera = _cameraBridge.BuildCamera(fieldOfView, aspectRatio);
        var mesh = _meshBuilder.Build(chunk, sample);

        _renderer.BeginFrame(camera);
        _renderer.Submit(mesh);
        _renderer.EndFrame();
    }
}
