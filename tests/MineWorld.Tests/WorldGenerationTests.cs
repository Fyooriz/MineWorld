using MineWorld.Core.World;

namespace MineWorld.Tests;

public sealed class WorldGenerationTests
{
    [Fact]
    public void GenerationIsDeterministic()
    {
        var generator = new BasicWorldGenerator();
        var config = new WorldGeneratorConfig(1);

        var a = generator.GenerateBlock(123, 20, -456, 987654321L, config);
        var b = generator.GenerateBlock(123, 20, -456, 987654321L, config);

        Assert.Equal(a, b);
    }

    [Fact]
    public void DifferentSeedsCanProduceDifferentTerrain()
    {
        var generator = new BasicWorldGenerator();
        var config = new WorldGeneratorConfig(1);

        var first = generator.GenerateBlock(123, 40, -456, 1L, config);
        var second = generator.GenerateBlock(123, 40, -456, 2L, config);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void GenerationUsesFloorCompatibleWorldCoordinates()
    {
        var generator = new BasicWorldGenerator();
        var config = new WorldGeneratorConfig(1);

        var negative = generator.GenerateBlock(-1, 10, -1, 42L, config);

        Assert.NotEqual(BlockId.Air, negative);
    }
}
