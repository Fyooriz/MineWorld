namespace MineWorld.Playable;

internal sealed class GameLoop
{
    private const float MaxDeltaTime = 0.05f;
    private const int SimulationStepsPerFrameCap = 4;

    private readonly IRenderer _renderer;
    private readonly InputState _input;
    private readonly VoxelWorld _world;
    private readonly PlayerController _player;
    private readonly string _savePath;

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
        var accumulator = 0f;
        const float fixedStep = 1f / 60f;

        while (!_renderer.ShouldClose)
        {
            var frameTime = MathF.Min(Raylib_cs.Raylib.GetFrameTime(), MaxDeltaTime);
            accumulator += frameTime;

            _input.Poll();

            var steps = 0;
            while (accumulator >= fixedStep && steps < SimulationStepsPerFrameCap)
            {
                _player.Update(fixedStep, _input);
                _world.StreamAround(_player.Position.X, _player.Position.Z);
                accumulator -= fixedStep;
                steps++;
            }

            // Avoid a spiral of death after a long hitch while preserving a stable simulation step.
            if (steps == SimulationStepsPerFrameCap && accumulator > fixedStep)
                accumulator = fixedStep;

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
