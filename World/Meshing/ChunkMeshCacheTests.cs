using System.Collections.Generic;
using MineWorld.Blocks.Runtime;
using Xunit;

namespace MineWorld.World.Meshing;

public sealed class ChunkMeshCacheTests
{
    [Fact]
    public void MarkDirty_TracksChunkUntilRebuild()
    {
        var cache = new ChunkMeshCache();
        cache.MarkDirty(2, -3);

        Assert.True(cache.IsDirty(2, -3));
    }

    [Fact]
    public void Remove_ClearsMeshAndDirtyState()
    {
        var cache = new ChunkMeshCache();
        cache.MarkDirty(0, 0);
        cache.Remove(0, 0);

        Assert.False(cache.IsDirty(0, 0));
        Assert.False(cache.Meshes.ContainsKey((0, 0)));
    }
}
