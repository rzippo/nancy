using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class DeviationChunkBoundaries
{
    public static IEnumerable<object[]> InvalidPartitions() => PairChunkBoundaries.InvalidPartitions()
        .Select(c => new object[] { c.Support, c.Boundaries });

    [Theory]
    [MemberData(nameof(InvalidPartitions))]
    public void InvalidPartitionsAreRejectedBeforeComputingChunks(Interval support, Rational[] boundaries)
    {
        Assert.ThrowsAny<ArgumentException>(() => Sequence.ComputeSupremumInParallel(support, boundaries,
            _ => throw new InvalidOperationException("Chunk must not be computed."), new ComputationSettings()));
    }

    public static IEnumerable<object[]> InvalidEvenPartitions =>
    [
        [Interval.Closed(0, 1), 0, 2], [Interval.Closed(0, 1), -1, 2],
        [Interval.Closed(0, 1), 2, 0], [Interval.Closed(0, 1), 2, -1],
        [Interval.Closed(1, 1), 2, 2], [Interval.UnboundedAbove(0), 2, 2]
    ];

    [Theory]
    [MemberData(nameof(InvalidEvenPartitions))]
    public void InvalidEvenPartitionsAreRejected(Interval interval, int partitions, int budget)
        => Assert.ThrowsAny<ArgumentException>(() => Sequence.GetEvenBoundaries(interval, partitions, budget));

    [Fact]
    public void SingletonSupremumUsesOneBoundary()
    {
        var result = Sequence.ComputeSupremumInParallel(Interval.Closed(1, 1), [1], _ => 7, new ComputationSettings());
        Assert.Equal(7, result);
    }

    [Fact]
    public void EvenPartitionsRespectTheBudgetAndBothEnds()
        => Assert.Equal(new Rational[] { 0, 2, 4 }, Sequence.GetEvenBoundaries(Interval.Closed(0, 4), 8, 2));
}
