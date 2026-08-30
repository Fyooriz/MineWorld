using System;
using System.Collections.Generic;
using MineWorld.Blocks.Runtime;
using MineWorld.World.SaveSystem;
using MineWorld.World.Terrain;

namespace MineWorld.World.Chunks;

public sealed class ChunkWorldService
{
    private readonly BlockRegistry _registry;
    private readonly ChunkGenerationPipeline _generator;
    private readonly WorldSaveService _save;
    private readonly Dictionary<(int X, int Z), ChunkBlockStorage> _loaded = new();

    public ChunkWorldService(BlockRegistry registry, ChunkGenerationPipeline generator, WorldSaveService save)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _save = save ?? throw new ArgumentNullException(nameof(save));
    }

    public IReadOnlyDictionary<(int X, int Z), ChunkBlockStorage> LoadedChunks => _loaded;

    public ChunkBlockStorage LoadOrGenerate(int chunkX, int chunkZ)
    {
        if (_loaded.TryGetValue((chunkX, chunkZ), out var loaded)) return loaded;
        var chunk = _save.TryLoadChunk(chunkX, chunkZ, out var saved) && saved is not null
            ? saved
            : _generator.Generate(chunkX, chunkZ);
        _loaded[(chunkX, chunkZ)] = chunk;
        return chunk;
    }

    public void Save(int chunkX, int chunkZ)
    {
        if (_loaded.TryGetValue((chunkX, chunkZ), out var chunk)) _save.SaveChunk(chunkX, chunkZ, chunk);
    }

    public void Unload(int chunkX, int chunkZ, bool save = true)
    {
        if (save) Save(chunkX, chunkZ);
        _loaded.Remove((chunkX, chunkZ));
    }

    public BlockState GetBlock(int chunkX, int chunkZ, int x, int y, int z) =>
        LoadOrGenerate(chunkX, chunkZ).Get(x, y, z);

    public void SetBlock(int chunkX, int chunkZ, int x, int y, int z, string blockId)
    {
        var chunk = LoadOrGenerate(chunkX, chunkZ);
        if ((uint)y >= (uint)chunk.Height) throw new ArgumentOutOfRangeException(nameof(y));
        _registry.GetDefinition(blockId);
        chunk.Set(x, y, z, _registry.CreateDefaultState(blockId));
    }
}
