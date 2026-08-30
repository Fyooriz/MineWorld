using Raylib_cs;
using System.Numerics;

namespace MineWorld.Playable;

internal sealed class VoxelWorld
{
    public const byte Air = 0;
    public const byte Grass = 1;
    public const byte Dirt = 2;
    public const byte Stone = 3;

    private const int WaterLevel = 12;
    private const int WorldHeight = VoxelChunk.MaxYExclusive;

    private readonly int _renderDistance;
    private readonly Dictionary<(int X, int Z), VoxelChunk> _chunks = new();
    private readonly Dictionary<(int X, int Y, int Z), byte> _overrides = new();

    public VoxelWorld(int seed, int renderDistance)
    {
        Seed = seed;
        _renderDistance = Math.Max(1, renderDistance);
        StreamAround(0, 0);
    }

    public int Seed { get; }
    public int LoadedChunkCount => _chunks.Count;
    public IReadOnlyDictionary<(int X, int Y, int Z), byte> BlockOverrides => _overrides;

    public void StreamAround(float worldX, float worldZ)
    {
        var centerX = FloorDiv((int)MathF.Floor(worldX), VoxelChunk.Size);
        var centerZ = FloorDiv((int)MathF.Floor(worldZ), VoxelChunk.Size);

        for (var cz = centerZ - _renderDistance; cz <= centerZ + _renderDistance; cz++)
        for (var cx = centerX - _renderDistance; cx <= centerX + _renderDistance; cx++)
            EnsureChunk(cx, cz);
    }

    public int GetSurfaceHeight(int x, int z)
    {
        var n = Noise(x, z);
        var broad = MathF.Sin(x * 0.035f) * 4f + MathF.Cos(z * 0.03f) * 4f;
        return Math.Clamp(16 + (int)(n * 7f + broad), 4, WorldHeight - 2);
    }

    public bool IsSolid(int x, int y, int z) => GetBlock(x, y, z) != Air;

    public byte GetBlock(int x, int y, int z)
    {
        if (y < 0 || y >= WorldHeight)
            return Air;

        if (_overrides.TryGetValue((x, y, z), out var overridden))
            return overridden;

        var chunkX = FloorDiv(x, VoxelChunk.Size);
        var chunkZ = FloorDiv(z, VoxelChunk.Size);
        var chunk = EnsureChunk(chunkX, chunkZ);
        return chunk.GetBlock(FloorMod(x, VoxelChunk.Size), y, FloorMod(z, VoxelChunk.Size));
    }

    public void SetBlock(int x, int y, int z, byte block)
    {
        if (y < 0 || y >= WorldHeight)
            return;

        var chunkX = FloorDiv(x, VoxelChunk.Size);
        var chunkZ = FloorDiv(z, VoxelChunk.Size);
        var chunk = EnsureChunk(chunkX, chunkZ);
        var localX = FloorMod(x, VoxelChunk.Size);
        var localZ = FloorMod(z, VoxelChunk.Size);
        var generated = chunk.GetBlock(localX, y, localZ);

        chunk.SetBlock(localX, y, localZ, block);
        if (block == generated)
            _overrides.Remove((x, y, z));
        else
            _overrides[(x, y, z)] = block;
    }

    public void ApplySavedBlock(int x, int y, int z, byte block)
    {
        if (y < 0 || y >= WorldHeight)
            return;

        var chunkX = FloorDiv(x, VoxelChunk.Size);
        var chunkZ = FloorDiv(z, VoxelChunk.Size);
        var chunk = EnsureChunk(chunkX, chunkZ);
        chunk.SetBlock(FloorMod(x, VoxelChunk.Size), y, FloorMod(z, VoxelChunk.Size), block);
        _overrides[(x, y, z)] = block;
    }

