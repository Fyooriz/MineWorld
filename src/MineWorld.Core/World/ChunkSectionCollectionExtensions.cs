namespace MineWorld.Core.World;

public static class ChunkSectionCollectionExtensions
{
    public static BlockId GetBlock(this ChunkSectionCollection collection, int worldY, int localX, int localZ)
    {
        ArgumentNullException.ThrowIfNull(collection);
        if (localX is < 0 or >= ChunkSection.Size) throw new ArgumentOutOfRangeException(nameof(localX));
        if (localZ is < 0 or >= ChunkSection.Size) throw new ArgumentOutOfRangeException(nameof(localZ));

        var sectionY = Math.DivRem(worldY, ChunkSection.Size, out var localY);
        if (localY < 0)
        {
            sectionY--;
            localY += ChunkSection.Size;
        }

        return collection.TryGet(sectionY, out var section) && section is not null
            ? section.GetBlock(localX, localY, localZ)
            : BlockId.Air;
    }

    public static void SetBlock(this ChunkSectionCollection collection, int worldY, int localX, int localZ, BlockId block)
    {
        ArgumentNullException.ThrowIfNull(collection);
        if (localX is < 0 or >= ChunkSection.Size) throw new ArgumentOutOfRangeException(nameof(localX));
        if (localZ is < 0 or >= ChunkSection.Size) throw new ArgumentOutOfRangeException(nameof(localZ));

        var sectionY = Math.DivRem(worldY, ChunkSection.Size, out var localY);
        if (localY < 0)
        {
            sectionY--;
            localY += ChunkSection.Size;
        }

        collection.GetOrCreate(sectionY).SetBlock(localX, localY, localZ, block);
    }
}
