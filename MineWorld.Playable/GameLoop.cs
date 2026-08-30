namespace MineWorld.Playable;

internal sealed class GameLoop
{
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
        while (!_renderer.ShouldClose)
        {
            var dt = MathF.Min(Raylib_cs.Raylib.GetFrameTime(), 0.05f);
            _input.Poll();
            _player.Update(dt, _input);
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
