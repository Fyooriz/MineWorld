namespace MineWorld.Core.World;

/// <summary>
/// Vertical collection of 16x16x16 sections. Section Y is an integer index, allowing
/// the world height to evolve without changing the voxel storage primitive.
/// </summary>
public sealed class ChunkSectionCollection
{
    private readonly Dictionary<int, ChunkSection> _sections = new();

    public ChunkSection GetOrCreate(int sectionY)
    {
        if (!_sections.TryGetValue(sectionY, out var section))
        {
            section = new ChunkSection(sectionY);
            _sections.Add(sectionY, section);
        }

        return section;
    }

    public bool TryGet(int sectionY, out ChunkSection? section) => _sections.TryGetValue(sectionY, out section);

    public IEnumerable<ChunkSection> Sections => _sections.Values.OrderBy(s => s.SectionY);
}
