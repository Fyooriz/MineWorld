using MineWorld.Core.World;

namespace MineWorld.Tests;

public sealed class WorldStateTests
{
    [Fact]
    public void SameSeedAndGeneratorConfigProduceSameBlocks()
    {
        var a = new WorldState(12345);
        var b = new WorldState(12345);

        foreach (var coordinate in new[]
                 {
                     (X: 0, Y: 32, Z: 0),
                     (X: 17, Y: 20, Z: -9),
                     (X: -32, Y: 10, Z: 48)
                 })
        {
            Assert.Equal(a.GetBlock(coordinate.X, coordinate.Y, coordinate.Z),
                         b.GetBlock(coordinate.X, coordinate.Y, coordinate.Z));
        }
    }

    [Fact]
    public void NegativeCoordinatesMapToStableChunkStorage()
    {
        var world = new WorldState(12345);

        world.SetBlock(-1, 10, -1, BlockId.Wood);

        Assert.Equal(BlockId.Wood, world.GetBlock(-1, 10, -1));
        Assert.True(world.TryGetLoadedChunk(-1, -1, out _));
    }

    [Fact]
    public void ModifiedBlockSurvivesChunkUnloadAndReload()
    {
        var world = new WorldState(12345);
        world.SetBlock(0, 20, 0, BlockId.Wood);

        Assert.Equal(BlockId.Wood, world.GetBlock(0, 20, 0));
        Assert.True(world.UnloadChunk(0, 0));
        Assert.False(world.TryGetLoadedChunk(0, 0, out _));

        Assert.Equal(BlockId.Wood, world.GetBlock(0, 20, 0));
        Assert.True(world.TryGetLoadedChunk(0, 0, out _));
    }

    [Fact]
    public void RestoringGeneratedBlockRemovesItsPersistentOverride()
    {
        var world = new WorldState(12345);
        var original = world.GetBlock(0, 20, 0);

        world.SetBlock(0, 20, 0, BlockId.Wood);
        Assert.Single(world.EnumerateOverrides());

        world.SetBlock(0, 20, 0, original);
        Assert.Empty(world.EnumerateOverrides());
    }
}
