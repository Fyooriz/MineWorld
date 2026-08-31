using MineWorld.Core.Inventory;
using MineWorld.Core.World;
using Raylib_cs;
using System.Numerics;

namespace MineWorld.Playable;

internal sealed class VoxelWorld
{
    public const byte Air = 0;
    public const byte Grass = 1;
    public const byte Dirt = 2;
    public const byte Stone = 3;

    private const int WorldHeight = VoxelChunk.MaxYExclusive;
    private readonly int _renderDistance;
    private readonly Dictionary<(int X, int Z), VoxelChunk> _chunks = new();
    private readonly Dictionary<(int X, int Y, int Z), byte> _overrides = new();
    private readonly Dictionary<(int X, int Z), ChunkMeshData> _meshCache = new();
    private readonly ChunkMesher _mesher = new();

    public VoxelWorld(int seed, int renderDistance)
    {
        Seed = seed;
        _renderDistance = Math.Max(1, renderDistance);
        StreamAround(0, 0);
    }

    public int Seed { get; }
    public int RenderDistance => _renderDistance;
    public int LoadedChunkCount => _chunks.Count;
    public int CachedMeshCount => _meshCache.Count;
    public IReadOnlyDictionary<(int X, int Y, int Z), byte> BlockOverrides => _overrides;

    public void StreamAround(float worldX, float worldZ)
    {
        var center = HorizontalChunkCoordinate.FromWorld((int)MathF.Floor(worldX), (int)MathF.Floor(worldZ));

        for (var cz = center.Z - _renderDistance; cz <= center.Z + _renderDistance; cz++)
        for (var cx = center.X - _renderDistance; cx <= center.X + _renderDistance; cx++)
            EnsureChunk(cx, cz);

        foreach (var key in _chunks.Keys.Where(key =>
                     Math.Abs(key.X - center.X) > _renderDistance ||
                     Math.Abs(key.Z - center.Z) > _renderDistance).ToArray())
        {
            _chunks.Remove(key);
            _meshCache.Remove(key);
        }
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
        if (y < 0 || y >= WorldHeight) return Air;
        if (_overrides.TryGetValue((x, y, z), out var overridden)) return overridden;

        var chunkCoordinate = HorizontalChunkCoordinate.FromWorld(x, z);
        var chunk = EnsureChunk(chunkCoordinate.X, chunkCoordinate.Z);
        var local = chunkCoordinate.ToLocal(x, z);
        return chunk.GetBlock(local.X, y, local.Z);
    }

    public void SetBlock(int x, int y, int z, byte block)
    {
        if (y < 0 || y >= WorldHeight) return;

        var chunkCoordinate = HorizontalChunkCoordinate.FromWorld(x, z);
        var chunk = EnsureChunk(chunkCoordinate.X, chunkCoordinate.Z);
        var local = chunkCoordinate.ToLocal(x, z);
        var generated = chunk.GetBlock(local.X, y, local.Z);
        chunk.SetBlock(local.X, y, local.Z, block);

        if (block == generated) _overrides.Remove((x, y, z));
        else _overrides[(x, y, z)] = block;

        InvalidateMesh(chunkCoordinate.X, chunkCoordinate.Z);
        if (local.X == 0) InvalidateMesh(chunkCoordinate.X - 1, chunkCoordinate.Z);
        if (local.X == VoxelChunk.Size - 1) InvalidateMesh(chunkCoordinate.X + 1, chunkCoordinate.Z);
        if (local.Z == 0) InvalidateMesh(chunkCoordinate.X, chunkCoordinate.Z - 1);
        if (local.Z == VoxelChunk.Size - 1) InvalidateMesh(chunkCoordinate.X, chunkCoordinate.Z + 1);
    }

    public void ApplySavedBlock(int x, int y, int z, byte block)
    {
        if (y < 0 || y >= WorldHeight) return;

        var chunkCoordinate = HorizontalChunkCoordinate.FromWorld(x, z);
        var chunk = EnsureChunk(chunkCoordinate.X, chunkCoordinate.Z);
        var local = chunkCoordinate.ToLocal(x, z);
        chunk.SetBlock(local.X, y, local.Z, block);
        _overrides[(x, y, z)] = block;
        InvalidateMesh(chunkCoordinate.X, chunkCoordinate.Z);
    }

