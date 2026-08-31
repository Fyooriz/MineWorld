using System.Text.Json;
using MineWorld.Core.Entities;

namespace MineWorld.Playable.Tests;

public sealed class EntityPersistenceRuntimeE2ETests
{
    [Fact]
    public void EntityPersistenceRoundTripsThroughRuntimeContainer()
    {
        var runtime = new EntityRuntime();
        var original = new TestEntity(new EntityId("test:persistent"));
        runtime.Add(original);

        runtime.Tick(new EntityTickContext(10, 0.05));
        Assert.Equal(1, original.TickCount);
        Assert.Equal(10, original.LastContext.Tick);

        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var path = Path.Combine(Path.GetTempPath(), $"mineworld-entity-e2e-{Guid.NewGuid():N}.json");

        try
        {
            WorldPersistence.Save(world, path, new[] { original });
            var loaded = WorldPersistence.LoadState(path, renderDistance: 1);
            var loadedJson = JsonSerializer.Serialize(loaded.Entities);
            var snapshots = EntityPersistence.DeserializeEntities(loadedJson);

            var rehydrated = EntityRehydrator.RehydrateAll(
                snapshots,
                saved => new TestEntity(
                    new EntityId(saved.Id),
                    saved.Kind,
                    new EntityPosition(saved.X, saved.Y, saved.Z)));

            var rehydratedRuntime = new EntityRuntime();
            foreach (var entity in rehydrated)
                rehydratedRuntime.Add(entity);

            Assert.Equal(1, rehydratedRuntime.Count);
            Assert.True(rehydratedRuntime.TryGet(original.Id, out var restored));
            Assert.NotSame(original, restored);
            Assert.Equal(original.Id, restored.Id);
            Assert.Equal(original.Kind, restored.Kind);
            Assert.Equal(original.Position, restored.Position);

            rehydratedRuntime.Tick(new EntityTickContext(11, 0.05));

            var restoredEntity = Assert.IsType<TestEntity>(restored);
            Assert.Equal(1, restoredEntity.TickCount);
            Assert.Equal(11, restoredEntity.LastContext.Tick);
            Assert.Equal(0.05, restoredEntity.LastContext.DeltaSeconds, precision: 12);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
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
