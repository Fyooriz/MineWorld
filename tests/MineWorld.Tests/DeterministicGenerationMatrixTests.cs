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

    [Fact]
    public void GeneratorVersionParticipatesInDeterministicInput()
    {
        var generator = new BasicWorldGenerator();
        var first = generator.GenerateBlock(123, 40, -456, 99L, new WorldGeneratorConfig(1));
        var second = generator.GenerateBlock(123, 40, -456, 99L, new WorldGeneratorConfig(2));

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void ChunkBoundaryCoordinatesRemainStableAcrossRepeatedEvaluation()
    {
        var generator = new BasicWorldGenerator();
        var config = new WorldGeneratorConfig(3);
        var boundary = new[] { 15, 16, 31, 32, -16, -15, -32, -31 };

        foreach (var x in boundary)
        foreach (var z in boundary)
        {
            var first = generator.GenerateBlock(x, 32, z, 42L, config);
            var second = generator.GenerateBlock(x, 32, z, 42L, config);
            Assert.Equal(first, second);
        }
    }

    [Fact]
    public void DeterminismDoesNotDependOnEvaluationOrder()
    {
        var generator = new BasicWorldGenerator();
        var config = new WorldGeneratorConfig(5);
        var coordinates = new[]
        {
            (-32, -32), (-16, 0), (-1, -1), (0, 0), (1, 1), (16, 0), (32, 32)
        };

        var forward = coordinates
            .Select(c => generator.GenerateBlock(c.Item1, 36, c.Item2, 77L, config))
            .ToArray();
        var reverse = coordinates
            .Reverse()
            .Select(c => generator.GenerateBlock(c.Item1, 36, c.Item2, 77L, config))
            .Reverse()
            .ToArray();

        Assert.Equal(forward, reverse);
    }
}
