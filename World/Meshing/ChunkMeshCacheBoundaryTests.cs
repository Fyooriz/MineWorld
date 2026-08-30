using Xunit;

namespace MineWorld.World.Meshing;

public sealed class ChunkMeshCacheBoundaryTests
{
    [Fact]
    public void BoundaryEdit_MarksAdjacentChunks()
    {
        var cache = new ChunkMeshCache();

        cache.MarkBlockChanged(10, 20, 0, 0, 16, 16);

        Assert.True(cache.IsDirty(10, 20));
        Assert.True(cache.IsDirty(9, 20));
        Assert.True(cache.IsDirty(10, 19));
        Assert.False(cache.IsDirty(11, 20));
        Assert.False(cache.IsDirty(10, 21));
    }

    [Fact]
    public void OppositeBoundaryEdit_MarksPositiveNeighbors()
    {
        var cache = new ChunkMeshCache();

        cache.MarkBlockChanged(-4, 7, 15, 15, 16, 16);

        Assert.True(cache.IsDirty(-4, 7));
        Assert.True(cache.IsDirty(-3, 7));
        Assert.True(cache.IsDirty(-4, 8));
        Assert.False(cache.IsDirty(-5, 7));
        Assert.False(cache.IsDirty(-4, 6));
    }
}
