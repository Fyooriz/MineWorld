using System;
using MineWorld.Client.Rendering;

namespace MineWorld.Client.Player;

public sealed class PlayerCameraBridge
{
    private readonly PlayerState _player;

    public PlayerCameraBridge(PlayerState player)
    {
        _player = player ?? throw new ArgumentNullException(nameof(player));
    }

    public CameraState BuildCamera(float fieldOfView, float aspectRatio, float nearClip = 0.05f, float farClip = 512f)
    {
        return new CameraState(
            _player.Position,
            _player.Yaw,
            Math.Clamp(_player.Pitch, -89f, 89f),
            fieldOfView,
            aspectRatio,
            nearClip,
            farClip);
    }
}
