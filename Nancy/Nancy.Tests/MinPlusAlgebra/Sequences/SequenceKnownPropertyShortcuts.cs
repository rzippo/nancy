using System;
using System.Threading;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceKnownPropertyShortcuts
{
    [Fact]
    public void ToLeftContinuousReusesAnInputKnownToSatisfyTheProperty()
    {
        var source = new Sequence([new Point(0, 0), new Segment(0, 1, 0, 0)]);
        Assert.True(source.IsLeftContinuous);
        Assert.Same(source, source.ToLeftContinuous());
    }

    [Fact]
    public void ToLeftContinuousDoesNotInitializeAnUnknownPredicate()
    {
        var source = new Sequence([new Point(0, 0), new Segment(0, 1, 0, 0)]);
        source._isLeftContinuous = new Lazy<bool>(() => throw new InvalidOperationException("Predicate was evaluated."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        var result = source.ToLeftContinuous();
        Assert.True(Sequence.Equivalent(source, result));
        Assert.False(source._isLeftContinuous.IsValueCreated);
    }

    [Fact]
    public void ToLeftContinuousDoesNotReadAFaultedPredicate()
    {
        var source = new Sequence([new Point(0, 0), new Segment(0, 1, 0, 0)]);
        source._isLeftContinuous = new Lazy<bool>(() => throw new InvalidOperationException("Cached failure."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        Assert.Throws<InvalidOperationException>(() => source.IsLeftContinuous);
        var result = source.ToLeftContinuous();
        Assert.True(Sequence.Equivalent(source, result));
        Assert.Throws<InvalidOperationException>(() => source.IsLeftContinuous);
    }

    [Fact]
    public void ToRightContinuousReusesAnInputKnownToSatisfyTheProperty()
    {
        var source = new Sequence([new Point(0, 0), new Segment(0, 1, 0, 0)]);
        Assert.True(source.IsRightContinuous);
        Assert.Same(source, source.ToRightContinuous());
    }

    [Fact]
    public void ToRightContinuousDoesNotInitializeAnUnknownPredicate()
    {
        var source = new Sequence([new Point(0, 0), new Segment(0, 1, 0, 0)]);
        source._isRightContinuous = new Lazy<bool>(() => throw new InvalidOperationException("Predicate was evaluated."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        var result = source.ToRightContinuous();
        Assert.True(Sequence.Equivalent(source, result));
        Assert.False(source._isRightContinuous.IsValueCreated);
    }

    [Fact]
    public void ToRightContinuousDoesNotReadAFaultedPredicate()
    {
        var source = new Sequence([new Point(0, 0), new Segment(0, 1, 0, 0)]);
        source._isRightContinuous = new Lazy<bool>(() => throw new InvalidOperationException("Cached failure."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        Assert.Throws<InvalidOperationException>(() => source.IsRightContinuous);
        var result = source.ToRightContinuous();
        Assert.True(Sequence.Equivalent(source, result));
        Assert.Throws<InvalidOperationException>(() => source.IsRightContinuous);
    }

    [Fact]
    public void ToLeftContinuousTransformsAnInputKnownToViolateTheProperty()
    {
        var source = new Sequence([new Point(0, 2), new Segment(0, 1, 2, 0), new Point(1, -1), new Segment(1, 2, 2, 0)]);
        var coldResult = source.ToLeftContinuous();
        Assert.False(source.IsLeftContinuous);
        var warmResult = source.ToLeftContinuous();
        Assert.False(Sequence.Equivalent(source, warmResult));
        Assert.True(Sequence.Equivalent(coldResult, warmResult));
    }

    [Fact]
    public void ToRightContinuousTransformsAnInputKnownToViolateTheProperty()
    {
        var source = new Sequence([new Point(0, 2), new Segment(0, 1, 2, 0), new Point(1, -1), new Segment(1, 2, 2, 0)]);
        var coldResult = source.ToRightContinuous();
        Assert.False(source.IsRightContinuous);
        var warmResult = source.ToRightContinuous();
        Assert.False(Sequence.Equivalent(source, warmResult));
        Assert.True(Sequence.Equivalent(coldResult, warmResult));
    }

}
