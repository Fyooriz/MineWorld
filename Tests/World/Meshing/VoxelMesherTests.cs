using MineWorld.World.Meshing;
using Xunit;

namespace MineWorld.Tests.World.Meshing;

public sealed class VoxelMesherTests
{
    [Fact]
    public void EmptyVoxelWorldProducesEmptyMesh()
    {
        var chunk = new MineWorld.World.Chunks.ChunkBlockStorage(16, 16, 16);
        var mesher = new VoxelMesher();
        var mesh = mesher.Build(chunk, (_, _, _) => new MineWorld.Blocks.Runtime.BlockState("mineworld:air"));

        Assert.Empty(mesh.Vertices);
        Assert.Empty(mesh.Indices);
    }
}