    public void Draw(Vector3 cameraPosition, Vector3 cameraTarget, int viewportWidth, int viewportHeight)
    {
        var aspect = viewportWidth / (float)Math.Max(1, viewportHeight);
        var frustum = Frustum.Create(
            cameraPosition,
            cameraTarget,
            Vector3.UnitY,
            70f,
            aspect,
            0.05f,
            (_renderDistance + 2) * VoxelChunk.Size);

        foreach (var pair in _chunks.ToArray())
        {
            var chunk = pair.Value;
            var min = new Vector3(chunk.ChunkX * VoxelChunk.Size, 0, chunk.ChunkZ * VoxelChunk.Size);
            if (!frustum.Intersects(new BoundingBox(min, min + new Vector3(VoxelChunk.Size, WorldHeight, VoxelChunk.Size))))
                continue;

            if (!_meshCache.TryGetValue(pair.Key, out var mesh))
            {
                mesh = _mesher.Build(this, chunk);
                _meshCache[pair.Key] = mesh;
            }

            DrawChunkMesh(mesh);
        }
    }

    private static void DrawChunkMesh(ChunkMeshData mesh)
    {
        for (var n = 0; n < mesh.Indices.Length; n += 3)
        {
            var color = mesh.Colors[mesh.Indices[n]];
            Raylib.DrawTriangle3D(
                mesh.Vertices[mesh.Indices[n]],
                mesh.Vertices[mesh.Indices[n + 1]],
                mesh.Vertices[mesh.Indices[n + 2]],
                new Color(color.R, color.G, color.B, color.A));
        }
    }

    private void InvalidateMesh(int x, int z) => _meshCache.Remove((x, z));

    public bool Mine(Ray ray, Inventory inventory) => EditRay(ray, place: false, inventory);
    public bool Place(Ray ray, Inventory inventory) => EditRay(ray, place: true, inventory);

    private bool EditRay(Ray ray, bool place, Inventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        var previousAir = (
            X: (int)MathF.Floor(ray.Position.X),
            Y: (int)MathF.Floor(ray.Position.Y),
            Z: (int)MathF.Floor(ray.Position.Z));

        for (var distance = 0.15f; distance < 8f; distance += 0.04f)
        {
            var point = ray.Position + ray.Direction * distance;
            var x = (int)MathF.Floor(point.X);
            var y = (int)MathF.Floor(point.Y);
            var z = (int)MathF.Floor(point.Z);
            var block = GetBlock(x, y, z);

            if (block == Air)
            {
                previousAir = (x, y, z);
                continue;
            }

            if (place)
            {
                var (px, py, pz) = previousAir;
                if (py >= 0 && py < WorldHeight && GetBlock(px, py, pz) == Air && inventory.TryRemove("core:dirt", 1))
                {
                    SetBlock(px, py, pz, Dirt);
                    return true;
                }

                return false;
            }

            var itemId = ItemIdForBlock(block);
            SetBlock(x, y, z, Air);
            if (inventory.TryAdd(new ItemStack(itemId, 1))) return true;
            SetBlock(x, y, z, block);
            return false;
        }

        return false;
    }

    private static string ItemIdForBlock(byte block) => block switch
    {
        Grass => "core:grass",
        Dirt => "core:dirt",
        Stone => "core:stone",
        _ => "core:unknown"
    };

    private VoxelChunk EnsureChunk(int chunkX, int chunkZ)
    {
        if (_chunks.TryGetValue((chunkX, chunkZ), out var existing)) return existing;

        var chunk = new VoxelChunk(chunkX, chunkZ);
        for (var localZ = 0; localZ < VoxelChunk.Size; localZ++)
        for (var localX = 0; localX < VoxelChunk.Size; localX++)
        {
            var worldX = chunkX * VoxelChunk.Size + localX;
            var worldZ = chunkZ * VoxelChunk.Size + localZ;
            var surface = GetSurfaceHeight(worldX, worldZ);
            for (var y = 0; y <= surface; y++)
                chunk.SetBlock(localX, y, localZ, y == surface ? Grass : y > surface - 4 ? Dirt : Stone);
        }

        foreach (var entry in _overrides)
        {
            var coordinate = HorizontalChunkCoordinate.FromWorld(entry.Key.X, entry.Key.Z);
            if (coordinate.X != chunkX || coordinate.Z != chunkZ) continue;

            var local = coordinate.ToLocal(entry.Key.X, entry.Key.Z);
            chunk.SetBlock(local.X, entry.Key.Y, local.Z, entry.Value);
        }

        _chunks[(chunkX, chunkZ)] = chunk;
        return chunk;
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
}
