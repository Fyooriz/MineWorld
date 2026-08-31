using System.Numerics;
using MineWorld.Playable;

namespace MineWorld.Playable.Tests;

public sealed class P1ChunkGenerationCancellationTests
{
    [Fact]
    public void CompletedChunkCannotBeRequeuedUntilConsumed()
    {
        var key = new ChunkKey(0, 0);
        var calls = 0;
        var scheduler = new ChunkGenerationScheduler(
            viewDistance: 0,
            generate: requestedKey =>
            {
                Interlocked.Increment(ref calls);
                return new ChunkMesh(requestedKey, Array.Empty<ChunkVertex>());
            });

        try
        {
            var loaded = new HashSet<ChunkKey>();
            scheduler.Update(Vector3.Zero, loaded);
            Assert.True(SpinWait.SpinUntil(() => scheduler.CompletedCount == 1, TimeSpan.FromSeconds(2)));

            scheduler.Update(Vector3.Zero, loaded);
            Assert.Equal(1, Volatile.Read(ref calls));
            Assert.Equal(1, scheduler.CompletedCount);

            Assert.True(scheduler.TryTakeCompleted(out var completed));
            Assert.Equal(key, completed.Key);
        }
        finally
        {
            scheduler.Dispose();
        }
    }

    [Fact]
    public void DisposingSchedulerCancelsInFlightGenerationWithoutPublishingResult()
    {
        using var started = new ManualResetEventSlim(false);
        var scheduler = new ChunkGenerationScheduler(
            viewDistance: 0,
            generate: (key, cancellationToken) =>
            {
                started.Set();
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Thread.Sleep(2);
                }
            });

        scheduler.Update(Vector3.Zero, new HashSet<ChunkKey>());
        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));

        scheduler.Dispose();

        Assert.Equal(0, scheduler.CompletedCount);
    }
}
