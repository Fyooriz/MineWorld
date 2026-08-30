using MineWorld.Core.World;

namespace MineWorld.Tests;

public sealed class ChunkSectionCollectionTests
{
    [Fact]
    public void MissingSectionReadsAsAir()
    {
        var collection = new ChunkSectionCollection();

        Assert.Equal(BlockId.Air, collection.GetBlock(32, 0, 0));
    }

    [Fact]
    public void WorldYMapsIntoCorrectSection()
    {
        var collection = new ChunkSectionCollection();
        var stone = new BlockId(1);

        collection.SetBlock(17, 2, 3, stone);

        Assert.Equal(stone, collection.GetBlock(17, 2, 3));
        Assert.True(collection.TryGet(1, out var section));
        Assert.NotNull(section);
        Assert.Equal(stone, section!.GetBlock(2, 1, 3));
    }

    [Fact]
    public void NegativeWorldYUsesFloorDivisionSemantics()
    {
        var collection = new ChunkSectionCollection();
        var stone = new BlockId(1);

        collection.SetBlock(-1, 2, 3, stone);

        Assert.Equal(stone, collection.GetBlock(-1, 2, 3));
        Assert.True(collection.TryGet(-1, out var section));
        Assert.NotNull(section);
        Assert.Equal(stone, section!.GetBlock(2, 15, 3));
    }
}
