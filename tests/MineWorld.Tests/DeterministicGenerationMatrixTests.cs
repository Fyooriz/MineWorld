using MineWorld.Core.World;

namespace MineWorld.Tests;

public sealed class DeterministicGenerationMatrixTests
{
    public static IEnumerable<object[]> Coordinates()
    {
        yield return new object[] { 0, 0, 0 };
        yield return new object[] { 1, 0, -1 };
        yield return new object[] { 15, 0, 15 };
        yield return new object[] { 16, 0, 16 };
        yield return new object[] { -16, 0, -16 };
        yield return new object[] { -17, 0, 17 };
        yield return new object[] { 31, 0, -32 };
        yield return new object[] { -33, 0, 34 };
    }

    [Theory]
    [MemberData(nameof(Coordinates))]
    public void SameInputsAlwaysProduceSameBlock(int worldX, int worldY, int worldZ)
    {
        var generator = new BasicWorldGenerator();
        var config = new WorldGeneratorConfig(7);

        var first = generator.GenerateBlock(worldX, worldY, worldZ, 123456789L, config);
        var second = generator.GenerateBlock(worldX, worldY, worldZ, 123456789L, config);

        Assert.Equal(first, second);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, -1)]
    [InlineData(-1, 1)]
    [InlineData(16, 16)]
    [InlineData(-16, -16)]
    public void FullChunkSnapshotIsStableForRepeatedEvaluation(int chunkX, int chunkZ)
    {
        var generator = new BasicWorldGenerator();
        var config = new WorldGeneratorConfig(7);

        var first = SnapshotChunk(generator, chunkX, chunkZ, 123456789L, config);
        var second = SnapshotChunk(generator, chunkX, chunkZ, 123456789L, config);

        Assert.Equal(first, second);
        Assert.Equal(16 * 64 * 16, first.Length);
    }

    [Fact]
    public void GeneratorVersionParticipatesInFullChunkSnapshot()
    {
        var generator = new BasicWorldGenerator();
        const long seed = 99L;

        var version1 = SnapshotChunk(generator, -3, 7, seed, new WorldGeneratorConfig(1));
        var version2 = SnapshotChunk(generator, -3, 7, seed, new WorldGeneratorConfig(2));

        Assert.Equal(version1.Length, version2.Length);
        Assert.False(version1.SequenceEqual(version2));
    }

    [Theory]
    [InlineData(15, 16)]
    [InlineData(16, 16)]
    [InlineData(-17, -16)]
    [InlineData(-16, -17)]
    [InlineData(31, -32)]
    [InlineData(-33, 32)]
    public void FullChunkBoundaryCoordinatesRemainStable(int chunkX, int chunkZ)
    {
        var generator = new BasicWorldGenerator();
        var config = new WorldGeneratorConfig(3);
        var seed = 42L;

        var first = SnapshotChunk(generator, chunkX, chunkZ, seed, config);
        var second = SnapshotChunk(generator, chunkX, chunkZ, seed, config);

        Assert.Equal(first, second);
    }

    [Fact]
    public void FullChunkDeterminismDoesNotDependOnEvaluationOrder()
    {
        var generator = new BasicWorldGenerator();
        var config = new WorldGeneratorConfig(5);
        var seed = 77L;
        var coordinates = new[]
        {
            (X: -2, Z: -2),
            (X: -1, Z: 0),
            (X: 0, Z: 0),
            (X: 1, Z: 1),
            (X: 2, Z: 2)
        };

        var forward = coordinates
            .Select(c => SnapshotChunk(generator, c.X, c.Z, seed, config))
            .ToArray();
        var reverse = coordinates
            .Reverse()
            .Select(c => SnapshotChunk(generator, c.X, c.Z, seed, config))
            .Reverse()
            .ToArray();

        Assert.Equal(forward.Length, reverse.Length);
        for (var i = 0; i < forward.Length; i++)
            Assert.Equal(forward[i], reverse[i]);
    }

    private static byte[] SnapshotChunk(
        IWorldGenerator generator,
        int chunkX,
        int chunkZ,
        long seed,
        WorldGeneratorConfig config)
    {
        var snapshot = new byte[16 * 64 * 16];
        var index = 0;

        for (var y = 0; y < 64; y++)
        for (var localZ = 0; localZ < 16; localZ++)
        for (var localX = 0; localX < 16; localX++)
        {
            var worldX = chunkX * 16 + localX;
            var worldZ = chunkZ * 16 + localZ;
            snapshot[index++] = generator.GenerateBlock(worldX, y, worldZ, seed, config).Value;
        }

        return snapshot;
    }
}
