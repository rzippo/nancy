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
    public void ToNonNegativeReusesAnInputKnownToSatisfyTheProperty()
    {
        var source = new Sequence([new Point(0, 0), new Segment(0, 1, 0, 0)]);
        Assert.True(source.IsNonNegative);
        Assert.Same(source, source.ToNonNegative());
    }

    [Fact]
    public void ToNonNegativeDoesNotInitializeAnUnknownPredicate()
    {
        var source = new Sequence([new Point(0, 0), new Segment(0, 1, 0, 0)]);
        source._isNonNegative = new Lazy<bool>(() => throw new InvalidOperationException("Predicate was evaluated."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        var result = source.ToNonNegative();
        Assert.True(Sequence.Equivalent(source, result));
        Assert.False(source._isNonNegative.IsValueCreated);
    }

    [Fact]
    public void ToNonNegativeDoesNotReadAFaultedPredicate()
    {
        var source = new Sequence([new Point(0, 0), new Segment(0, 1, 0, 0)]);
        source._isNonNegative = new Lazy<bool>(() => throw new InvalidOperationException("Cached failure."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        Assert.Throws<InvalidOperationException>(() => source.IsNonNegative);
        var result = source.ToNonNegative();
        Assert.True(Sequence.Equivalent(source, result));
        Assert.Throws<InvalidOperationException>(() => source.IsNonNegative);
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

    [Fact]
    public void ToNonNegativeTransformsAnInputKnownToViolateTheProperty()
    {
        var source = new Sequence([new Point(0, 2), new Segment(0, 1, 2, 0), new Point(1, -1), new Segment(1, 2, 2, 0)]);
        var coldResult = source.ToNonNegative();
        Assert.False(source.IsNonNegative);
        var warmResult = source.ToNonNegative();
        Assert.False(Sequence.Equivalent(source, warmResult));
        Assert.True(Sequence.Equivalent(coldResult, warmResult));
    }

    [Fact]
    public void OptimizeReusesAnInputKnownToBeNormalized()
    {
        var source = new Sequence([new Point(0, 0), new Segment(0, 1, 0, 1)]);
        Assert.True(source.IsNormalized);
        Assert.Same(source, source.Optimize());
    }

    [Fact]
    public void OptimizeDoesNotInitializeAnUnknownPredicate()
    {
        var source = new Sequence([new Point(0, 0), new Segment(0, 1, 0, 1)]);
        source._isNormalized = new Lazy<bool>(() => throw new InvalidOperationException("Predicate was evaluated."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        Assert.Same(source, source.Optimize());
        Assert.False(source._isNormalized.IsValueCreated);
    }

    [Fact]
    public void OptimizeDoesNotReadAFaultedPredicate()
    {
        var source = new Sequence([new Point(0, 0), new Segment(0, 1, 0, 1)]);
        source._isNormalized = new Lazy<bool>(() => throw new InvalidOperationException("Cached failure."),
            LazyThreadSafetyMode.ExecutionAndPublication);
        Assert.Throws<InvalidOperationException>(() => source.IsNormalized);
        Assert.Same(source, source.Optimize());
        Assert.Throws<InvalidOperationException>(() => source.IsNormalized);
    }

    [Fact]
    public void OptimizeMergesAnInputKnownNotToBeNormalizedAndMarksTheResult()
    {
        var source = new Sequence([new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, 2, 1, 1)]);
        Assert.False(source.IsNormalized);
        var result = source.Optimize();
        Assert.Equal(new Sequence([new Point(0, 0), new Segment(0, 2, 0, 1)]), result);
        Assert.True(KnownProperties.GetIfKnown(ref result._isNormalized));
        Assert.True(result.ComputeIsNormalized());
    }

    [Fact]
    public void OptimizeWithoutPropagationChecksTheResultAndLeavesItUnknown()
    {
        var source = new Sequence([new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, 2, 1, 1)]);
        var result = source.Optimize(new ComputationSettings { UseKnownPropertyPropagation = false });
        Assert.Equal(new Sequence([new Point(0, 0), new Segment(0, 2, 0, 1)]), result);
        Assert.Null(KnownProperties.GetIfKnown(ref result._isNormalized));
        Assert.True(result.IsNormalized);
    }

}
