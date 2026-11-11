using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class PairChunkBoundaries
{
    public static IEnumerable<(Interval Support, Rational[] Boundaries)> InvalidPartitions()
    {
        yield return (Interval.Closed(0, 2), []);
        yield return (Interval.Closed(0, 2), [0]);
        yield return (Interval.Closed(0, 2), [1, 2]);
        yield return (Interval.Closed(0, 2), [0, 1]);
        yield return (Interval.Closed(0, 2), [0, 1, 1, 2]);
        yield return (Interval.Closed(0, 2), [0, 2, 1, 2]);
        yield return (Interval.Closed(0, 2), [0, -1, 2]);
        yield return (Interval.Closed(1, 1), [1, 1]);
        yield return (new Interval(1, 1, false, false), [1]);
    }

    public static IEnumerable<object[]> InvalidCases() => InvalidPartitions()
        .Select(c => new object[] { c.Support, c.Boundaries });

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void InvalidPartitionsAreRejectedBeforeReadingOperands(Interval support, Rational[] boundaries)
    {
        Assert.ThrowsAny<ArgumentException>(() => Sequence.ComputePointwisePairOperationInParallel(
            support, boundaries,
            _ => throw new InvalidOperationException("Operand must not be read."),
            _ => throw new InvalidOperationException("Operand must not be read."),
            (a, b) => a, new ComputationSettings()));
    }

    [Fact]
    public void SingletonSupportUsesOneBoundary()
    {
        var point = Sequence.Zero(1, 1, true, true);
        var result = Sequence.ComputePointwisePairOperationInParallel(Interval.Closed(1, 1), [1],
            _ => point.Elements, _ => point.Elements, (a, b) => a, new ComputationSettings());
        Assert.Equal(0, result.ValueAt(1));
        Assert.Equal(point.Support, result.Support);
    }
}
