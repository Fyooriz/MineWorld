using MineWorld.Core.World;
using MineWorld.Playable;

namespace MineWorld.Playable.Tests;

public sealed class P1WorldChunkTests
{
    [Theory]
    [InlineData(0, 0, 0, 0, 0, 0)]
    [InlineData(15, 15, 0, 0, 15, 15)]
    [InlineData(16, 16, 1, 1, 0, 0)]
    [InlineData(-1, -1, -1, -1, 15, 15)]
    [InlineData(-16, -16, -1, -1, 0, 0)]
    [InlineData(-17, -17, -2, -2, 15, 15)]
    [InlineData(-33, 7, -3, 0, 15, 7)]
    public void WorldToChunkUsesFloorSemantics(
        int worldX,
        int worldZ,
        int expectedChunkX,
        int expectedChunkZ,
        int expectedLocalX,
        int expectedLocalZ)
    {
        var chunk = HorizontalChunkCoordinate.FromWorld(worldX, worldZ);

        Assert.Equal(expectedChunkX, chunk.X);
        Assert.Equal(expectedChunkZ, chunk.Z);

        var local = chunk.ToLocal(worldX, worldZ);
        Assert.Equal(expectedLocalX, local.X);
        Assert.Equal(expectedLocalZ, local.Z);
    }

    [Fact]
    public void ChunkLocalRoundTripPreservesWorldCoordinate()
    {
        var chunk = new HorizontalChunkCoordinate(-7, 4);
        var world = chunk.ToWorld(3, 12);

        var recovered = HorizontalChunkCoordinate.FromWorld(world.X, world.Z);
        var local = recovered.ToLocal(world.X, world.Z);

        Assert.Equal(chunk, recovered);
        Assert.Equal(3, local.X);
        Assert.Equal(12, local.Z);
    }

    [Fact]
    public void SameSeedProducesDeterministicTerrainAcrossChunkBoundaries()
    {
        var left = new VoxelWorld(seed: 12345, renderDistance: 1);
        var right = new VoxelWorld(seed: 12345, renderDistance: 1);
        var coordinates = new[]
        {
            (-17, -17), (-16, -1), (-1, 0), (0, 0), (15, 15), (16, 16), (31, -33), (48, 7)
        };

        foreach (var (x, z) in coordinates)
        {
            Assert.Equal(left.GetSurfaceHeight(x, z), right.GetSurfaceHeight(x, z));
            foreach (var y in new[] { 0, 1, 4, 15, 31, 63 })
                Assert.Equal(left.GetBlock(x, y, z), right.GetBlock(x, y, z));
        }
    }

    [Fact]
    public void StreamingKeepsLoadedChunkSetBoundedAndDropsDistantMeshEntries()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        Assert.Equal(9, world.LoadedChunkCount);

        world.StreamAround(16 * 10 + 0.5f, 16 * -8 + 0.5f);

        Assert.Equal(9, world.LoadedChunkCount);
        Assert.InRange(world.LoadedChunkCount, 1, (world.RenderDistance * 2 + 1) * (world.RenderDistance * 2 + 1));
        Assert.InRange(world.CachedMeshCount, 0, world.LoadedChunkCount);
    }

    [Fact]
    public void ModifiedDistantChunkSurvivesUnloadAndReloadThroughOverrideState()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        const int x = 40;
        const int z = -25;
        var y = world.GetSurfaceHeight(x, z);
        var generated = world.GetBlock(x, y, z);

        world.SetBlock(x, y, z, VoxelWorld.Air);
        Assert.Equal(VoxelWorld.Air, world.GetBlock(x, y, z));

        world.StreamAround(0.5f, 0.5f);
        world.StreamAround(x + 0.5f, z + 0.5f);

        Assert.Equal(VoxelWorld.Air, world.GetBlock(x, y, z));
        Assert.NotEqual(generated, world.GetBlock(x, y, z));
    }

    [Fact]
    public void ModifiedChunkSurvivesPersistenceAfterStreamingEviction()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        const int x = 40;
        const int z = -25;
        var y = world.GetSurfaceHeight(x, z);
        world.SetBlock(x, y, z, VoxelWorld.Air);

        world.StreamAround(0.5f, 0.5f);
        Assert.Equal(VoxelWorld.Air, world.GetBlock(x, y, z));

        var path = Path.Combine(Path.GetTempPath(), $"mineworld-p1-{Guid.NewGuid():N}.json");
        try
        {
            WorldPersistence.Save(world, path);
            var loaded = WorldPersistence.Load(path, renderDistance: 1);
            Assert.Equal(VoxelWorld.Air, loaded.GetBlock(x, y, z));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
