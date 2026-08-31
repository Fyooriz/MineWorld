using System.Collections.Concurrent;
using System.Numerics;

namespace MineWorld.Playable;

internal readonly record struct GeneratedChunk(ChunkKey Key, ChunkMeshData Mesh);

/// <summary>Background CPU generation/meshing pipeline. GPU resources remain renderer-owned.</summary>
internal sealed class ChunkGenerationScheduler : IDisposable
{
    private readonly ChunkStreamingScheduler _streaming;
    private readonly Func<ChunkKey, CancellationToken, ChunkMeshData> _generate;
    private readonly ConcurrentQueue<ChunkKey> _work = new();
    private readonly ConcurrentQueue<GeneratedChunk> _completed = new();
    private readonly HashSet<ChunkKey> _inFlight = new();
    private readonly object _gate = new();
    private readonly CancellationTokenSource _shutdown = new();
    private readonly List<Task> _workers = new();

    public ChunkGenerationScheduler(int viewDistance, Func<ChunkKey, ChunkMeshData> generate, int workerCount = 1)
        : this(viewDistance, (key, _) => generate(key), workerCount)
    {
    }

    public ChunkGenerationScheduler(int viewDistance, Func<ChunkKey, CancellationToken, ChunkMeshData> generate, int workerCount = 1)
    {
        _streaming = new ChunkStreamingScheduler(viewDistance);
        _generate = generate ?? throw new ArgumentNullException(nameof(generate));
        if (workerCount < 1) throw new ArgumentOutOfRangeException(nameof(workerCount));
        for (var i = 0; i < workerCount; i++)
            _workers.Add(Task.Run(WorkerLoop));
    }

    public int PendingCount => _work.Count;
    public int CompletedCount => _completed.Count;

    public void Update(Vector3 playerPosition, ISet<ChunkKey> loaded)
    {
        _streaming.Rebuild(playerPosition, loaded);
        while (_streaming.TryDequeue(out var key))
        {
            lock (_gate)
            {
                if (!_inFlight.Add(key))
                    continue;
            }
            _work.Enqueue(key);
        }
    }

    public bool TryTakeCompleted(out GeneratedChunk chunk)
    {
        if (!_completed.TryDequeue(out chunk))
            return false;

        lock (_gate)
            _inFlight.Remove(chunk.Key);

        return true;
    }

    private async Task WorkerLoop()
    {
        while (!_shutdown.IsCancellationRequested)
        {
            if (!_work.TryDequeue(out var key))
            {
                try { await Task.Delay(2, _shutdown.Token).ConfigureAwait(false); }
                catch (OperationCanceledException) { }
                continue;
            }

            try
            {
                var generated = _generate(key, _shutdown.Token);
                if (!_shutdown.IsCancellationRequested)
                    _completed.Enqueue(new GeneratedChunk(key, generated));
            }
            catch (OperationCanceledException) when (_shutdown.IsCancellationRequested)
            {
            }
            catch
            {
                lock (_gate) _inFlight.Remove(key);
                throw;
            }
            finally
            {
                if (_shutdown.IsCancellationRequested)
                {
                    lock (_gate) _inFlight.Remove(key);
                }
            }
        }
    }

    public void Dispose()
    {
        _shutdown.Cancel();
        try { Task.WaitAll(_workers.ToArray(), TimeSpan.FromSeconds(2)); }
        catch (AggregateException) { }
        _shutdown.Dispose();
    }
}
