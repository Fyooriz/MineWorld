using MineWorld.Core.World;

namespace MineWorld.Tests;

public sealed class ChunkLifecycleTests
{
    [Fact]
    public void HappyPathFollowsAuthoritativeLifecycle()
    {
        var lifecycle = new ChunkLifecycle();

        lifecycle.TransitionTo(ChunkLifecycleState.Requested);
        lifecycle.TransitionTo(ChunkLifecycleState.Generating);
        lifecycle.TransitionTo(ChunkLifecycleState.Loaded);
        lifecycle.TransitionTo(ChunkLifecycleState.Dirty);
        lifecycle.TransitionTo(ChunkLifecycleState.Meshing);
        lifecycle.TransitionTo(ChunkLifecycleState.Ready);
        lifecycle.TransitionTo(ChunkLifecycleState.Unloading);
        lifecycle.TransitionTo(ChunkLifecycleState.Unloaded);

        Assert.Equal(ChunkLifecycleState.Unloaded, lifecycle.State);
    }

    [Fact]
    public void InvalidTransitionIsRejected()
    {
        var lifecycle = new ChunkLifecycle();

        Assert.Throws<InvalidOperationException>(() =>
            lifecycle.TransitionTo(ChunkLifecycleState.Ready));
    }

    [Theory]
    [InlineData(ChunkLifecycleState.Requested, ChunkLifecycleState.Unloading)]
    [InlineData(ChunkLifecycleState.Generating, ChunkLifecycleState.Unloading)]
    [InlineData(ChunkLifecycleState.Ready, ChunkLifecycleState.Dirty)]
    [InlineData(ChunkLifecycleState.Meshing, ChunkLifecycleState.Dirty)]
    public void CancellationOrInvalidationPathsRemainExplicit(ChunkLifecycleState current, ChunkLifecycleState next)
        => Assert.True(ChunkLifecycle.CanTransition(current, next));

    [Theory]
    [InlineData(ChunkLifecycleState.Unloaded, ChunkLifecycleState.Loaded)]
    [InlineData(ChunkLifecycleState.Unloaded, ChunkLifecycleState.Ready)]
    [InlineData(ChunkLifecycleState.Requested, ChunkLifecycleState.Ready)]
    [InlineData(ChunkLifecycleState.Generating, ChunkLifecycleState.Dirty)]
    [InlineData(ChunkLifecycleState.Loaded, ChunkLifecycleState.Requested)]
    [InlineData(ChunkLifecycleState.Dirty, ChunkLifecycleState.Loaded)]
    [InlineData(ChunkLifecycleState.Meshing, ChunkLifecycleState.Loaded)]
    [InlineData(ChunkLifecycleState.Ready, ChunkLifecycleState.Generating)]
    [InlineData(ChunkLifecycleState.Unloading, ChunkLifecycleState.Requested)]
    public void AdversarialInvalidTransitionsAreRejected(ChunkLifecycleState current, ChunkLifecycleState next)
    {
        var lifecycle = CreateAt(current);

        Assert.False(ChunkLifecycle.CanTransition(current, next));
        Assert.Throws<InvalidOperationException>(() => lifecycle.TransitionTo(next));
        Assert.Equal(current, lifecycle.State);
    }

    private static ChunkLifecycle CreateAt(ChunkLifecycleState target)
    {
        var lifecycle = new ChunkLifecycle();
        foreach (var state in target switch
        {
            ChunkLifecycleState.Unloaded => Array.Empty<ChunkLifecycleState>(),
            ChunkLifecycleState.Requested => new[] { ChunkLifecycleState.Requested },
            ChunkLifecycleState.Generating => new[] { ChunkLifecycleState.Requested, ChunkLifecycleState.Generating },
            ChunkLifecycleState.Loaded => new[] { ChunkLifecycleState.Requested, ChunkLifecycleState.Generating, ChunkLifecycleState.Loaded },
            ChunkLifecycleState.Dirty => new[] { ChunkLifecycleState.Requested, ChunkLifecycleState.Generating, ChunkLifecycleState.Loaded, ChunkLifecycleState.Dirty },
            ChunkLifecycleState.Meshing => new[] { ChunkLifecycleState.Requested, ChunkLifecycleState.Generating, ChunkLifecycleState.Loaded, ChunkLifecycleState.Meshing },
            ChunkLifecycleState.Ready => new[] { ChunkLifecycleState.Requested, ChunkLifecycleState.Generating, ChunkLifecycleState.Loaded, ChunkLifecycleState.Meshing, ChunkLifecycleState.Ready },
            ChunkLifecycleState.Unloading => new[] { ChunkLifecycleState.Requested, ChunkLifecycleState.Generating, ChunkLifecycleState.Loaded, ChunkLifecycleState.Unloading },
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        })
        {
            lifecycle.TransitionTo(state);
        }

        return lifecycle;
    }
}
