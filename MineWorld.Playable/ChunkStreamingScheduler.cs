using System.Numerics;

namespace MineWorld.Playable;

/// <summary>
/// Small, deterministic streaming scheduler. It separates the desired chunk set from
/// generation work so world generation can later move to worker threads without changing
/// the game loop contract.
/// </summary>
internal sealed class ChunkStreamingScheduler
{
    private readonly int _viewDistance;
    private readonly Queue<ChunkKey> _pending = new();
    private readonly HashSet<ChunkKey> _queued = new();

    public ChunkStreamingScheduler(int viewDistance)
    {
        if (viewDistance < 0)
            throw new ArgumentOutOfRangeException(nameof(viewDistance));

        _viewDistance = viewDistance;
    }

    public int PendingCount => _pending.Count;

    public void Rebuild(Vector3 playerPosition, ISet<ChunkKey> loaded)
    {
        var center = new ChunkKey(
            (int)MathF.Floor(playerPosition.X / 16f),
            (int)MathF.Floor(playerPosition.Z / 16f));

        for (var dz = -_viewDistance; dz <= _viewDistance; dz++)
        {
            for (var dx = -_viewDistance; dx <= _viewDistance; dx++)
            {
                var key = new ChunkKey(center.X + dx, center.Z + dz);
                if (loaded.Contains(key) || !_queued.Add(key))
                    continue;

                _pending.Enqueue(key);
            }
        }
    }

    public bool TryDequeue(out ChunkKey key)
    {
        if (_pending.Count == 0)
        {
            key = default;
            return false;
        }

        key = _pending.Dequeue();
        _queued.Remove(key);
        return true;
    }
}
