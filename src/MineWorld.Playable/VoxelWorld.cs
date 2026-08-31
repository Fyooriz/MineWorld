using MineWorld.Core.Inventory;
using MineWorld.Core.World;
using Raylib_cs;
using System.Numerics;

namespace MineWorld.Playable;

/// <summary>
/// Client-facing world facade over the canonical Core WorldState.
/// Owns only render-derived state; simulation state remains in Core.
/// </summary>
internal sealed class VoxelWorld
{
    public const byte Air = (byte)0;
    public const byte Stone = (byte)1;
    public const byte Dirt = (byte)2;
    public const byte Grass = (byte)3;

    private readonly int _renderDistance;
    private readonly WorldState _state;
    private readonly Dictionary<(int X, int Z), VoxelChunk> _chunks = new();
    private readonly Dictionary<(int X, int Z), ChunkMeshData> _meshCache = new();
    private readonly ChunkMesher _mesher = new();

    public VoxelWorld(int seed, int renderDistance)
    {
        Seed = seed;
        _renderDistance = Math.Max(1, renderDistance);
        _state = new WorldState(seed);
        StreamAround(0, 0);
    }

    public int Seed { get; }
    public int LoadedChunkCount => _chunks.Count;
    public int CachedMeshCount => _meshCache.Count;
    internal WorldState State => _state;
    internal IEnumerable<(int X, int Y, int Z, byte Block)> BlockOverrides
        => _state.EnumerateOverrides().Select(static value => (value.X, value.Y, value.Z, checked((byte)value.Block.Value)));

    public void StreamAround(float worldX, float worldZ)
    {
        var centerX = FloorDiv((int)MathF.Floor(worldX), VoxelChunk.Size);
        var centerZ = FloorDiv((int)MathF.Floor(worldZ), VoxelChunk.Size);
        var required = new HashSet<(int X, int Z)>();

        for (var cz = centerZ - _renderDistance; cz <= centerZ + _renderDistance; cz++)
        for (var cx = centerX - _renderDistance; cx <= centerX + _renderDistance; cx++)
        {
            var key = (cx, cz);
            required.Add(key);
            EnsureChunk(cx, cz);
        }

        foreach (var key in _chunks.Keys.Where(key => !required.Contains(key)).ToArray())
        {
            _chunks.Remove(key);
            _meshCache.Remove(key);
            _state.UnloadChunk(key.X, key.Z);
        }
    }

    public int GetSurfaceHeight(int x, int z) => _state.GetSurfaceHeight(x, z);
    public bool IsSolid(int x, int y, int z) => GetBlock(x, y, z) != Air;

    public byte GetBlock(int x, int y, int z) => checked((byte)_state.GetBlock(x, y, z).Value);

    public void SetBlock(int x, int y, int z, byte block)
    {
        if (y < 0 || y >= VoxelChunk.MaxYExclusive)
            return;

        _state.SetBlock(x, y, z, new BlockId(block));
        var chunkX = FloorDiv(x, VoxelChunk.Size);
        var chunkZ = FloorDiv(z, VoxelChunk.Size);
        InvalidateMesh(chunkX, chunkZ);

        var localX = FloorMod(x, VoxelChunk.Size);
        var localZ = FloorMod(z, VoxelChunk.Size);
        if (localX == 0) InvalidateMesh(chunkX - 1, chunkZ);
        if (localX == VoxelChunk.Size - 1) InvalidateMesh(chunkX + 1, chunkZ);
        if (localZ == 0) InvalidateMesh(chunkX, chunkZ - 1);
        if (localZ == VoxelChunk.Size - 1) InvalidateMesh(chunkX, chunkZ + 1);
    }

    public void ApplySavedBlock(int x, int y, int z, byte block) => SetBlock(x, y, z, block);

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
            if (!frustum.Intersects(new BoundingBox(min, min + new Vector3(VoxelChunk.Size, VoxelChunk.MaxYExclusive, VoxelChunk.Size))))
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
                if (py >= 0 && py < VoxelChunk.MaxYExclusive && GetBlock(px, py, pz) == Air && inventory.TryRemove("core:dirt", 1))
                {
                    SetBlock(px, py, pz, Dirt);
                    return true;
                }

                return false;
            }

            var itemId = ItemIdForBlock(block);
            SetBlock(x, y, z, Air);
            if (inventory.TryAdd(new ItemStack(itemId, 1)))
                return true;

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
        if (_chunks.TryGetValue((chunkX, chunkZ), out var existing))
            return existing;

        var wrapper = new VoxelChunk(_state.GetOrCreateChunk(chunkX, chunkZ));
        _chunks[(chunkX, chunkZ)] = wrapper;
        return wrapper;
    }

    private void InvalidateMesh(int x, int z) => _meshCache.Remove((x, z));

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
