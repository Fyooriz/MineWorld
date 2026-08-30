using Raylib_cs;
using System.Numerics;

namespace MineWorld.Playable;

internal sealed class PlayerController
{
    public Vector3 Position { get; private set; }
    public Vector3 LookDirection { get; private set; } = Vector3.UnitZ;
    private readonly VoxelWorld _world;
    private float _yaw;
    private float _pitch;
    private float _verticalVelocity;

    public PlayerController(VoxelWorld world)
    {
        _world = world;
        Position = new Vector3(.5f, world.GetSurfaceHeight(0, 0) + 2f, .5f);
    }

    public void Update(float dt)
    {
        var mouse = Raylib.GetMouseDelta();
        _yaw -= mouse.X * .0025f;
        _pitch = Math.Clamp(_pitch - mouse.Y * .0025f, -1.5f, 1.5f);
        LookDirection = Vector3.Normalize(new Vector3(
            MathF.Cos(_pitch) * MathF.Sin(_yaw),
            MathF.Sin(_pitch),
            MathF.Cos(_pitch) * MathF.Cos(_yaw)));

        var forward = Vector3.Normalize(new Vector3(LookDirection.X, 0, LookDirection.Z));
        var right = new Vector3(forward.Z, 0, -forward.X);
        var move = Vector3.Zero;
        if (Raylib.IsKeyDown(KeyboardKey.W)) move += forward;
        if (Raylib.IsKeyDown(KeyboardKey.S)) move -= forward;
        if (Raylib.IsKeyDown(KeyboardKey.D)) move += right;
        if (Raylib.IsKeyDown(KeyboardKey.A)) move -= right;
        if (move.LengthSquared() > 0) move = Vector3.Normalize(move) * 6f * dt;

        var ground = _world.GetSurfaceHeight((int)MathF.Floor(Position.X), (int)MathF.Floor(Position.Z)) + 1.7f;
        if (Position.Y <= ground + .02f && Raylib.IsKeyPressed(KeyboardKey.Space)) _verticalVelocity = 8.5f;
        _verticalVelocity -= 22f * dt;
        var next = Position + move;
        next.Y += _verticalVelocity * dt;
        if (next.Y < ground) { next.Y = ground; _verticalVelocity = 0; }
        Position = next;

        var ray = new Ray3D(Position, LookDirection);
        if (Raylib.IsMouseButtonPressed(MouseButton.Left)) _world.Mine(ray);
        if (Raylib.IsMouseButtonPressed(MouseButton.Right)) _world.Place(ray);
    }
}
