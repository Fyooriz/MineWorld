using System.Numerics;
using MineWorld.Playable;
using Xunit;

namespace MineWorld.Tests;

public sealed class ChunkVisibilityPolicyTests
{
    [Fact]
    public void DistanceRejectsFarChunk()
    {
        var policy = new ChunkVisibilityPolicy(2);

        Assert.False(policy.IsVisible(new ChunkKey(10, 0), Vector3.Zero, 16));
    }

    [Fact]
    public void DistanceAcceptsNearbyChunk()
    {
        var policy = new ChunkVisibilityPolicy(2);

        Assert.True(policy.IsVisible(new ChunkKey(1, 0), Vector3.Zero, 16));
    }

    [Fact]
    public void ForwardFovRejectsChunkBehindCamera()
    {
        var policy = new ChunkVisibilityPolicy(8);

        Assert.False(policy.IsVisible(
            new ChunkKey(2, 0),
            Vector3.Zero,
            -Vector3.UnitZ,
            16,
            90f));
    }

    [Fact]
    public void ForwardFovAcceptsChunkAheadOfCamera()
    {
        var policy = new ChunkVisibilityPolicy(8);

        Assert.True(policy.IsVisible(
            new ChunkKey(2, 0),
            Vector3.Zero,
            Vector3.UnitX,
            16,
            90f));
    }

    [Fact]
    public void FrustumAcceptsChunkInsideView()
    {
        var policy = new ChunkVisibilityPolicy(32);
        var view = Matrix4x4.CreateLookAt(new Vector3(0, 16, 32), Vector3.Zero, Vector3.UnitY);
        var projection = Matrix4x4.CreateOrthographic(64, 64, 0.1f, 128f);
        var frustum = Frustum3D.FromViewProjection(view * projection);

        Assert.True(policy.IsVisible(new ChunkKey(0, 0), frustum, 16, 64));
    }

    [Fact]
    public void FrustumRejectsChunkOutsideView()
    {
        var policy = new ChunkVisibilityPolicy(128);
        var view = Matrix4x4.CreateLookAt(new Vector3(0, 16, 32), Vector3.Zero, Vector3.UnitY);
        var projection = Matrix4x4.CreateOrthographic(64, 64, 0.1f, 128f);
        var frustum = Frustum3D.FromViewProjection(view * projection);

        Assert.False(policy.IsVisible(new ChunkKey(100, 0), frustum, 16, 64));
    }
}
