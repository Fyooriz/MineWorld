using System.Numerics;
using MineWorld.Core.Crafting;
using MineWorld.Core.Entities;
using MineWorld.Core.Inventory;
using MineWorld.Playable;
using Raylib_cs;

namespace MineWorld.Playable.Tests;

public sealed class P0IntegrationTests
{
    [Fact]
    public void MineCraftPlaceSaveReloadVerticalSlicePreservesState()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var inventory = new Inventory(capacity: 8);
        var x = 0;
        var z = 0;
        var y = world.GetSurfaceHeight(x, z);
        var miningRay = new Ray(new Vector3(0.5f, y + 2f, 0.5f), -Vector3.UnitY);

        Assert.Equal(VoxelWorld.Grass, world.GetBlock(x, y, z));
        Assert.True(world.Mine(miningRay, inventory));
        Assert.Equal(VoxelWorld.Air, world.GetBlock(x, y, z));
        Assert.Equal(1, inventory.Count("core:grass"));

        var recipe = new RecipeDefinition(
            "p0:dirt-from-grass",
            new[] { new ItemStack("core:grass", 1) },
            new ItemStack("core:dirt", 1));
        var crafting = new CraftingService();

        Assert.True(crafting.TryCraft(inventory, recipe));
        Assert.Equal(0, inventory.Count("core:grass"));
        Assert.Equal(1, inventory.Count("core:dirt"));

        var placementRay = new Ray(new Vector3(0.5f, y + 2f, 0.5f), -Vector3.UnitY);
        Assert.True(world.Place(placementRay, inventory));
        Assert.Equal(VoxelWorld.Dirt, world.GetBlock(x, y, z));
        Assert.Equal(0, inventory.Count("core:dirt"));

        var entity = new TestEntity(
            new EntityId("test:entity"),
            EntityKind.Passive,
            new EntityPosition(1, 2, 3));
        entity.Tick(new EntityTickContext(Tick: 42, DeltaSeconds: 1.0 / 60.0));
        Assert.Equal(1, entity.TickCount);
        Assert.Equal(42, entity.LastTick);
        Assert.Equal(new EntityPosition(1, 2, 3), entity.Position);

        var path = Path.Combine(Path.GetTempPath(), $"mineworld-p0-e2e-{Guid.NewGuid():N}.json");
        try
        {
            WorldPersistence.Save(world, path, new[] { entity });
            var reloaded = WorldPersistence.LoadState(path, renderDistance: 1);
            Assert.Equal(VoxelWorld.Dirt, reloaded.World.GetBlock(x, y, z));
            Assert.Equal(world.Seed, reloaded.World.Seed);
            var savedEntity = Assert.Single(reloaded.Entities);
            Assert.Equal(entity.Id.Value, savedEntity.Id);
            Assert.Equal(entity.Kind, savedEntity.Kind);
            Assert.Equal(entity.Position.X, savedEntity.X);
            Assert.Equal(entity.Position.Y, savedEntity.Y);
            Assert.Equal(entity.Position.Z, savedEntity.Z);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void MiningWithFullInventoryLeavesWorldUnchanged()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var inventory = new Inventory(capacity: 1, maxStackSize: 1);
        Assert.True(inventory.TryAdd(new ItemStack("core:stone", 1)));

        var y = world.GetSurfaceHeight(0, 0);
        var ray = new Ray(new Vector3(0.5f, y + 2f, 0.5f), -Vector3.UnitY);
        var original = world.GetBlock(0, y, 0);

        Assert.False(world.Mine(ray, inventory));
        Assert.Equal(original, world.GetBlock(0, y, 0));
        Assert.Equal(1, inventory.Count("core:stone"));
    }

    [Fact]
    public void SaveReloadPreservesModifiedBlock()
    {
        var world = new VoxelWorld(seed: 12345, renderDistance: 1);
        var x = 0;
        var z = 0;
        var y = world.GetSurfaceHeight(x, z);
        world.SetBlock(x, y, z, VoxelWorld.Air);

        var path = Path.Combine(Path.GetTempPath(), $"mineworld-p0-{Guid.NewGuid():N}.json");
        try
        {
            WorldPersistence.Save(world, path);
            var reloaded = WorldPersistence.Load(path, renderDistance: 1);
            Assert.Equal(VoxelWorld.Air, reloaded.GetBlock(x, y, z));
            Assert.Equal(world.Seed, reloaded.Seed);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void EntityLifecycleTicksThroughCoreContract()
    {
        var entity = new TestEntity(
            new EntityId("test:entity"),
            EntityKind.Passive,
            new EntityPosition(1, 2, 3));

        entity.Tick(new EntityTickContext(Tick: 42, DeltaSeconds: 1.0 / 60.0));

        Assert.Equal(1, entity.TickCount);
        Assert.Equal(42, entity.LastTick);
        Assert.Equal(1.0 / 60.0, entity.LastDeltaSeconds, precision: 12);
        Assert.Equal(new EntityPosition(1, 2, 3), entity.Position);
    }

    private sealed class TestEntity(EntityId id, EntityKind kind, EntityPosition position)
        : EntityBase(id, kind, position)
    {
        public int TickCount { get; private set; }
        public long LastTick { get; private set; }
        public double LastDeltaSeconds { get; private set; }

        public override void Tick(EntityTickContext context)
        {
            TickCount++;
            LastTick = context.Tick;
            LastDeltaSeconds = context.DeltaSeconds;
        }
    }
}
