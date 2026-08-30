using System;
using System.Numerics;
using System.Runtime.InteropServices;
using MineWorld.World.Meshing;
using Raylib_cs;

namespace MineWorld.Client.Rendering;

/// <summary>
/// Concrete P0 renderer. Raylib owns the graphics context while the voxel mesh remains engine-owned.
/// All GPU uploads and draws happen on the render thread.
/// </summary>
public sealed class RaylibVoxelRenderBackend : IRenderBackend, IDisposable
{
    private readonly Camera3D _camera;
    private readonly Material _material;
    private Mesh? _mesh;

    public RaylibVoxelRenderBackend(int width, int height, string title = "MineWorld")
    {
        if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));

        Raylib.InitWindow(width, height, title);
        Raylib.SetTargetFPS(120);
        _camera = new Camera3D(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, 70f, CameraProjection.Perspective);
        _material = Raylib.LoadMaterialDefault();
    }

    public void BeginFrame(CameraState camera)
    {
        _camera.Position = camera.Position;
        var forward = Forward(camera.Yaw, camera.Pitch);
        _camera.Target = camera.Position + forward;
        _camera.Fovy = camera.FieldOfView;
        _camera.Projection = CameraProjection.Perspective;

        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(145, 205, 245, 255));
        Raylib.BeginMode3D(_camera);
    }

    public void Submit(VoxelMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        if (mesh.Vertices.Count == 0 || mesh.Indices.Count == 0) return;
        if (mesh.Vertices.Count > ushort.MaxValue)
            throw new InvalidOperationException("P0 renderer uses 16-bit indices; split the mesh before upload.");

        UnloadMesh();

        var vertices = new float[mesh.Vertices.Count * 3];
        var normals = new float[mesh.Vertices.Count * 3];
        for (var i = 0; i < mesh.Vertices.Count; i++)
        {
            var v = mesh.Vertices[i];
            var offset = i * 3;
            vertices[offset] = v.X;
            vertices[offset + 1] = v.Y;
            vertices[offset + 2] = v.Z;
            normals[offset] = v.Nx;
            normals[offset + 1] = v.Ny;
            normals[offset + 2] = v.Nz;
        }

        var indices = new ushort[mesh.Indices.Count];
        for (var i = 0; i < indices.Length; i++)
            indices[i] = checked((ushort)mesh.Indices[i]);

        var gpuMesh = new Mesh
        {
            VertexCount = mesh.Vertices.Count,
            TriangleCount = mesh.Indices.Count / 3,
            Vertices = vertices,
            Normals = normals,
            Indices = indices
        };

        Raylib.UploadMesh(ref gpuMesh, false);
        _mesh = gpuMesh;
        Raylib.DrawMesh(gpuMesh, _material, Matrix4x4.Identity);
    }

    public void EndFrame()
    {
        Raylib.EndMode3D();
        Raylib.EndDrawing();
    }

    public void Dispose()
    {
        UnloadMesh();
        Raylib.UnloadMaterial(_material);
        Raylib.CloseWindow();
    }

    private void UnloadMesh()
    {
        if (!_mesh.HasValue) return;
        var mesh = _mesh.Value;
        Raylib.UnloadMesh(mesh);
        _mesh = null;
    }

    private static Vector3 Forward(float yaw, float pitch)
    {
        var cp = MathF.Cos(pitch);
        return Vector3.Normalize(new Vector3(
            MathF.Sin(yaw) * cp,
            MathF.Sin(pitch),
            MathF.Cos(yaw) * cp));
    }
}
