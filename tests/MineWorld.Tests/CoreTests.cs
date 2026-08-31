using System.Text.Json;
using MineWorld.Core.Entities;
using MineWorld.Core.Inventory;
using MineWorld.Core.World;
using MineWorld.Playable;

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

    [Fact]
    public void EntityPersistenceRoundTripsThroughRuntimeContainer()
    {
        var runtime = new EntityRuntime();
        var original = new TestEntity(new EntityId("test:persistent"));
        runtime.Add(original);
        runtime.Tick(new EntityTickContext(10, 0.05));

        var snapshot = EntityPersistence.Capture(original);
        var json = JsonSerializer.Serialize(new[] { snapshot });
        var loadedSnapshots = EntityPersistence.DeserializeEntities(json);
        var loadedEntities = EntityRehydrator.RehydrateAll(
            loadedSnapshots,
            saved => new TestEntity(new EntityId(saved.Id), saved.Kind, new EntityPosition(saved.X, saved.Y, saved.Z)));

        var rehydratedRuntime = new EntityRuntime();
        foreach (var entity in loadedEntities)
            rehydratedRuntime.Add(entity);

        Assert.Equal(1, rehydratedRuntime.Count);
        Assert.True(rehydratedRuntime.TryGet(original.Id, out var restored));
        Assert.NotSame(original, restored);
        Assert.Equal(original.Id, restored.Id);
        Assert.Equal(original.Kind, restored.Kind);
        Assert.Equal(original.Position, restored.Position);

        rehydratedRuntime.Tick(new EntityTickContext(11, 0.05));

        var restoredTestEntity = Assert.IsType<TestEntity>(restored);
        Assert.Equal(1, restoredTestEntity.TickCount);
        Assert.Equal(11, restoredTestEntity.LastContext.Tick);
        Assert.Equal(0.05, restoredTestEntity.LastContext.DeltaSeconds, precision: 12);
    }

    private sealed class TestEntity(
        EntityId id,
        EntityKind kind = EntityKind.Passive,
        EntityPosition? position = null)
        : EntityBase(id, kind, position ?? new EntityPosition(1, 2, 3))
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
