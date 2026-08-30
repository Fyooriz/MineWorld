using MineWorld.World.Chunks;
using MineWorld.World.Meshing;
using Xunit;

namespace MineWorld.Tests.World.Meshing;

public sealed class ChunkMeshBuilderTests
{
    [Fact]
    public void EmptyChunkBuildsWithoutGeometry()
    {
        var chunk = new ChunkBlockStorage(16, 16, 16);
        var builder = new ChunkMeshBuilder();

        var mesh = builder.Build(chunk, (_, _, _) => new MineWorld.Blocks.Runtime.BlockState("mineworld:air"));

        Assert.Empty(mesh.Vertices);
        Assert.Empty(mesh.Indices);
    }
}
