using System.Diagnostics;
using MineWorld.Core.Entities;
using MineWorld.Playable;

namespace MineWorld.Playable.Tests;

public sealed class PlayerRuntimePerformanceTests
{
    [Fact]
    public void EntityRuntimeTickThroughputIsMeasured()
    {
        const int entityCount = 10_000;
        const int tickCount = 120;
        var runtime = new EntityRuntime();

        for (var i = 0; i < entityCount; i++)
            runtime.Add(new MeasuredEntity(new EntityId($"perf:{i}")));

        var stopwatch = Stopwatch.StartNew();
        for (var tick = 1; tick <= tickCount; tick++)
            runtime.Tick(new EntityTickContext(tick, 1f / 60f));
        stopwatch.Stop();

        var totalTicks = (long)entityCount * tickCount;
        var ticksPerSecond = totalTicks / stopwatch.Elapsed.TotalSeconds;
        Console.WriteLine($"PERFORMANCE_MEASURED entity_tick_calls={totalTicks} elapsed_ms={stopwatch.Elapsed.TotalMilliseconds:F2} ticks_per_second={ticksPerSecond:F0}");

        Assert.All(runtime.Snapshot(), entity => Assert.Equal(tickCount, ((MeasuredEntity)entity).TickCount));
    }

    private sealed class MeasuredEntity(EntityId id) : EntityBase(id, EntityKind.Passive, new EntityPosition(0, 0, 0))
    {
        public int TickCount { get; private set; }

        public override void Tick(EntityTickContext context) => TickCount++;
    }
}
