namespace MineWorld.Core.World;

/// <summary>
/// Canonical P0 runtime world state. Owns generated chunks and persistent block deltas;
/// rendering, input, persistence, and transport remain outside this class.
/// </summary>
public sealed class WorldState
{
    public const int DefaultWorldHeight = Chunk.Height;

    private readonly Dictionary<ChunkCoordinate, Chunk> _chunks = new();
    private readonly Dictionary<ChunkCoordinate, Dictionary<int, BlockId>> _overrides = new();
    private readonly IWorldGenerator _generator;

    public WorldState(long seed, WorldGeneratorConfig? config = null, IWorldGenerator? generator = null)
    {
        Seed = seed;
        GeneratorConfig = config ?? new WorldGeneratorConfig(Version: 1, SeaLevel: 32, BaseHeight: 32, HeightAmplitude: 16);
        _generator = generator ?? new BasicWorldGenerator();
    }

    public long Seed { get; }
    public WorldGeneratorConfig GeneratorConfig { get; }
    public int LoadedChunkCount => _chunks.Count;

    public Chunk GetOrCreateChunk(int chunkX, int chunkZ)
    {
        var coordinate = new ChunkCoordinate(chunkX, 0, chunkZ);
        if (_chunks.TryGetValue(coordinate, out var existing))
            return existing;

        var chunk = new Chunk(coordinate);
        for (var y = 0; y < Chunk.Height; y++)
        for (var z = 0; z < Chunk.Size; z++)
        for (var x = 0; x < Chunk.Size; x++)
        {
            var worldX = chunkX * Chunk.Size + x;
            var worldZ = chunkZ * Chunk.Size + z;
            chunk.SetBlock(x, y, z, _generator.GenerateBlock(worldX, y, worldZ, Seed, GeneratorConfig));
        }

        if (_overrides.TryGetValue(coordinate, out var overrides))
        {
            foreach (var entry in overrides)
            {
                var localX = entry.Key % Chunk.Size;
                var localZ = entry.Key / Chunk.Size % Chunk.Size;
                var y = entry.Key / (Chunk.Size * Chunk.Size);
                chunk.SetBlock(localX, y, localZ, entry.Value);
            }
        }

        _chunks.Add(coordinate, chunk);
        return chunk;
    }

    public bool TryGetLoadedChunk(int chunkX, int chunkZ, out Chunk? chunk)
        => _chunks.TryGetValue(new ChunkCoordinate(chunkX, 0, chunkZ), out chunk);

    public bool UnloadChunk(int chunkX, int chunkZ)
        => _chunks.Remove(new ChunkCoordinate(chunkX, 0, chunkZ));

    public BlockId GetBlock(int x, int y, int z)
    {
        if (y < 0 || y >= Chunk.Height)
            return BlockId.Air;

        var chunkX = FloorDiv(x, Chunk.Size);
        var chunkZ = FloorDiv(z, Chunk.Size);
        var chunk = GetOrCreateChunk(chunkX, chunkZ);
        return chunk.GetBlock(FloorMod(x, Chunk.Size), y, FloorMod(z, Chunk.Size));
    }

    public void SetBlock(int x, int y, int z, BlockId block)
    {
        if (y < 0 || y >= Chunk.Height)
            throw new ArgumentOutOfRangeException(nameof(y));

        var chunkX = FloorDiv(x, Chunk.Size);
        var chunkZ = FloorDiv(z, Chunk.Size);
        var chunkCoordinate = new ChunkCoordinate(chunkX, 0, chunkZ);
        var chunk = GetOrCreateChunk(chunkX, chunkZ);
        var localX = FloorMod(x, Chunk.Size);
        var localZ = FloorMod(z, Chunk.Size);
        var generated = _generator.GenerateBlock(x, y, z, Seed, GeneratorConfig);
        chunk.SetBlock(localX, y, localZ, block);

        var key = EncodeLocal(localX, y, localZ);
        if (block == generated)
        {
            if (_overrides.TryGetValue(chunkCoordinate, out var overrides))
            {
                overrides.Remove(key);
                if (overrides.Count == 0)
                    _overrides.Remove(chunkCoordinate);
            }
        }
        else
        {
            if (!_overrides.TryGetValue(chunkCoordinate, out var overrides))
                _overrides[chunkCoordinate] = overrides = new Dictionary<int, BlockId>();
            overrides[key] = block;
        }
    }

    public IEnumerable<(int X, int Y, int Z, BlockId Block)> EnumerateOverrides()
    {
        foreach (var (coordinate, overrides) in _overrides)
        foreach (var (key, block) in overrides)
        {
            var localX = key % Chunk.Size;
            var localZ = key / Chunk.Size % Chunk.Size;
            var y = key / (Chunk.Size * Chunk.Size);
            yield return (
                coordinate.X * Chunk.Size + localX,
                y,
                coordinate.Z * Chunk.Size + localZ,
                block);
        }
    }

    public int GetSurfaceHeight(int x, int z)
    {
        for (var y = Chunk.Height - 1; y >= 0; y--)
        {
            if (GetBlock(x, y, z) != BlockId.Air)
                return y;
        }

        return 0;
    }

    private static int EncodeLocal(int x, int y, int z) => (y * Chunk.Size + z) * Chunk.Size + x;

    private static int FloorDiv(int value, int divisor)
    {
        var quotient = value / divisor;
        var remainder = value % divisor;
        return remainder >= 0 ? quotient : quotient - 1;
    }

    private static int FloorMod(int value, int divisor)
    {
        var result = value % divisor;
        return result < 0 ? result + divisor : result;
    }
}
