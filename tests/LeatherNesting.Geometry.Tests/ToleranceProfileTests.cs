using LeatherNesting.Geometry;
using Xunit;

namespace LeatherNesting.Geometry.Tests;

public sealed class ToleranceProfileTests
{
    [Fact]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-TOL-001")]
    public void Default_profile_has_positive_values()
    {
        var profile = ToleranceProfile.Default;

        Assert.True(profile.ImportSnapToleranceMm > 0);
        Assert.True(profile.TopologyToleranceMm > 0);
        Assert.True(profile.FlattenChordToleranceMm > 0);
        Assert.True(profile.CollisionToleranceMm > 0);
        Assert.True(profile.ExportRoundTripToleranceMm > 0);
    }

    [Theory]
    [Trait("Stage", "2")]
    [Trait("TestId", "P2-TOL-002")]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Zero_or_negative_tolerance_throws(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ToleranceProfile { ImportSnapToleranceMm = value });
    }
}