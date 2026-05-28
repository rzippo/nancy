using System;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics;

public class IntervalMutationTests
{
    [Fact]
    public void WithLower_ChangesBound()
    {
        var interval = Interval.Closed(2, 5);
        var result = interval.WithLower(3);
        Assert.Equal(Interval.Closed(3, 5), result);
    }

    [Fact]
    public void WithLower_SameValue_ReturnsEqual()
    {
        var interval = Interval.Closed(2, 5);
        Assert.Equal(interval, interval.WithLower(2));
    }

    [Fact]
    public void WithLower_GreaterThanUpper_Throws()
    {
        var interval = Interval.Closed(2, 5);
        Assert.Throws<ArgumentException>(() => interval.WithLower(10));
    }

    [Fact]
    public void WithLower_PreservesInclusivityFlags()
    {
        var interval = Interval.OpenClosed(2, 5);
        var result = interval.WithLower(3);
        Assert.False(result.IsLowerIncluded);
        Assert.True(result.IsUpperIncluded);
    }

    [Fact]
    public void WithUpper_ChangesBound()
    {
        var interval = Interval.Closed(2, 5);
        var result = interval.WithUpper(4);
        Assert.Equal(Interval.Closed(2, 4), result);
    }

    [Fact]
    public void WithUpper_SameValue_ReturnsEqual()
    {
        var interval = Interval.Closed(2, 5);
        Assert.Equal(interval, interval.WithUpper(5));
    }

    [Fact]
    public void WithUpper_LessThanLower_Throws()
    {
        var interval = Interval.Closed(2, 5);
        Assert.Throws<ArgumentException>(() => interval.WithUpper(0));
    }

    [Fact]
    public void WithUpper_PreservesInclusivityFlags()
    {
        var interval = Interval.ClosedOpen(2, 5);
        var result = interval.WithUpper(4);
        Assert.True(result.IsLowerIncluded);
        Assert.False(result.IsUpperIncluded);
    }

    [Fact]
    public void WithIsLowerIncluded_SetsFlag()
    {
        var interval = Interval.Closed(2, 5);
        var result = interval.WithIsLowerIncluded(false);
        Assert.Equal(Interval.OpenClosed(2, 5), result);
    }

    [Fact]
    public void WithIsLowerIncluded_SameFlag_ReturnsEqual()
    {
        var interval = Interval.Closed(2, 5);
        Assert.Equal(interval, interval.WithIsLowerIncluded(true));
    }

    [Fact]
    public void WithIsUpperIncluded_SetsFlag()
    {
        var interval = Interval.Closed(2, 5);
        var result = interval.WithIsUpperIncluded(false);
        Assert.Equal(Interval.ClosedOpen(2, 5), result);
    }

    [Fact]
    public void WithIsUpperIncluded_SameFlag_ReturnsEqual()
    {
        var interval = Interval.Closed(2, 5);
        Assert.Equal(interval, interval.WithIsUpperIncluded(true));
    }

    [Fact]
    public void Overlaps_Overlapping_ReturnsTrue()
    {
        Assert.True(
            Interval.Closed(1, 5).Overlaps(Interval.Closed(3, 7))
        );
    }

    [Fact]
    public void Overlaps_Disjoint_ReturnsFalse()
    {
        Assert.False(
            Interval.Closed(1, 2).Overlaps(Interval.Closed(3, 4))
        );
    }

    [Fact]
    public void Overlaps_TouchingBothInclusive_ReturnsTrue()
    {
        Assert.True(
            Interval.Closed(1, 2).Overlaps(Interval.Closed(2, 3))
        );
    }

    [Fact]
    public void Overlaps_TouchingBothExclusive_ReturnsFalse()
    {
        Assert.False(
            Interval.Open(1, 2).Overlaps(Interval.Open(2, 3))
        );
    }

    [Fact]
    public void SetEquals_EqualIntervals_ReturnsTrue()
    {
        Assert.True(
            Interval.Closed(1, 5).SetEquals(Interval.Closed(1, 5))
        );
    }

    [Fact]
    public void SetEquals_DifferentInclusivity_ReturnsFalse()
    {
        Assert.False(
            Interval.Closed(1, 5).SetEquals(Interval.ClosedOpen(1, 5))
        );
    }

    [Fact]
    public void SetEquals_DifferentBounds_ReturnsFalse()
    {
        Assert.False(
            Interval.Closed(1, 5).SetEquals(Interval.Closed(2, 6))
        );
    }
}
