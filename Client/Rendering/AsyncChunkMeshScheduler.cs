using System.Collections.Concurrent;

namespace MineWorld.Playable;

/// <summary>Runs CPU voxel meshing off the render thread and discards stale results.</summary>
internal sealed class AsyncChunkMeshScheduler : IDisposable
{
    private readonly BlockingCollection<MeshWorkItem> _queue = new();
    private readonly ConcurrentDictionary<ChunkKey, int> _versions = new();
    private readonly ConcurrentQueue<MeshBuildResult> _completed = new();
    private readonly Thread[] _workers;
    private int _disposed;

    public AsyncChunkMeshScheduler(int workerCount = 0)
    {
        workerCount = workerCount <= 0 ? Math.Max(1, Environment.ProcessorCount - 1) : workerCount;
        _workers = Enumerable.Range(0, workerCount)
            .Select(i => new Thread(WorkerLoop) { IsBackground = true, Name = $"MW-Mesher-{i}" })
            .ToArray();
        foreach (var worker in _workers) worker.Start();
    }

    public int PendingCount => _queue.Count;
    public int CompletedCount => _completed.Count;

    public int Schedule(ChunkKey key, Func<ChunkMeshData> build)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        ArgumentNullException.ThrowIfNull(build);
        var version = _versions.AddOrUpdate(key, 1, static (_, old) => checked(old + 1));
        _queue.Add(new MeshWorkItem(key, version, build));
        return version;
    }

    public bool TryDequeueCompleted(out MeshBuildResult result) => _completed.TryDequeue(out result);

    private void WorkerLoop()
    {
        try
        {
            foreach (var item in _queue.GetConsumingEnumerable())
            {
                if (Volatile.Read(ref _disposed) != 0) return;
                try
                {
                    var mesh = item.Build();
                    if (_versions.TryGetValue(item.Key, out var current) && current == item.Version)
                        _completed.Enqueue(new MeshBuildResult(item.Key, item.Version, mesh, null));
                }
                catch (Exception ex)
                {
                    if (_versions.TryGetValue(item.Key, out var current) && current == item.Version)
                        _completed.Enqueue(new MeshBuildResult(item.Key, item.Version, null, ex));
                }
            }
        }
        catch (ObjectDisposedException) { }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        _queue.CompleteAdding();
        foreach (var worker in _workers) worker.Join(TimeSpan.FromSeconds(1));
        _queue.Dispose();
        _versions.Clear();
        while (_completed.TryDequeue(out _)) { }
    }

    private readonly record struct MeshWorkItem(ChunkKey Key, int Version, Func<ChunkMeshData> Build);
}

internal readonly record struct ChunkKey(int X, int Z);

internal sealed record MeshBuildResult(ChunkKey Key, int Version, ChunkMeshData? Mesh, Exception? Error)
{
    public bool Succeeded => Mesh is not null && Error is null;
}
