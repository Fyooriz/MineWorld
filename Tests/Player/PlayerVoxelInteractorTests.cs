using MineWorld.Player;
using Xunit;

namespace MineWorld.Tests.Player;

public sealed class PlayerVoxelInteractorTests
{
    [Fact]
    public void Vector3NormalizeProducesUnitDirection()
    {
        var value = Vector3.Normalize(new Vector3(3, 4, 0));
        Assert.Equal(0.6f, value.X, 3);
        Assert.Equal(0.8f, value.Y, 3);
        Assert.Equal(0f, value.Z, 3);
    }

    [Fact]
    public void Vector3NormalizeHandlesZeroVector()
    {
        var value = Vector3.Normalize(new Vector3(0, 0, 0));
        Assert.Equal(0f, value.X);
        Assert.Equal(0f, value.Y);
        Assert.Equal(0f, value.Z);
    }
}
