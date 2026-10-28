using System;
using System.Threading;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveKnownPropertyShortcuts
{
    [Fact]
    public void ToLeftContinuousReusesAnInputKnownToSatisfyTheProperty()
    {
        var source = Curve.Zero();
        Assert.True(source.IsLeftContinuous);
        Assert.Same(source, source.ToLeftContinuous());
    }

    [Fact]
    public void ToLeftContinuousDoesNotInitializeAnUnknownPredicate()
    {
        var source = Curve.Zero();
        source._isLeftContinuous = new Lazy<bool>(() => throw new InvalidOperationException("Predicate was evaluated."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        var result = source.ToLeftContinuous();
        Assert.True(Curve.Equivalent(source, result));
        Assert.False(source._isLeftContinuous.IsValueCreated);
    }

    [Fact]
    public void ToLeftContinuousDoesNotReadAFaultedPredicate()
    {
        var source = Curve.Zero();
        source._isLeftContinuous = new Lazy<bool>(() => throw new InvalidOperationException("Cached failure."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        Assert.Throws<InvalidOperationException>(() => source.IsLeftContinuous);
        var result = source.ToLeftContinuous();
        Assert.True(Curve.Equivalent(source, result));
        Assert.Throws<InvalidOperationException>(() => source.IsLeftContinuous);
    }

    [Fact]
    public void ToRightContinuousReusesAnInputKnownToSatisfyTheProperty()
    {
        var source = Curve.Zero();
        Assert.True(source.IsRightContinuous);
        Assert.Same(source, source.ToRightContinuous());
    }

    [Fact]
    public void ToRightContinuousDoesNotInitializeAnUnknownPredicate()
    {
        var source = Curve.Zero();
        source._isRightContinuous = new Lazy<bool>(() => throw new InvalidOperationException("Predicate was evaluated."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        var result = source.ToRightContinuous();
        Assert.True(Curve.Equivalent(source, result));
        Assert.False(source._isRightContinuous.IsValueCreated);
    }

    [Fact]
    public void ToRightContinuousDoesNotReadAFaultedPredicate()
    {
        var source = Curve.Zero();
        source._isRightContinuous = new Lazy<bool>(() => throw new InvalidOperationException("Cached failure."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        Assert.Throws<InvalidOperationException>(() => source.IsRightContinuous);
        var result = source.ToRightContinuous();
        Assert.True(Curve.Equivalent(source, result));
        Assert.Throws<InvalidOperationException>(() => source.IsRightContinuous);
    }

    [Fact]
    public void ToNonNegativeReusesAnInputKnownToSatisfyTheProperty()
    {
        var source = Curve.Zero();
        Assert.True(source.IsNonNegative);
        Assert.Same(source, source.ToNonNegative());
    }

    [Fact]
    public void ToNonNegativeDoesNotInitializeAnUnknownPredicate()
    {
        var source = Curve.Zero();
        source._isNonNegative = new Lazy<bool>(() => throw new InvalidOperationException("Predicate was evaluated."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        var result = source.ToNonNegative();
        Assert.True(Curve.Equivalent(source, result));
        Assert.False(source._isNonNegative.IsValueCreated);
    }

    [Fact]
    public void ToNonNegativeDoesNotReadAFaultedPredicate()
    {
        var source = Curve.Zero();
        source._isNonNegative = new Lazy<bool>(() => throw new InvalidOperationException("Cached failure."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        Assert.Throws<InvalidOperationException>(() => source.IsNonNegative);
        var result = source.ToNonNegative();
        Assert.True(Curve.Equivalent(source, result));
        Assert.Throws<InvalidOperationException>(() => source.IsNonNegative);
    }

    [Fact]
    public void ToLowerNonIncreasingReusesAnInputKnownToSatisfyTheProperty()
    {
        var source = Curve.Zero();
        Assert.True(source.IsNonIncreasing);
        Assert.Same(source, source.ToLowerNonIncreasing());
    }

    [Fact]
    public void ToLowerNonIncreasingDoesNotInitializeAnUnknownPredicate()
    {
        var source = Curve.Zero();
        source._isNonIncreasing = new Lazy<bool>(() => throw new InvalidOperationException("Predicate was evaluated."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        var result = source.ToLowerNonIncreasing();
        Assert.True(Curve.Equivalent(source, result));
        Assert.False(source._isNonIncreasing.IsValueCreated);
    }

    [Fact]
    public void ToLowerNonIncreasingDoesNotReadAFaultedPredicate()
    {
        var source = Curve.Zero();
        source._isNonIncreasing = new Lazy<bool>(() => throw new InvalidOperationException("Cached failure."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        Assert.Throws<InvalidOperationException>(() => source.IsNonIncreasing);
        var result = source.ToLowerNonIncreasing();
        Assert.True(Curve.Equivalent(source, result));
        Assert.Throws<InvalidOperationException>(() => source.IsNonIncreasing);
    }

    [Fact]
    public void ToUpperNonIncreasingReusesAnInputKnownToSatisfyTheProperty()
    {
        var source = Curve.Zero();
        Assert.True(source.IsNonIncreasing);
        Assert.Same(source, source.ToUpperNonIncreasing());
    }

    [Fact]
    public void ToUpperNonIncreasingDoesNotInitializeAnUnknownPredicate()
    {
        var source = Curve.Zero();
        source._isNonIncreasing = new Lazy<bool>(() => throw new InvalidOperationException("Predicate was evaluated."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        var result = source.ToUpperNonIncreasing();
        Assert.True(Curve.Equivalent(source, result));
        Assert.False(source._isNonIncreasing.IsValueCreated);
    }

    [Fact]
    public void ToUpperNonIncreasingDoesNotReadAFaultedPredicate()
    {
        var source = Curve.Zero();
        source._isNonIncreasing = new Lazy<bool>(() => throw new InvalidOperationException("Cached failure."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        Assert.Throws<InvalidOperationException>(() => source.IsNonIncreasing);
        var result = source.ToUpperNonIncreasing();
        Assert.True(Curve.Equivalent(source, result));
        Assert.Throws<InvalidOperationException>(() => source.IsNonIncreasing);
    }

    [Fact]
    public void ToLeftContinuousTransformsAnInputKnownToViolateTheProperty()
    {
        var source = new Curve(new Sequence([new Point(0, 2), new Segment(0, 1, 2, 0), new Point(1, -1), new Segment(1, 2, 2, 0)]), 1, 1, 0);
        var coldResult = source.ToLeftContinuous();
        Assert.False(source.IsLeftContinuous);
        var warmResult = source.ToLeftContinuous();
        Assert.False(Curve.Equivalent(source, warmResult));
        Assert.True(Curve.Equivalent(coldResult, warmResult));
    }

    [Fact]
    public void ToRightContinuousTransformsAnInputKnownToViolateTheProperty()
    {
        var source = new Curve(new Sequence([new Point(0, 2), new Segment(0, 1, 2, 0), new Point(1, -1), new Segment(1, 2, 2, 0)]), 1, 1, 0);
        var coldResult = source.ToRightContinuous();
        Assert.False(source.IsRightContinuous);
        var warmResult = source.ToRightContinuous();
        Assert.False(Curve.Equivalent(source, warmResult));
        Assert.True(Curve.Equivalent(coldResult, warmResult));
    }

    [Fact]
    public void ToNonNegativeTransformsAnInputKnownToViolateTheProperty()
    {
        var source = new Curve(new Sequence([new Point(0, 2), new Segment(0, 1, 2, 0), new Point(1, -1), new Segment(1, 2, 2, 0)]), 1, 1, 0);
        var coldResult = source.ToNonNegative();
        Assert.False(source.IsNonNegative);
        var warmResult = source.ToNonNegative();
        Assert.False(Curve.Equivalent(source, warmResult));
        Assert.True(Curve.Equivalent(coldResult, warmResult));
    }

    [Fact]
    public void ToLowerNonIncreasingTransformsAnInputKnownToViolateTheProperty()
    {
        var source = new Curve(new Sequence([new Point(0, 2), new Segment(0, 1, 2, 0), new Point(1, -1), new Segment(1, 2, 2, 0)]), 1, 1, 0);
        var coldResult = source.ToLowerNonIncreasing();
        Assert.False(source.IsNonIncreasing);
        var warmResult = source.ToLowerNonIncreasing();
        Assert.False(Curve.Equivalent(source, warmResult));
        Assert.True(Curve.Equivalent(coldResult, warmResult));
    }

    [Fact]
    public void ToUpperNonIncreasingTransformsAnInputKnownToViolateTheProperty()
    {
        var source = new Curve(new Sequence([new Point(0, 2), new Segment(0, 1, 2, 0), new Point(1, -1), new Segment(1, 2, 2, 0)]), 1, 1, 0);
        var coldResult = source.ToUpperNonIncreasing();
        Assert.False(source.IsNonIncreasing);
        var warmResult = source.ToUpperNonIncreasing();
        Assert.False(Curve.Equivalent(source, warmResult));
        Assert.True(Curve.Equivalent(coldResult, warmResult));
    }

}
