using MineWorld.World.Meshing;
using Xunit;

namespace MineWorld.Tests;

public sealed class ChunkGpuMeshCacheTests
{
    [Fact]
    public void UploadCreatesAndReusesResource()
    {
        using var cache = new ChunkGpuMeshCache();
        var mesh = new VoxelMesh();

        var first = cache.Upload(2, 3, mesh);
        var second = cache.Upload(2, 3, mesh);

        Assert.Same(first, second);
        Assert.True(first.IsUploaded);
        Assert.Equal(2, first.Generation);
    }

    [Fact]
    public void RemoveDisposesResource()
    {
        using var cache = new ChunkGpuMeshCache();
        var resource = cache.Upload(0, 0, new VoxelMesh());

        Assert.True(cache.Remove(0, 0));
        Assert.False(resource.IsUploaded);
        Assert.False(cache.Remove(0, 0));
    }

    [Fact]
    public void ClearDisposesAllResources()
    {
        using var cache = new ChunkGpuMeshCache();
        var first = cache.Upload(0, 0, new VoxelMesh());
        var second = cache.Upload(1, 0, new VoxelMesh());

        cache.Clear();

        Assert.False(first.IsUploaded);
        Assert.False(second.IsUploaded);
        Assert.Empty(cache.Resources);
    }
}
