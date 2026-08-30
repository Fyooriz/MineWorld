using Raylib_cs;
using System.Numerics;

namespace MineWorld.Playable;

/// <summary>GPU-backed renderer boundary with chunk-level visibility delegated to the world renderer.</summary>
internal sealed class RaylibRenderer : IRenderer
{
    private readonly Camera3D _camera;
    public RaylibRenderer(int width, int height, string title)
    {
        Width = width; Height = height;
        Raylib.InitWindow(width, height, title);
        Raylib.SetTargetFPS(120); Raylib.DisableCursor();
        _camera = new Camera3D(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, 70f, CameraProjection.Perspective);
    }
    public int Width { get; }
    public int Height { get; }
    public bool ShouldClose => Raylib.WindowShouldClose();
    public void BeginFrame(Vector3 position, Vector3 target)
    {
        _camera.Position = position; _camera.Target = target;
        Raylib.BeginDrawing(); Raylib.ClearBackground(new Color(145, 205, 245, 255)); Raylib.BeginMode3D(_camera);
    }
    public void RenderWorld(VoxelWorld world) => world.Draw(_camera.Position, _camera.Target, Width, Height);
    public void DrawHud(PlayerController player, VoxelWorld world)
    {
        Raylib.EndMode3D();
        Raylib.DrawText("MINEWORLD P0 — CHUNK MESH + FRUSTUM CULLING", 20, 18, 24, Color.White);
        Raylib.DrawText("WASD move | Mouse look | Space jump | LMB mine | RMB place | F5 save", 20, 50, 18, Color.White);
        Raylib.DrawText($"XYZ {player.Position.X:0.0} {player.Position.Y:0.0} {player.Position.Z:0.0} | Seed {world.Seed} | Chunks {world.LoadedChunkCount} | Meshes {world.CachedMeshCount}", 20, 76, 18, Color.White);
        Raylib.DrawCircle(Width / 2, Height / 2, 3, Color.White);
    }
    public void EndFrame() => Raylib.EndDrawing();
    public void Dispose() { Raylib.EnableCursor(); Raylib.CloseWindow(); }
}
