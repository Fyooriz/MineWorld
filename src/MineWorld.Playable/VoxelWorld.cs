using Raylib_cs;
using System.Numerics;

namespace MineWorld.Playable;

internal sealed class VoxelWorld
{
    public int Seed { get; }
    private readonly int _radius;
    private readonly Dictionary<(int X, int Y, int Z), byte> _blocks = new();

    public VoxelWorld(int seed, int renderDistance)
    {
        Seed = seed;
        _radius = renderDistance * 16;
        Generate();
    }

    public int GetSurfaceHeight(int x, int z)
    {
        var n = Noise(x, z);
        var waves = MathF.Sin(x * 0.055f) * 3f + MathF.Cos(z * 0.045f) * 3f;
        return Math.Clamp(16 + (int)(n * 8f + waves), 3, 62);
    }

    private void Generate()
    {
        for (var x = -_radius; x <= _radius; x++)
        for (var z = -_radius; z <= _radius; z++)
        {
            var surface = GetSurfaceHeight(x, z);
            for (var y = 0; y <= surface; y++)
                _blocks[(x, y, z)] = (byte)(y == surface ? 1 : y > surface - 4 ? 2 : 3);
        }
    }

    public void Draw()
    {
        foreach (var b in _blocks)
        {
            var color = b.Value switch
            {
                1 => new Color(91, 160, 74, 255),
                2 => new Color(130, 88, 52, 255),
                _ => new Color(112, 116, 122, 255)
            };
            var p = b.Key;
            Raylib.DrawCube(new Vector3(p.X + .5f, p.Y + .5f, p.Z + .5f), 1f, 1f, 1f, color);
        }
    }

    public void Mine(Ray3D ray) => EditRay(ray, false);
    public void Place(Ray3D ray) => EditRay(ray, true);

    private void EditRay(Ray3D ray, bool place)
    {
        for (var d = .2f; d < 8f; d += .05f)
        {
            var p = ray.Position + ray.Direction * d;
            var key = ((int)MathF.Floor(p.X), (int)MathF.Floor(p.Y), (int)MathF.Floor(p.Z));
            if (!_blocks.ContainsKey(key)) continue;
            if (place)
            {
                var q = p - ray.Direction * .06f;
                _blocks[((int)MathF.Floor(q.X), (int)MathF.Floor(q.Y), (int)MathF.Floor(q.Z))] = 2;
            }
            else _blocks.Remove(key);
            return;
        }
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
