using System;
using System.IO;
using MineWorld.World.Chunks;

namespace MineWorld.World.SaveSystem;

public sealed class WorldSaveService
{
    private readonly string _rootDirectory;

    public WorldSaveService(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory)) throw new ArgumentException("Save directory is required.", nameof(rootDirectory));
        _rootDirectory = rootDirectory;
        Directory.CreateDirectory(_rootDirectory);
    }

    public void SaveChunk(int chunkX, int chunkZ, ChunkBlockStorage chunk)
    {
        var path = GetChunkPath(chunkX, chunkZ);
        var temporary = path + ".tmp";
        File.WriteAllBytes(temporary, ChunkSerializer.Serialize(chunk));
        File.Move(temporary, path, overwrite: true);
    }

    public bool TryLoadChunk(int chunkX, int chunkZ, out ChunkBlockStorage? chunk)
    {
        var path = GetChunkPath(chunkX, chunkZ);
        if (!File.Exists(path))
        {
            chunk = null;
            return false;
        }

        chunk = ChunkSerializer.Deserialize(File.ReadAllBytes(path));
        return true;
    }

    private string GetChunkPath(int chunkX, int chunkZ) =>
        Path.Combine(_rootDirectory, $"chunk_{chunkX}_{chunkZ}.mwchunk");
}
