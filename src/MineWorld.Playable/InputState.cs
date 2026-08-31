using Raylib_cs;
using System.Numerics;

namespace MineWorld.Playable;

/// <summary>Frame-local input snapshot. Gameplay reads this instead of querying the window directly.</summary>
internal sealed class InputState
{
    public Vector2 MouseDelta { get; private set; }
    public bool Forward { get; private set; }
    public bool Backward { get; private set; }
    public bool Left { get; private set; }
    public bool Right { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool MinePressed { get; private set; }
    public bool PlacePressed { get; private set; }
    public bool SavePressed { get; private set; }

    public void Poll()
    {
        SetFrame(new InputFrame(
            Raylib.GetMouseDelta(),
            Raylib.IsKeyDown(KeyboardKey.W),
            Raylib.IsKeyDown(KeyboardKey.S),
            Raylib.IsKeyDown(KeyboardKey.A),
            Raylib.IsKeyDown(KeyboardKey.D),
            Raylib.IsKeyPressed(KeyboardKey.Space),
            Raylib.IsMouseButtonPressed(MouseButton.Left),
            Raylib.IsMouseButtonPressed(MouseButton.Right),
            Raylib.IsKeyPressed(KeyboardKey.F5)));
    }

    internal void SetFrame(InputFrame frame)
    {
        MouseDelta = frame.MouseDelta;
        Forward = frame.Forward;
        Backward = frame.Backward;
        Left = frame.Left;
        Right = frame.Right;
        JumpPressed = frame.JumpPressed;
        MinePressed = frame.MinePressed;
        PlacePressed = frame.PlacePressed;
        SavePressed = frame.SavePressed;
    }

    public Vector2 ConsumeMouseDelta()
    {
        var delta = MouseDelta;
        MouseDelta = Vector2.Zero;
        return delta;
    }
}

internal readonly record struct InputFrame(
    Vector2 MouseDelta,
    bool Forward,
    bool Backward,
    bool Left,
    bool Right,
    bool JumpPressed,
    bool MinePressed,
    bool PlacePressed,
    bool SavePressed)
{
    public static InputFrame Empty => new(Vector2.Zero, false, false, false, false, false, false, false, false);
}
