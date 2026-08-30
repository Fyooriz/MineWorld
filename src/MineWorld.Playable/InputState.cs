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
        MouseDelta = Raylib.GetMouseDelta();
        Forward = Raylib.IsKeyDown(KeyboardKey.W);
        Backward = Raylib.IsKeyDown(KeyboardKey.S);
        Left = Raylib.IsKeyDown(KeyboardKey.A);
        Right = Raylib.IsKeyDown(KeyboardKey.D);
        JumpPressed = Raylib.IsKeyPressed(KeyboardKey.Space);
        MinePressed = Raylib.IsMouseButtonPressed(MouseButton.Left);
        PlacePressed = Raylib.IsMouseButtonPressed(MouseButton.Right);
        SavePressed = Raylib.IsKeyPressed(KeyboardKey.F5);
    }

    public Vector2 ConsumeMouseDelta()
    {
        var delta = MouseDelta;
        MouseDelta = Vector2.Zero;
        return delta;
    }
}
