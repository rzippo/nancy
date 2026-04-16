using System;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics;

public class IntervalExceptionTests
{
    [Fact]
    public void IsSubsetOf_EmptyInterval_ThrowsEmptyIntervalException()
    {
        var emptyInterval = Interval.Open(5, 5);
        var otherInterval = Interval.Closed(1, 10);
        
        var exception = Assert.Throws<EmptyIntervalException>(() => emptyInterval.IsSubsetOf(otherInterval));
        Assert.Contains("5", exception.Message);
    }

    [Fact]
    public void IsSubsetOf_EmptyIntervalOnBoth_ThrowsEmptyIntervalException()
    {
        var empty1 = Interval.Open(3, 3);
        var empty2 = Interval.Open(5, 5);
        
        var exception = Assert.Throws<EmptyIntervalException>(() => empty1.IsSubsetOf(empty2));
    }

    [Fact]
    public void IsSupersetOf_EmptyInterval_ThrowsEmptyIntervalException()
    {
        var emptyInterval = Interval.Open(5, 5);
        var otherInterval = Interval.Closed(1, 10);
        
        var exception = Assert.Throws<EmptyIntervalException>(() => emptyInterval.IsSupersetOf(otherInterval));
    }

    [Fact]
    public void IsSubsetOf_ClosedSingleton_Succeeds()
    {
        var singleton = Interval.Closed(5, 5);
        var other = Interval.Closed(1, 10);
        
        Assert.True(singleton.IsSubsetOf(other));
    }

    [Fact]
    public void IsSubsetOf_OpenSingleton_CannotBeChecked()
    {
        var openSingleton = Interval.Open(5, 5);
        var other = Interval.Closed(1, 10);
        
        Assert.True(openSingleton.IsEmpty);
        Assert.Throws<EmptyIntervalException>(() => openSingleton.IsSubsetOf(other));
    }

    [Fact]
    public void IsSubsetOf_ProperSubset()
    {
        var inner = Interval.Open(2, 8);
        var outer = Interval.Closed(1, 10);
        
        Assert.True(inner.IsSubsetOf(outer));
    }

    [Fact]
    public void IsSubsetOf_NotSubset_DifferentRanges()
    {
        var a = Interval.Closed(1, 5);
        var b = Interval.Closed(6, 10);
        
        Assert.False(a.IsSubsetOf(b));
    }

    [Fact]
    public void IsSubsetOf_SameInterval()
    {
        var interval = Interval.Closed(1, 10);
        
        Assert.True(interval.IsSubsetOf(interval));
    }

    [Fact]
    public void IsSubsetOf_BoundaryCases()
    {
        var a = Interval.ClosedOpen(1, 5);  // [1, 5)
        var b = Interval.Closed(1, 5);       // [1, 5]
        
        Assert.True(a.IsSubsetOf(b));  // [1,5) is a subset of [1,5]
        Assert.False(b.IsSubsetOf(a)); // [1,5] is not a subset of [1,5)
    }
}
