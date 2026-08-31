using System.Numerics;
using MineWorld.Core.Entities;
using MineWorld.Playable;
using Raylib_cs;

namespace MineWorld.Playable.Tests;

public sealed class PlayerE2ETests
{
    [Fact]
    public void DeterministicInputDrivesPlayerMiningAndPlacement()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var player = new PlayerController(world);
        var input = new InputState();
        var surfaceY = world.GetSurfaceHeight(0, 0);

        input.SetFrame(new InputFrame(
            MouseDelta: new Vector2(0, 600),
            Forward: false,
            Backward: false,
            Left: false,
            Right: false,
            JumpPressed: false,
            MinePressed: false,
            PlacePressed: false,
            SavePressed: false));
        player.Update(1f / 60f, input, input.ConsumeMouseDelta());

        input.SetFrame(new InputFrame(
            MouseDelta: Vector2.Zero,
            Forward: false,
            Backward: false,
            Left: false,
            Right: false,
            JumpPressed: false,
            MinePressed: true,
            PlacePressed: false,
            SavePressed: false));
        player.Update(1f / 60f, input, Vector2.Zero);

        Assert.Equal(VoxelWorld.Air, world.GetBlock(0, surfaceY, 0));
        Assert.Equal(1, player.State.Inventory.Count("core:grass"));

        input.SetFrame(new InputFrame(
            MouseDelta: Vector2.Zero,
            Forward: false,
            Backward: false,
            Left: false,
            Right: false,
            JumpPressed: false,
            MinePressed: false,
            PlacePressed: true,
            SavePressed: false));
        player.Update(1f / 60f, input, Vector2.Zero);

        Assert.Equal(VoxelWorld.Grass, world.GetBlock(0, surfaceY, 0));
        Assert.Equal(0, player.State.Inventory.Count("core:grass"));
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

    private sealed class TestEntity(EntityId id)
        : EntityBase(id, EntityKind.Passive, new EntityPosition(1, 2, 3))
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