    public void Draw()
    {
        foreach (var chunk in _chunks.Values)
        {
            for (var y = 0; y < WorldHeight; y++)
            for (var z = 0; z < VoxelChunk.Size; z++)
            for (var x = 0; x < VoxelChunk.Size; x++)
            {
                var block = chunk.GetBlock(x, y, z);
                if (block == Air || !HasVisibleFace(chunk, x, y, z))
                    continue;

                var worldX = chunk.ChunkX * VoxelChunk.Size + x;
                var worldZ = chunk.ChunkZ * VoxelChunk.Size + z;
                var color = block switch
                {
                    Grass => new Color(91, 160, 74, 255),
                    Dirt => new Color(130, 88, 52, 255),
                    Stone => new Color(112, 116, 122, 255),
                    _ => new Color(255, 255, 255, 255)
                };

                Raylib.DrawCube(new Vector3(worldX + 0.5f, y + 0.5f, worldZ + 0.5f), 1f, 1f, 1f, color);
            }
        }
    }

    public void Mine(Ray3D ray) => EditRay(ray, place: false);
    public void Place(Ray3D ray) => EditRay(ray, place: true);

    private void EditRay(Ray3D ray, bool place)
    {
        var previous = ray.Position;

        for (var distance = 0.15f; distance < 8f; distance += 0.04f)
        {
            var point = ray.Position + ray.Direction * distance;
            var x = (int)MathF.Floor(point.X);
            var y = (int)MathF.Floor(point.Y);
            var z = (int)MathF.Floor(point.Z);

            if (GetBlock(x, y, z) == Air)
            {
                previous = point;
                continue;
            }

            if (place)
            {
                var px = (int)MathF.Floor(previous.X);
                var py = (int)MathF.Floor(previous.Y);
                var pz = (int)MathF.Floor(previous.Z);
                if (GetBlock(px, py, pz) == Air)
                    SetBlock(px, py, pz, Dirt);
            }
            else
            {
                SetBlock(x, y, z, Air);
            }

            return;
        }
    }

    private VoxelChunk EnsureChunk(int chunkX, int chunkZ)
    {
        if (_chunks.TryGetValue((chunkX, chunkZ), out var existing))
            return existing;

        var chunk = new VoxelChunk(chunkX, chunkZ);
        for (var localZ = 0; localZ < VoxelChunk.Size; localZ++)
        for (var localX = 0; localX < VoxelChunk.Size; localX++)
        {
            var worldX = chunkX * VoxelChunk.Size + localX;
            var worldZ = chunkZ * VoxelChunk.Size + localZ;
            var surface = GetSurfaceHeight(worldX, worldZ);

            for (var y = 0; y <= surface; y++)
            {
                var block = y == surface ? Grass : y > surface - 4 ? Dirt : Stone;
                chunk.SetBlock(localX, y, localZ, block);
            }
        }

        foreach (var entry in _overrides)
        {
            if (FloorDiv(entry.Key.X, VoxelChunk.Size) != chunkX ||
                FloorDiv(entry.Key.Z, VoxelChunk.Size) != chunkZ)
                continue;

            chunk.SetBlock(
                FloorMod(entry.Key.X, VoxelChunk.Size),
                entry.Key.Y,
                FloorMod(entry.Key.Z, VoxelChunk.Size),
                entry.Value);
        }

        _chunks[(chunkX, chunkZ)] = chunk;
        return chunk;
    }

    private bool HasVisibleFace(VoxelChunk chunk, int x, int y, int z)
    {
        var wx = chunk.ChunkX * VoxelChunk.Size + x;
        var wz = chunk.ChunkZ * VoxelChunk.Size + z;
        return GetBlock(wx + 1, y, wz) == Air ||
               GetBlock(wx - 1, y, wz) == Air ||
               GetBlock(wx, y + 1, wz) == Air ||
               GetBlock(wx, y - 1, wz) == Air ||
               GetBlock(wx, y, wz + 1) == Air ||
               GetBlock(wx, y, wz - 1) == Air;
    }

    private float Noise(int x, int z)
    {
        unchecked
        {
            var h = Seed * 374761393 + x * 668265263 + z * 2147483647;
            h = (h ^ (h >> 13)) * 1274126177;
            h ^= h >> 16;
            return (h & 0x7fffffff) / 1073741823f - 1f;
        }
    }

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
