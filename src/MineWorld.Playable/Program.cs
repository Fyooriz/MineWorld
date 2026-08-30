using Raylib_cs;
using System.Numerics;

namespace MineWorld.Playable;

internal static class Program
{
    private const string SavePath = "saves/p0-world.json";

    public static void Main()
    {
        Raylib.InitWindow(1280, 720, "MineWorld P0");
        Raylib.SetTargetFPS(120);
        Raylib.DisableCursor();

        var world = WorldPersistence.Load(SavePath, renderDistance: 3);
        var player = new PlayerController(world);
        var camera = new Camera3D(
            player.Position,
            player.Position + Vector3.UnitZ,
            Vector3.UnitY,
            70f,
            CameraProjection.Perspective);

        while (!Raylib.WindowShouldClose())
        {
            var dt = MathF.Min(Raylib.GetFrameTime(), 0.05f);
            player.Update(dt);
            world.StreamAround(player.Position.X, player.Position.Z);

            if (Raylib.IsKeyPressed(KeyboardKey.F5))
                WorldPersistence.Save(world, SavePath);

            camera.Position = player.Position;
            camera.Target = player.Position + player.LookDirection;

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(145, 205, 245, 255));
            Raylib.BeginMode3D(camera);
            world.Draw();
            Raylib.EndMode3D();

            Raylib.DrawText("MINEWORLD P0 — PLAYABLE VOXEL SLICE", 20, 18, 24, Color.White);
            Raylib.DrawText("WASD move | Mouse look | Space jump | LMB mine | RMB place | F5 save", 20, 50, 18, Color.White);
            Raylib.DrawText(
                $"XYZ {player.Position.X:0.0} {player.Position.Y:0.0} {player.Position.Z:0.0} | Seed {world.Seed} | Chunks {world.LoadedChunkCount} | Edits {world.BlockOverrides.Count}",
                20,
                76,
                18,
                Color.White);
            Raylib.DrawCircle(640, 360, 3, Color.White);
            Raylib.EndDrawing();
        }

        WorldPersistence.Save(world, SavePath);
        Raylib.EnableCursor();
        Raylib.CloseWindow();
    }
}
