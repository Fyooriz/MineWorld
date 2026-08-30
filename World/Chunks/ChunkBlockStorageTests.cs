using System;
using MineWorld.Blocks.Runtime;

namespace MineWorld.World.Chunks;

public static class ChunkBlockStorageTests
{
    public static void Run()
    {
        var air = new BlockState(0, "mineworld:air", new Dictionary<string, string>());
        var stone = new BlockState(1, "mineworld:stone", new Dictionary<string, string>());
        var chunk = new ChunkBlockStorage(16, 64, 16, air);

        if (chunk.Get(0, 0, 0).BlockId != "mineworld:air") throw new Exception("Default block mismatch.");
        chunk.Set(3, 12, 7, stone);
        if (chunk.Get(3, 12, 7).BlockId != "mineworld:stone") throw new Exception("Stored block mismatch.");

        try
        {
            chunk.Get(16, 0, 0);
            throw new Exception("Out-of-range coordinate was accepted.");
        }
        catch (ArgumentOutOfRangeException) { }
    }
}
