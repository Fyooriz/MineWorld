using MineWorld.Core.Inventory;
using MineWorld.Core.World;

namespace MineWorld.Tests;

public sealed class CoreTests
{
    [Fact]
    public void NewChunkIsFilledWithAir()
    {
        var chunk = new Chunk(new ChunkCoordinate(0, 0, 0));

        Assert.Equal(BlockId.Air, chunk.GetBlock(0, 0, 0));
        Assert.Equal(BlockId.Air, chunk.GetBlock(15, 63, 15));
    }

    [Fact]
    public void ChunkStoresAndReturnsBlocks()
    {
        var chunk = new Chunk(new ChunkCoordinate(2, -1, 4));
        var stone = BlockId.Stone;

        chunk.SetBlock(3, 12, 7, stone);

        Assert.Equal(stone, chunk.GetBlock(3, 12, 7));
    }

    [Fact]
    public void WorldSeedMixIsDeterministic()
    {
        var coordinate = new ChunkCoordinate(-12, 0, 31);

        Assert.Equal(WorldSeed.Mix(1234UL, coordinate), WorldSeed.Mix(1234UL, coordinate));
        Assert.NotEqual(WorldSeed.Mix(1234UL, coordinate), WorldSeed.Mix(1235UL, coordinate));
    }

    [Fact]
    public void InventoryAddsAndRemovesStacks()
    {
        var inventory = new Inventory(4);

        Assert.True(inventory.TryAdd(new ItemStack("core:dirt", 8)));
        Assert.True(inventory.TryAdd(new ItemStack("core:dirt", 2)));
        Assert.True(inventory.TryRemove("core:dirt", 5));
        Assert.Equal(5, inventory.GetSlot(0).Count);
    }
}
