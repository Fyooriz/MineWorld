using System.Numerics;
using Raylib_cs;

namespace MineWorld.Playable;

/// <summary>Coordinates CPU chunk visibility decisions with persistent GPU chunk meshes.</summary>
internal sealed class ChunkRenderCoordinator : IDisposable
{
    private readonly GpuChunkMeshUploader _gpu;
    private readonly ChunkVisibilityPolicy _visibility;
    private readonly int _chunkSize;

    public ChunkRenderCoordinator(GpuChunkMeshUploader gpu, int chunkSize, int renderDistanceChunks = 8)
    {
        _gpu = gpu;
        _chunkSize = Math.Max(1, chunkSize);
        _visibility = new ChunkVisibilityPolicy(renderDistanceChunks);
    }

    public int ResidentCount => _gpu.ResidentCount;
    public int VisibleCount => _gpu.VisibleCount;

    public void UpdateVisibility(IEnumerable<ChunkKey> residentChunks, Vector3 cameraPosition)
    {
        foreach (var key in residentChunks)
            _gpu.SetVisible(key, _visibility.IsVisible(key, cameraPosition, _chunkSize));
    }

    public void UpdateVisibility(
        IEnumerable<ChunkKey> residentChunks,
        Vector3 cameraPosition,
        Vector3 cameraForward,
        float horizontalFovDegrees)
    {
        foreach (var key in residentChunks)
        {
            var visible = _visibility.IsVisible(
                key,
                cameraPosition,
                cameraForward,
                _chunkSize,
                horizontalFovDegrees);
            _gpu.SetVisible(key, visible);
        }
    }

    public void Draw()
    {
        _gpu.DrawVisible();
    }

    public void Remove(ChunkKey key) => _gpu.Remove(key);

    public void Dispose() => _gpu.Dispose();
}
