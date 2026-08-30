using System;
using MineWorld.Blocks.Runtime;
using MineWorld.World.Chunks;

namespace MineWorld.Player;

public readonly record struct VoxelRay(Vector3 Origin, Vector3 Direction);
public readonly record struct Vector3(float X, float Y, float Z)
{
    public static Vector3 Normalize(Vector3 value)
    {
        var length = MathF.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
        return length <= float.Epsilon ? new Vector3(0, 0, 0) : new Vector3(value.X / length, value.Y / length, value.Z / length);
    }

    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator *(Vector3 a, float b) => new(a.X * b, a.Y * b, a.Z * b);
}

public readonly record struct VoxelHit(int X, int Y, int Z, int PreviousX, int PreviousY, int PreviousZ, float Distance, BlockState State);

public sealed class PlayerVoxelInteractor
{
    private readonly ChunkWorldService _world;
    private readonly int _chunkWidth;
    private readonly int _chunkDepth;

    public PlayerVoxelInteractor(ChunkWorldService world, int chunkWidth = 16, int chunkDepth = 16)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _chunkWidth = chunkWidth > 0 ? chunkWidth : throw new ArgumentOutOfRangeException(nameof(chunkWidth));
        _chunkDepth = chunkDepth > 0 ? chunkDepth : throw new ArgumentOutOfRangeException(nameof(chunkDepth));
    }

    public bool TryRaycast(VoxelRay ray, float maxDistance, out VoxelHit hit, float step = 0.05f)
    {
        hit = default;
        if (maxDistance <= 0 || step <= 0) return false;
        var direction = Vector3.Normalize(ray.Direction);
        var previous = ToVoxel(ray.Origin);

        for (var distance = 0f; distance <= maxDistance; distance += step)
        {
            var position = ray.Origin + direction * distance;
            var voxel = ToVoxel(position);
            if (voxel == previous && distance > 0) continue;

            var state = GetWorldBlock(voxel.X, voxel.Y, voxel.Z);
            if (!state.BlockId.Equals("mineworld:air", StringComparison.Ordinal))
            {
                hit = new VoxelHit(voxel.X, voxel.Y, voxel.Z, previous.X, previous.Y, previous.Z, distance, state);
                return true;
            }
            previous = voxel;
        }

        return false;
    }

    public void PlaceBlock(VoxelHit hit, string blockId)
    {
        SetWorldBlock(hit.PreviousX, hit.PreviousY, hit.PreviousZ, blockId);
    }

    public void BreakBlock(VoxelHit hit)
    {
        SetWorldBlock(hit.X, hit.Y, hit.Z, "mineworld:air");
    }

    private BlockState GetWorldBlock(int x, int y, int z)
    {
        if (y < 0) return _world.GetBlock(0, 0, 0, 0, 0);
        var chunkX = FloorDiv(x, _chunkWidth);
        var chunkZ = FloorDiv(z, _chunkDepth);
        var localX = Mod(x, _chunkWidth);
        var localZ = Mod(z, _chunkDepth);
        var chunk = _world.LoadOrGenerate(chunkX, chunkZ);
        if ((uint)y >= (uint)chunk.Height) return chunk.Get(0, 0, 0);
        return chunk.Get(localX, y, localZ);
    }

    private void SetWorldBlock(int x, int y, int z, string blockId)
    {
        var chunkX = FloorDiv(x, _chunkWidth);
        var chunkZ = FloorDiv(z, _chunkDepth);
        _world.SetBlock(chunkX, chunkZ, Mod(x, _chunkWidth), y, Mod(z, _chunkDepth), blockId);
    }

    private static (int X, int Y, int Z) ToVoxel(Vector3 p) =>
        ((int)MathF.Floor(p.X), (int)MathF.Floor(p.Y), (int)MathF.Floor(p.Z));

    private static int FloorDiv(int value, int divisor) => (int)MathF.Floor((float)value / divisor);
    private static int Mod(int value, int divisor) => ((value % divisor) + divisor) % divisor;
}
