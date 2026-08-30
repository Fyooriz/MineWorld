using MineWorld.Core.World;

namespace MineWorld.Tests;

public sealed class VoxelMesherTests
{
    [Fact]
    public void SingleBlockProducesSixVisibleFaces()
    {
        var section = new ChunkSection(0);
        section.SetBlock(8, 8, 8, new BlockId(1));

        var faces = VoxelMesher.Build(section);

        Assert.Equal(6, faces.Count);
    }

    [Fact]
    public void AdjacentBlocksDoNotProduceInternalFace()
    {
        var section = new ChunkSection(0);
        var stone = new BlockId(1);
        section.SetBlock(8, 8, 8, stone);
        section.SetBlock(9, 8, 8, stone);

        var faces = VoxelMesher.Build(section);

        Assert.Equal(10, faces.Count);
    }

    [Fact]
    public void AirProducesNoFaces()
    {
        var section = new ChunkSection(0);

        Assert.Empty(VoxelMesher.Build(section));
    }
}
