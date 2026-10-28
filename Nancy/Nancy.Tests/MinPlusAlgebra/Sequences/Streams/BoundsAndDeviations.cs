using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

/// <summary>
/// Checks sequence bounds and deviations computed from element streams.
/// </summary>
public class BoundsAndDeviations
{
    public static IEnumerable<object[]> Bounds()
        => Envelopes.OperandCases().Concat(Envelopes.SupportPairCases())
            .ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Bounds))]
    public void BoundsEqualTheMaterializedEnvelope(string name, Sequence a, Sequence b)
    {
        Assert.NotEmpty(name);
        var settings = new ComputationSettings { UseParallelism = false };
        foreach (var (left, right) in new[] { (a, b), (b, a) })
        {
            var overlap = Interval.Intersection(left.Support, right.Support)!.Value;
            var expected = Sequence.Equivalent(left.Cut(overlap), Sequence.Minimum(left, right, settings: settings));
            Assert.Equal(expected, Sequence.LessOrEqual(left, right, settings));
            Assert.Equal(expected, Sequence.GreaterOrEqual(right, left, settings));
        }
    }

    public static IEnumerable<object[]> Differences()
        => Addition.FiniteOperandCases().Concat(Addition.InfiniteOperandCases())
            .ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Differences))]
    public void VerticalDeviationsEqualTheMaterializedDifference(string name, Sequence a, Sequence b)
    {
        Assert.NotEmpty(name);
        foreach (var (left, right) in new[] { (a, b), (b, a) })
            Assert.Equal(Sequence.Subtraction(left, right)
                .SupValue(), Sequence.VerticalDeviation(left, right));
    }

    public static IEnumerable<(Sequence Left, Sequence Right)> HorizontalPairCases()
    {
        Sequence[] sequences =
        [
            new StairCurve(4, 3).Cut(0, 20, true, true),
            new StairCurve(6, 4).Cut(0, 20, true, true),
            new RateLatencyServiceCurve(2, 1).Cut(0, 20, true, true)
        ];
        foreach (var a in sequences)
        foreach (var b in sequences)
            yield return (a, b);
    }

    public static IEnumerable<object[]> HorizontalPairs() => HorizontalPairCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(HorizontalPairs))]
    public void HorizontalDeviationsEqualTheCompositionStrategy(Sequence a, Sequence b)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        var expected = Sequence.HorizontalDeviation(a, b, settings with { HorizontalDeviationStrategy = HorizontalDeviationStrategy.Composition });
        var actual = Sequence.HorizontalDeviation(a, b, settings with { HorizontalDeviationStrategy = HorizontalDeviationStrategy.PseudoInverseGap });
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void BoundChecksStopAtTheFirstViolatingPoint()
    {
        var disposed = 0;
        IEnumerable<Element> Source(int value)
        {
            try
            {
                yield return new Point(0, value);
                yield return Segment.Constant(0, 1, value);
                throw new InvalidOperationException("The tail must not be read.");
            }
            finally { disposed++; }
        }
        Assert.False(SequenceExtensions.LessOrEqualOnOverlap(Source(2), Source(1)));
        Assert.Equal(2, disposed);
    }

    [Fact]
    public void BoundChecksStopAtTheFirstViolatingSegment()
    {
        var disposed = 0;
        IEnumerable<Element> Source(int value)
        {
            try
            {
                yield return Segment.Constant(0, 1, value);
                throw new InvalidOperationException("The tail must not be read.");
            }
            finally { disposed++; }
        }
        Assert.False(SequenceExtensions.LessOrEqualOnOverlap(Source(2), Source(1)));
        Assert.Equal(2, disposed);
    }
}
