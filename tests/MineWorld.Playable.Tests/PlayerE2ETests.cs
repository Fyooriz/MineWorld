using System.Numerics;
using MineWorld.Core.Crafting;
using MineWorld.Core.Entities;
using MineWorld.Playable;
using Raylib_cs;

namespace MineWorld.Playable.Tests;

public sealed class PlayerE2ETests
{
    [Fact]
    public void DeterministicInputDrivesPlayerMiningCraftingAndPlacement()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var player = new PlayerController(world, initialLookDirection: -Vector3.UnitY);
        var input = new InputState();
        var surfaceY = world.GetSurfaceHeight(0, 0);

        input.SetFrame(new InputFrame(
            Vector2.Zero, false, false, false, false, false, true, false, false));
        player.Update(1f / 60f, input, Vector2.Zero);

        Assert.Equal(VoxelWorld.Air, world.GetBlock(0, surfaceY, 0));
        Assert.Equal(1, player.State.Inventory.Count("core:grass"));

        var recipe = new RecipeDefinition(
            "p0:dirt-from-grass",
            new[] { new ItemStack("core:grass", 1) },
            new ItemStack("core:dirt", 1));
        Assert.True(new CraftingService().TryCraft(player.State.Inventory, recipe));
        Assert.Equal(0, player.State.Inventory.Count("core:grass"));
        Assert.Equal(1, player.State.Inventory.Count("core:dirt"));

        input.SetFrame(new InputFrame(
            Vector2.Zero, false, false, false, false, false, false, true, false));
        player.Update(1f / 60f, input, Vector2.Zero);

        Assert.Equal(VoxelWorld.Dirt, world.GetBlock(0, surfaceY, 0));
        Assert.Equal(0, player.State.Inventory.Count("core:dirt"));
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
