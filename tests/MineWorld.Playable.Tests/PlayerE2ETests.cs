using System.Numerics;
using System.Text.Json;
using MineWorld.Core.Entities;
using MineWorld.Core.Inventory;
using MineWorld.Playable;
using Raylib_cs;

namespace MineWorld.Playable.Tests;

public sealed class PlayerE2ETests
{
    [Fact]
    public void FullPlayerActionLayerRoundTripsGameplayState()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var player = new PlayerController(world, initialLookDirection: -Vector3.UnitY);
        player.State.Name = "P0-E2E";
        player.State.Health = 17.5f;
        var input = new InputState();
        var entities = new EntityRuntime();
        var entity = new TestEntity(new EntityId("test:player-e2e"), new EntityPosition(1, 2, 3));
        entities.Add(entity);
        player.State.Inventory.TryAdd(new ItemStack("core:grass", 1));
        var surfaceY = world.GetSurfaceHeight(0, 0);
        var path = Path.Combine(Path.GetTempPath(), $"mineworld-player-e2e-{Guid.NewGuid():N}.json");

        try
        {
            // Actual input → mining → inventory mutation.
            input.SetFrame(new InputFrame(Vector2.Zero, false, false, false, false, false, true, false, false, false));
            player.Update(1f / 60f, input, Vector2.Zero);
            Assert.Equal(VoxelWorld.Air, world.GetBlock(0, surfaceY, 0));
            Assert.Equal(2, player.State.Inventory.Count("core:grass"));

            // Actual input → action layer → crafting → inventory mutation.
            input.SetFrame(new InputFrame(Vector2.Zero, false, false, false, false, false, false, false, true, false));
            player.Update(1f / 60f, input, Vector2.Zero);
            Assert.Equal(1, player.State.Inventory.Count("core:grass"));
            Assert.Equal(1, player.State.Inventory.Count("core:dirt"));

            // Actual input → placement → inventory/world mutation.
            input.SetFrame(new InputFrame(Vector2.Zero, false, false, false, false, false, false, true, false, false));
            player.Update(1f / 60f, input, Vector2.Zero);
            Assert.Equal(VoxelWorld.Dirt, world.GetBlock(0, surfaceY, 0));
            Assert.Equal(0, player.State.Inventory.Count("core:dirt"));

            // EntityRuntime is inside the same simulation boundary.
            entities.Tick(new EntityTickContext(1, 1f / 60f));
            Assert.Equal(1, entity.TickCount);

            // Persist actual world + entity runtime + PlayerState.
            WorldPersistence.Save(world, path, entities.Snapshot(), player.State);
            var loaded = WorldPersistence.LoadState(path, renderDistance: 1);
            Assert.Equal(world.Seed, loaded.World.Seed);
            Assert.Equal(VoxelWorld.Dirt, loaded.World.GetBlock(0, surfaceY, 0));
            Assert.NotNull(loaded.Player);
            Assert.Equal(player.State.Id, loaded.Player!.Id);
            Assert.Equal("P0-E2E", loaded.Player.Name);
            Assert.Equal(17.5f, loaded.Player.Health);

            var restoredPlayer = PlayerPersistence.Restore(loaded.Player);
            Assert.Equal(player.State.Id, restoredPlayer.Id);
            Assert.Equal(player.State.Name, restoredPlayer.Name);
            Assert.Equal(player.State.Health, restoredPlayer.Health);
            Assert.Equal(1, restoredPlayer.Inventory.Count("core:grass"));
            Assert.Equal(0, restoredPlayer.Inventory.Count("core:dirt"));

            var snapshots = EntityPersistence.DeserializeEntities(JsonSerializer.Serialize(loaded.Entities));
            var rehydrated = EntityRehydrator.RehydrateAll(
                snapshots,
                saved => new TestEntity(
                    new EntityId(saved.Id),
                    new EntityPosition(saved.X, saved.Y, saved.Z)));

            var restoredRuntime = new EntityRuntime();
            foreach (var restored in rehydrated)
                restoredRuntime.Add(restored);

            Assert.True(restoredRuntime.TryGet(entity.Id, out var restoredEntity));
            Assert.NotSame(entity, restoredEntity);
            Assert.Equal(entity.Id, restoredEntity.Id);
            Assert.Equal(entity.Kind, restoredEntity.Kind);
            Assert.Equal(entity.Position, restoredEntity.Position);

            restoredRuntime.Tick(new EntityTickContext(2, 1f / 60f));
            Assert.Equal(1, Assert.IsType<TestEntity>(restoredEntity).TickCount);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void GameLoopFixedStepTicksEntityRuntime()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var player = new PlayerController(world);
        var input = new InputState();
        var entities = new EntityRuntime();
        var entity = new TestEntity(new EntityId("test:loop"));
        entities.Add(entity);
        using var renderer = new FakeRenderer();
        var loop = new GameLoop(renderer, input, world, player, Path.Combine(Path.GetTempPath(), "mineworld-e2e-no-write.json"), entities);

        loop.StepSimulation(1f / 60f, Vector2.Zero);

        Assert.Equal(1, entity.TickCount);
        Assert.Equal(1, entity.LastContext.Tick);
        Assert.Equal(1f / 60f, entity.LastContext.DeltaSeconds, precision: 12);
    }

    private sealed class TestEntity(EntityId id, EntityPosition? position = null)
        : EntityBase(id, EntityKind.Passive, position ?? new EntityPosition(1, 2, 3))
    {
        public int TickCount { get; private set; }
        public EntityTickContext LastContext { get; private set; } = new(0, 0);

        public override void Tick(EntityTickContext context)
        {
            TickCount++;
            LastContext = context;
        }
    }

    private sealed class FakeRenderer : IRenderer
    {
        public bool ShouldClose => false;
        public int Width => 1;
        public int Height => 1;
        public void BeginFrame(Vector3 position, Vector3 target) { }
        public void RenderWorld(VoxelWorld world) { }
        public void DrawHud(PlayerController player, VoxelWorld world) { }
        public void EndFrame() { }
        public void Dispose() { }
    }
}
