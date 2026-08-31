namespace MineWorld.Playable.Tests;

public sealed class P1PersistenceSafeStreamingTests
{
    [Fact]
    public void MultipleOverridesSurviveEvictionAndReloadAtChunkBoundaries()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var coordinates = new[]
        {
            (X: -17, Y: 20, Z: -17),
            (X: -16, Y: 21, Z: -16),
            (X: -1, Y: 22, Z: 0),
            (X: 0, Y: 23, Z: 0),
            (X: 15, Y: 24, Z: 15),
            (X: 16, Y: 25, Z: 16)
        };

        foreach (var coordinate in coordinates)
            world.SetBlock(coordinate.X, coordinate.Y, coordinate.Z, VoxelWorld.Air);

        Assert.Equal(coordinates.Length, world.BlockOverrides.Count);

        world.StreamAround(1600.5f, -1600.5f);
        world.StreamAround(-0.5f, -0.5f);

        foreach (var coordinate in coordinates)
            Assert.Equal(VoxelWorld.Air, world.GetBlock(coordinate.X, coordinate.Y, coordinate.Z));
    }

    [Fact]
    public void PersistedOverridesSurviveFullEvictionAndDiskReload()
    {
        var world = new VoxelWorld(seed: 2468, renderDistance: 1);
        var changes = new[]
        {
            (X: -33, Y: 18, Z: 7, Block: VoxelWorld.Air),
            (X: 31, Y: 19, Z: -32, Block: VoxelWorld.Dirt),
            (X: 16, Y: 20, Z: 16, Block: VoxelWorld.Stone)
        };

        foreach (var change in changes)
            world.SetBlock(change.X, change.Y, change.Z, change.Block);

        world.StreamAround(2048.5f, 2048.5f);

        var path = Path.Combine(Path.GetTempPath(), $"mineworld-p1-streaming-{Guid.NewGuid():N}.json");
        try
        {
            WorldPersistence.Save(world, path);
            var loaded = WorldPersistence.Load(path, renderDistance: 1);

            foreach (var change in changes)
                Assert.Equal(change.Block, loaded.GetBlock(change.X, change.Y, change.Z));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void UnmodifiedChunkRegeneratesToTheSameBaselineAfterEviction()
    {
        var original = new VoxelWorld(seed: 97531, renderDistance: 1);
        var reference = new VoxelWorld(seed: 97531, renderDistance: 1);
        var samples = new[]
        {
            (X: -31, Y: 0, Z: -17),
            (X: -16, Y: 16, Z: -16),
            (X: -1, Y: 12, Z: 15),
            (X: 15, Y: 31, Z: 15),
            (X: 16, Y: 8, Z: 16),
            (X: 33, Y: 20, Z: -7)
        };

        var expected = samples
            .Select(sample => (sample, Block: reference.GetBlock(sample.X, sample.Y, sample.Z)))
            .ToArray();

        original.StreamAround(4096.5f, -4096.5f);
        original.StreamAround(0.5f, 0.5f);

        foreach (var item in expected)
            Assert.Equal(item.Block, original.GetBlock(item.sample.X, item.sample.Y, item.sample.Z));

        Assert.Empty(original.BlockOverrides);
    }
}
