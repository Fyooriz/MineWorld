using MineWorld.Core.World;

namespace MineWorld.Tests;

public sealed class BlockRegistryTests
{
    [Fact]
    public void DefaultRegistryContainsAirAndCoreBlocks()
    {
        var registry = BlockRegistry.CreateDefault();

        Assert.Equal("air", registry.Get(new BlockId(0)).Name);
        Assert.Equal("stone", registry.Get(new BlockId(1)).Name);
        Assert.Equal("dirt", registry.Get(new BlockId(2)).Name);
        Assert.Equal("grass", registry.Get(new BlockId(3)).Name);
    }

    [Fact]
    public void DuplicateIdsAreRejected()
    {
        var registry = new BlockRegistry();
        registry.Register(new BlockDefinition(new BlockId(1), "stone"));

        Assert.Throws<InvalidOperationException>(() =>
            registry.Register(new BlockDefinition(new BlockId(1), "duplicate")));
    }
}
