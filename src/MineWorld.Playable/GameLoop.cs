using System.Diagnostics;
using System.Numerics;
using MineWorld.Core.Entities;

namespace MineWorld.Playable;

/// <summary>Owns frame timing and orchestration; gameplay, rendering, and entity simulation stay independently testable.</summary>
internal sealed class GameLoop
{
    private const float MaxDeltaSeconds = 0.05f;
    private const float FixedStepSeconds = 1f / 60f;

    private readonly IRenderer _renderer;
    private readonly InputState _input;
    private readonly VoxelWorld _world;
    private readonly PlayerController _player;
    private readonly EntityRuntime _entityRuntime;
    private readonly string _savePath;
    private float _fixedAccumulator;
    private long _simulationTick;

    public GameLoop(
        IRenderer renderer,
        InputState input,
        VoxelWorld world,
        PlayerController player,
        string savePath,
        EntityRuntime? entityRuntime = null)
    {
        _renderer = renderer;
        _input = input;
        _world = world;
        _player = player;
        _savePath = savePath;
        _entityRuntime = entityRuntime ?? new EntityRuntime();
    }

    public void Run(int? maxFrames = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var previous = stopwatch.Elapsed;
        var frames = 0;

        while (!_renderer.ShouldClose && (!maxFrames.HasValue || frames < maxFrames.Value))
        {
            var now = stopwatch.Elapsed;
            var dt = MathF.Min((float)(now - previous).TotalSeconds, MaxDeltaSeconds);
            previous = now;

            _input.Poll();
            _fixedAccumulator += dt;
            var mouseDelta = _input.ConsumeMouseDelta();
            var firstSimulationStep = true;

            while (_fixedAccumulator >= FixedStepSeconds)
            {
                var context = new EntityTickContext(++_simulationTick, FixedStepSeconds);
                _player.Update(FixedStepSeconds, _input, firstSimulationStep ? mouseDelta : Vector2.Zero);
                _entityRuntime.Tick(context);
                firstSimulationStep = false;
                _fixedAccumulator -= FixedStepSeconds;
            }

            _world.StreamAround(_player.Position.X, _player.Position.Z);

            if (_input.SavePressed)
                WorldPersistence.Save(_world, _savePath, GetEntities());

            _renderer.BeginFrame(_player.Position, _player.Position + _player.LookDirection);
            _renderer.RenderWorld(_world);
            _renderer.DrawHud(_player, _world);
            _renderer.EndFrame();
            frames++;
        }

        WorldPersistence.Save(_world, _savePath, GetEntities());
    }

    private IReadOnlyList<IEntity> GetEntities()
    {
        var entities = new List<IEntity>(_entityRuntime.Count);
        foreach (var id in GetActiveEntityIds())
        {
            if (_entityRuntime.TryGet(id, out var entity))
                entities.Add(entity);
        }

        return entities;
    }

    private IEnumerable<EntityId> GetActiveEntityIds()
    {
        return _activeIdsCache.ToArray();
    }

    private readonly List<EntityId> _activeIdsCache = [];
}
