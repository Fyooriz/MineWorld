using MineWorld.Core.World;

namespace MineWorld.Tests;

public sealed class ChunkMesherTests
{
    [Fact]
    public void MissingBoundaryNeighborProducesFace()
    {
        var section = new ChunkSection(0);
        section.SetBlock(0, 0, 0, new BlockId(1));

        var faces = ChunkMesher.Build(section);

        Assert.Contains(faces, f => f.Direction == FaceDirection.West);
        Assert.Equal(6, faces.Count);
    }

    [Fact]
    public void SolidBoundaryNeighborSuppressesFace()
    {
        var section = new ChunkSection(0);
        var stone = new BlockId(1);
        section.SetBlock(0, 0, 0, stone);

        var faces = ChunkMesher.Build(section, (x, y, z) =>
            x == -1 && y == 0 && z == 0 ? stone : BlockId.Air);

        Assert.DoesNotContain(faces, f => f.Direction == FaceDirection.West);
        Assert.Equal(5, faces.Count);
    }
}
