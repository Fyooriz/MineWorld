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
}
