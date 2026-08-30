using MineWorld.Core.World;

namespace MineWorld.Tests;

public sealed class ChunkSectionTests
{
    [Fact]
    public void NewSectionIsFilledWithAir()
    {
        var section = new ChunkSection(0);

        Assert.Equal(BlockId.Air, section.GetBlock(0, 0, 0));
        Assert.Equal(BlockId.Air, section.GetBlock(15, 15, 15));
    }

    [Fact]
    public void SectionStoresBlocksAtLocalCoordinates()
    {
        var section = new ChunkSection(3);
        var stone = new BlockId(1);

        section.SetBlock(4, 7, 12, stone);

        Assert.Equal(3, section.SectionY);
        Assert.Equal(stone, section.GetBlock(4, 7, 12));
        Assert.Equal(BlockId.Air, section.GetBlock(4, 7, 11));
    }

    [Fact]
    public void CoordinatesOutsideSectionAreRejected()
    {
        var section = new ChunkSection(0);

        Assert.Throws<ArgumentOutOfRangeException>(() => section.GetBlock(16, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => section.SetBlock(0, -1, 0, new BlockId(1)));
    }
}
