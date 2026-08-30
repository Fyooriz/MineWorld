using Raylib_cs;

namespace MineWorld.Playable;

/// <summary>Frame-local input snapshot. Keeps gameplay code independent from raw window polling.</summary>
internal sealed class InputState
{
    public float MouseDeltaX { get; private set; }
    public float MouseDeltaY { get; private set; }
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
        var mouse = Raylib.GetMouseDelta();
        MouseDeltaX = mouse.X;
        MouseDeltaY = mouse.Y;
        Forward = Raylib.IsKeyDown(KeyboardKey.W);
        Backward = Raylib.IsKeyDown(KeyboardKey.S);
        Left = Raylib.IsKeyDown(KeyboardKey.A);
        Right = Raylib.IsKeyDown(KeyboardKey.D);
        JumpPressed = Raylib.IsKeyPressed(KeyboardKey.Space);
        MinePressed = Raylib.IsMouseButtonPressed(MouseButton.Left);
        PlacePressed = Raylib.IsMouseButtonPressed(MouseButton.Right);
        SavePressed = Raylib.IsKeyPressed(KeyboardKey.F5);
    }
}
