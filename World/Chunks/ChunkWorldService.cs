using System;
using MineWorld.Blocks.Runtime;
using MineWorld.World.SaveSystem;

namespace MineWorld.World.Chunks;

public sealed class ChunkWorldService
{
    private readonly BlockRegistry _registry;
    private readonly ChunkGenerationPipeline _generator;
    private readonly WorldSaveService _save;

    public ChunkWorldService(BlockRegistry registry, ChunkGenerationPipeline generator, WorldSaveService save)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _save = save ?? throw new ArgumentNullException(nameof(save));
    }

    public ChunkBlockStorage LoadOrGenerate(int chunkX, int chunkZ)
    {
        return _save.TryLoadChunk(chunkX, chunkZ, out var chunk) && chunk is not null
            ? chunk
            : _generator.Generate(chunkX, chunkZ);
    }

    public void Save(int chunkX, int chunkZ, ChunkBlockStorage chunk) => _save.SaveChunk(chunkX, chunkZ, chunk);

    public BlockState GetBlock(ChunkBlockStorage chunk, int x, int y, int z) => chunk.Get(x, y, z);

    public void SetBlock(ChunkBlockStorage chunk, int x, int y, int z, string blockId)
    {
        if (y < 0 || y >= chunk.Height) throw new ArgumentOutOfRangeException(nameof(y));
        _registry.GetDefinition(blockId);
        chunk.Set(x, y, z, _registry.CreateDefaultState(blockId));
    }
}
