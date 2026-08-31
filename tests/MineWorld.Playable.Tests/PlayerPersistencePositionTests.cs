using System.Numerics;
using MineWorld.Core.Inventory;
using MineWorld.Core.Player;

namespace MineWorld.Playable.Tests;

public sealed class PlayerPersistencePositionTests
{
    [Fact]
    public void CaptureAndRestorePreservesPlayerPositionWhenProvided()
    {
        var player = new PlayerState
        {
            Name = "Positioned",
            Health = 15.5f
        };
        var position = new Vector3(-31.25f, 27.5f, 48.75f);

        var saved = PlayerPersistence.Capture(player, position);
        var restored = PlayerPersistence.Restore(saved);

        Assert.NotNull(saved.Position);
        Assert.Equal(position.X, saved.Position!.X);
        Assert.Equal(position.Y, saved.Position.Y);
        Assert.Equal(position.Z, saved.Position.Z);
        Assert.Equal(player.Id, restored.Id);
        Assert.Equal(player.Name, restored.Name);
        Assert.Equal(player.Health, restored.Health);
    }

    [Fact]
    public void RestoreRejectsNonFiniteSavedPosition()
    {
        var invalid = new SavedPlayerState(
            Guid.NewGuid(),
            "Player",
            20f,
            1,
            new ItemStack[] { default },
            new SavedPlayerPosition(float.NaN, 0f, 0f));

        Assert.Throws<InvalidDataException>(() => PlayerPersistence.Restore(invalid));
    }
}
