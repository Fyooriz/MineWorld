using Raylib_cs;
using System.Numerics;

namespace MineWorld.Playable;

internal sealed class PlayerController
{
    private const float Speed = 6f;
    private const float JumpVelocity = 8.5f;
    private const float Gravity = 22f;
    private const float EyeHeight = 1.7f;

    public Vector3 Position { get; private set; }
    public Vector3 LookDirection { get; private set; } = Vector3.UnitZ;

    private readonly VoxelWorld _world;
    private float _yaw;
    private float _pitch;
    private float _verticalVelocity;

    public PlayerController(VoxelWorld world)
    {
        _world = world;
        Position = new Vector3(0.5f, world.GetSurfaceHeight(0, 0) + 2f, 0.5f);
    }

    public void Update(float dt, InputState input)
    {
        UpdateLook(input);
        UpdateMovement(dt, input);
        UpdateInteraction(input);
    }

    private void UpdateLook(InputState input)
    {
        _yaw -= input.MouseDeltaX * 0.0025f;
        _pitch = Math.Clamp(_pitch - input.MouseDeltaY * 0.0025f, -1.5f, 1.5f);

        LookDirection = Vector3.Normalize(new Vector3(
            MathF.Cos(_pitch) * MathF.Sin(_yaw),
            MathF.Sin(_pitch),
            MathF.Cos(_pitch) * MathF.Cos(_yaw)));
    }

    private void UpdateMovement(float dt, InputState input)
    {
        var forward = Vector3.Normalize(new Vector3(LookDirection.X, 0, LookDirection.Z));
        var right = new Vector3(forward.Z, 0, -forward.X);
        var move = Vector3.Zero;

        if (input.Forward) move += forward;
        if (input.Backward) move -= forward;
        if (input.Right) move += right;
        if (input.Left) move -= right;

        if (move.LengthSquared() > 0)
            move = Vector3.Normalize(move) * Speed * dt;

        var blockX = (int)MathF.Floor(Position.X);
        var blockZ = (int)MathF.Floor(Position.Z);
        var ground = _world.GetSurfaceHeight(blockX, blockZ) + EyeHeight;
        var grounded = Position.Y <= ground + 0.02f;

        if (grounded && input.JumpPressed)
            _verticalVelocity = JumpVelocity;

        _verticalVelocity -= Gravity * dt;
        var next = Position + move;
        next.Y += _verticalVelocity * dt;

        if (next.Y < ground)
        {
            next.Y = ground;
            _verticalVelocity = 0;
        }

        Position = next;
    }

    private void UpdateInteraction(InputState input)
    {
        var ray = new Ray3D(Position, LookDirection);
        if (input.MinePressed)
            _world.Mine(ray);
        if (input.PlacePressed)
            _world.Place(ray);
    }
}
