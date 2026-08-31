using MineWorld.Core.Entities;
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

    [Fact]
    public void EntityRuntimeOwnsAndTicksLiveEntities()
    {
        var runtime = new EntityRuntime();
        var entity = new TestEntity(new EntityId("test:entity"));

        runtime.Add(entity);

        Assert.Equal(1, runtime.Count);
        Assert.True(runtime.TryGet(entity.Id, out var active));
        Assert.Same(entity, active);

        runtime.Tick(new EntityTickContext(42, 1.0 / 60.0));

        Assert.Equal(1, entity.TickCount);
        Assert.Equal(42, entity.LastContext.Tick);
    }

    [Fact]
    public void EntityRuntimeRejectsDuplicateIdsAndSupportsRemoval()
    {
        var runtime = new EntityRuntime();
        var first = new TestEntity(new EntityId("test:duplicate"));
        var second = new TestEntity(new EntityId("test:duplicate"));

        runtime.Add(first);

        Assert.Throws<InvalidOperationException>(() => runtime.Add(second));
        Assert.True(runtime.Remove(first.Id));
        Assert.False(runtime.Remove(first.Id));
        Assert.Equal(0, runtime.Count);
    }

    private sealed class TestEntity(EntityId id) : EntityBase(id, EntityKind.Passive, new EntityPosition(1, 2, 3))
    {
        public int TickCount { get; private set; }
        public EntityTickContext LastContext { get; private set; } = new(0, 0);

        public override void Tick(EntityTickContext context)
        {
            TickCount++;
            LastContext = context;
        }
    }
}
