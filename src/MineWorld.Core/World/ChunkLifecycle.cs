namespace MineWorld.Core.World;

public enum ChunkLifecycleState
{
    Unloaded,
    Requested,
    Generating,
    Loaded,
    Dirty,
    Meshing,
    Ready,
    Unloading
}

/// <summary>Explicit lifecycle gate for chunk ownership transitions.</summary>
public sealed class ChunkLifecycle
{
    public ChunkLifecycleState State { get; private set; } = ChunkLifecycleState.Unloaded;

    public void TransitionTo(ChunkLifecycleState next)
    {
        if (!CanTransition(State, next))
            throw new InvalidOperationException($"Invalid chunk lifecycle transition: {State} -> {next}.");

        State = next;
    }

    public static bool CanTransition(ChunkLifecycleState current, ChunkLifecycleState next)
        => current switch
        {
            ChunkLifecycleState.Unloaded => next == ChunkLifecycleState.Requested,
            ChunkLifecycleState.Requested => next is ChunkLifecycleState.Generating or ChunkLifecycleState.Unloading,
            ChunkLifecycleState.Generating => next is ChunkLifecycleState.Loaded or ChunkLifecycleState.Unloading,
            ChunkLifecycleState.Loaded => next is ChunkLifecycleState.Dirty or ChunkLifecycleState.Meshing or ChunkLifecycleState.Unloading,
            ChunkLifecycleState.Dirty => next is ChunkLifecycleState.Meshing or ChunkLifecycleState.Unloading,
            ChunkLifecycleState.Meshing => next is ChunkLifecycleState.Ready or ChunkLifecycleState.Dirty or ChunkLifecycleState.Unloading,
            ChunkLifecycleState.Ready => next is ChunkLifecycleState.Dirty or ChunkLifecycleState.Unloading,
            ChunkLifecycleState.Unloading => next == ChunkLifecycleState.Unloaded,
            _ => false
        };
}
