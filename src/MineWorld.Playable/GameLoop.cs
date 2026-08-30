using System.Diagnostics;
using Raylib_cs;
using System.Numerics;

namespace MineWorld.Playable;

/// <summary>Owns frame timing and orchestration; gameplay and rendering stay independently testable.</summary>
internal sealed class GameLoop
{
    private const float MaxDeltaSeconds = 0.05f;
    private const float FixedStepSeconds = 1f / 60f;

    private readonly IRenderer _renderer;
    private readonly InputState _input;
    private readonly VoxelWorld _world;
    private readonly PlayerController _player;
    private readonly string _savePath;
    private float _fixedAccumulator;

    public GameLoop(IRenderer renderer, InputState input, VoxelWorld world, PlayerController player, string savePath)
    {
        _renderer = renderer;
        _input = input;
        _world = world;
        _player = player;
        _savePath = savePath;
    }

    public void Run()
    {
        var stopwatch = Stopwatch.StartNew();
        var previous = stopwatch.Elapsed;

        while (!_renderer.ShouldClose)
        {
            var now = stopwatch.Elapsed;
            var dt = MathF.Min((float)(now - previous).TotalSeconds, MaxDeltaSeconds);
            previous = now;

            _input.Poll();
            _fixedAccumulator += dt;

            while (_fixedAccumulator >= FixedStepSeconds)
            {
                _player.Update(FixedStepSeconds, _input);
                _fixedAccumulator -= FixedStepSeconds;
            }

            _world.StreamAround(_player.Position.X, _player.Position.Z);

            if (_input.SavePressed)
                WorldPersistence.Save(_world, _savePath);

            _renderer.BeginFrame(_player.Position, _player.Position + _player.LookDirection);
            _renderer.RenderWorld(_world);
            _renderer.DrawHud(_player, _world);
            _renderer.EndFrame();
        }

        WorldPersistence.Save(_world, _savePath);
    }
}
