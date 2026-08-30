using System.Numerics;

namespace MineWorld.Client.Player;

public sealed class PlayerState
{
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public bool Grounded { get; set; }

    public PlayerState(Vector3 spawnPosition)
    {
        Position = spawnPosition;
    }
}
