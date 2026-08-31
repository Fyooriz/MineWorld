using MineWorld.Core.Player;
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
    public Vector3 LookDirection { get; private set; }
    public PlayerState State { get; }

    private readonly VoxelWorld _world;
    private readonly PlayerActionLayer _actions;
    private float _yaw;
    private float _pitch;
    private float _verticalVelocity;

    public PlayerController(
        VoxelWorld world,
        PlayerState? state = null,
        Vector3? initialLookDirection = null,
        PlayerActionLayer? actions = null,
        Vector3? initialPosition = null)
    {
        _world = world;
        State = state ?? new PlayerState();
        _actions = actions ?? new PlayerActionLayer();
        Position = initialPosition ?? new Vector3(0.5f, world.GetSurfaceHeight(0, 0) + 2f, 0.5f);
        if (!float.IsFinite(Position.X) || !float.IsFinite(Position.Y) || !float.IsFinite(Position.Z))
            throw new ArgumentException("Initial player position must contain finite coordinates.", nameof(initialPosition));

        var direction = initialLookDirection is { } initial && initial.LengthSquared() > 0.0001f
            ? Vector3.Normalize(initial)
            : Vector3.UnitZ;
        _yaw = MathF.Atan2(direction.X, direction.Z);
        _pitch = Math.Clamp(MathF.Asin(Math.Clamp(direction.Y, -1f, 1f)), -1.5f, 1.5f);
        LookDirection = Vector3.Normalize(new Vector3(
            MathF.Cos(_pitch) * MathF.Sin(_yaw),
            MathF.Sin(_pitch),
            MathF.Cos(_pitch) * MathF.Cos(_yaw)));
    }

    public void Update(float dt, InputState input, Vector2 mouseDelta)
    {
        ArgumentNullException.ThrowIfNull(input);
        UpdateLook(mouseDelta);
        UpdateMovement(dt, input);
        UpdateInteraction(input);
    }

    private void UpdateLook(Vector2 mouse)
    {
        _yaw -= mouse.X * 0.0025f;
        _pitch = Math.Clamp(_pitch - mouse.Y * 0.0025f, -1.5f, 1.5f);

        LookDirection = Vector3.Normalize(new Vector3(
            MathF.Cos(_pitch) * MathF.Sin(_yaw),
            MathF.Sin(_pitch),
            MathF.Cos(_pitch) * MathF.Cos(_yaw)));
    }

    private void UpdateMovement(float dt, InputState input)
    {
        var flatLook = new Vector3(LookDirection.X, 0, LookDirection.Z);
        var forward = flatLook.LengthSquared() > 0.0001f ? Vector3.Normalize(flatLook) : Vector3.UnitZ;
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
        var ray = new Ray(Position, LookDirection);
        if (input.MinePressed)
            _world.Mine(ray, State.Inventory);
        if (input.CraftPressed)
            _actions.TryCraft(State);
        if (input.PlacePressed)
            _world.Place(ray, State.Inventory);
    }
}
