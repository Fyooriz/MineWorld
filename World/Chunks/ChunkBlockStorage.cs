using System;
using MineWorld.Blocks.Runtime;

namespace MineWorld.World.Chunks;

public sealed class ChunkBlockStorage
{
    private readonly BlockState[] _blocks;

    public int Width { get; }
    public int Height { get; }
    public int Depth { get; }

    public ChunkBlockStorage(int width, int height, int depth, BlockState defaultState)
    {
        if (width <= 0 || height <= 0 || depth <= 0)
            throw new ArgumentOutOfRangeException("Chunk dimensions must be positive.");

        Width = width;
        Height = height;
        Depth = depth;
        _blocks = new BlockState[checked(width * height * depth)];
        Array.Fill(_blocks, defaultState);
    }

    public BlockState Get(int x, int y, int z)
    {
        return _blocks[ToIndex(x, y, z)];
    }

    public void Set(int x, int y, int z, BlockState state)
    {
        _blocks[ToIndex(x, y, z)] = state;
    }

    private int ToIndex(int x, int y, int z)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height || (uint)z >= (uint)Depth)
            throw new ArgumentOutOfRangeException("Block coordinate is outside the chunk.");
        return x + Width * (z + Depth * y);
    }
}
