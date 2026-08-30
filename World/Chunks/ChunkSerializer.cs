using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MineWorld.Blocks.Runtime;

namespace MineWorld.World.Chunks;

public static class ChunkSerializer
{
    private const int Version = 1;
    private static readonly byte[] Magic = Encoding.ASCII.GetBytes("MWCH");

    public static byte[] Serialize(ChunkBlockStorage chunk)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(Magic);
        writer.Write(Version);
        writer.Write(chunk.Width);
        writer.Write(chunk.Height);
        writer.Write(chunk.Depth);

        for (var y = 0; y < chunk.Height; y++)
        for (var z = 0; z < chunk.Depth; z++)
        for (var x = 0; x < chunk.Width; x++)
        {
            var state = chunk.Get(x, y, z);
            writer.Write(state.RuntimeId);
            writer.Write(state.BlockId);
            writer.Write(state.Properties.Count);
            foreach (var property in state.Properties)
            {
                writer.Write(property.Key);
                writer.Write(property.Value);
            }
        }

        writer.Flush();
        return stream.ToArray();
    }

    public static ChunkBlockStorage Deserialize(ReadOnlySpan<byte> data)
    {
        using var stream = new MemoryStream(data.ToArray(), writable: false);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var magic = reader.ReadBytes(Magic.Length);
        if (!magic.AsSpan().SequenceEqual(Magic)) throw new InvalidDataException("Invalid MineWorld chunk header.");
        var version = reader.ReadInt32();
        if (version != Version) throw new InvalidDataException($"Unsupported chunk version: {version}.");

        var width = reader.ReadInt32();
        var height = reader.ReadInt32();
        var depth = reader.ReadInt32();
        var empty = new BlockState(0, "mineworld:air", new Dictionary<string, string>());
        var chunk = new ChunkBlockStorage(width, height, depth, empty);

        for (var y = 0; y < height; y++)
        for (var z = 0; z < depth; z++)
        for (var x = 0; x < width; x++)
        {
            var runtimeId = reader.ReadInt32();
            var blockId = reader.ReadString();
            var propertyCount = reader.ReadInt32();
            if (propertyCount < 0 || propertyCount > 1024) throw new InvalidDataException("Invalid block property count.");
            var properties = new Dictionary<string, string>(propertyCount, StringComparer.Ordinal);
            for (var i = 0; i < propertyCount; i++) properties.Add(reader.ReadString(), reader.ReadString());
            chunk.Set(x, y, z, new BlockState(runtimeId, blockId, properties));
        }

        return chunk;
    }
}
