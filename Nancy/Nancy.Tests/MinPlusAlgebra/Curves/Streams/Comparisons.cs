using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves.Streams;

/// <summary>
/// Checks streamed first differences and strict bounds against materialized breakpoint and intersection references.
/// </summary>
public class Comparisons
{
    public static IEnumerable<object[]> Pairs() => MaterializedBinaryOperators.CurvePairCases().ToXUnitTestCases();

    private static Rational? ReferenceFirstDifference(Curve a, Curve b, Rational time, bool isStartIncluded, ComputationSettings settings)
    {
        var cutEnd = Rational.Max(a.PseudoPeriodStart, b.PseudoPeriodStart, time)
                            + Rational.LeastCommonMultiple(a.PseudoPeriodLength, b.PseudoPeriodLength);

        // sampled on the cuts, as sampling a curve past its first pseudo-period computes the extension of that period
        var cutA = a.Cut(time, cutEnd, isStartIncluded, true, settings);
        var cutB = b.Cut(time, cutEnd, isStartIncluded, true, settings);

        // Between consecutive breakpoints, both curves are affine, so they agree there if and only if they agree at both limits.
        var breakpoints = cutA.Elements
            .Concat(cutB.Elements)
            .SelectMany(element => new[] { element.StartTime, element.EndTime })
            .Append(time)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        for (int i = 0; i < breakpoints.Count; i++)
        {
            var t = breakpoints[i];
            if ((t > time || isStartIncluded) && cutA.ValueAt(t) != cutB.ValueAt(t))
                return t;

            if (i + 1 < breakpoints.Count)
            {
                var next = breakpoints[i + 1];
                if (cutA.RightLimitAt(t) != cutB.RightLimitAt(t) || cutA.LeftLimitAt(next) != cutB.LeftLimitAt(next))
                    return t;
            }
        }

        return null;
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void FirstDifferencesEqualTheBreakpointReference(Curve a, Curve b)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        foreach (Rational time in new[] { 0, 1, 7 })
        foreach (var included in new[] { false, true })
            Assert.Equal(ReferenceFirstDifference(a, b, time, included, settings), Curve.FindFirstInequivalenceAfter(a, b, time, included, settings));
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void StrictBoundsEqualTheIntersectionReference(Curve a, Curve b)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        var lower = Curve.IsLowerBoundOf(a, b, settings);
        Assert.Equal(lower && a.GetIntersections(b, settings: settings).IsEmpty, Curve.IsStrictLowerBoundOf(a, b, settings));
        Assert.Equal(lower && a.GetIntersections(b, from: 0, isStartInclusive: false, settings: settings).IsEmpty, Curve.IsStrictLowerBoundOfExceptOrigin(a, b, settings));
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void ExcludingTheOriginRetainsANonStrictBoundThere(int origin, bool expected)
    {
        var lower = new ConstantCurve(0).WithOriginAt(origin);
        var upper = new ConstantCurve(1).WithOriginAt(1);
        Assert.Equal(origin, lower.ValueAt(0));
        Assert.Equal(1, upper.ValueAt(0));
        Assert.Equal(0, lower.RightLimitAt(0));
        Assert.Equal(0, lower.ValueAt(1));
        Assert.Equal(1, upper.RightLimitAt(0));
        var settings = new ComputationSettings { UseParallelism = false };
        Assert.Equal(expected, Curve.IsStrictLowerBoundOfExceptOrigin(lower, upper, settings));
        Assert.Equal(expected, Curve.IsStrictUpperBoundOfExceptOrigin(upper, lower, settings));
    }

    [Fact]
    public void EqualRightEndpointLimitsCanBoundStrictlyInside()
    {
        Element[] a = [new Segment(0, 1, 0, 1)];
        Element[] b = [Segment.Constant(0, 1, 1)];
        Assert.True(SequenceExtensions.StrictlyLessOnOverlap(a, b));
        Assert.False(SequenceExtensions.StrictlyLessOnOverlap(a, a));
    }

    [Fact]
    public void EqualLeftEndpointLimitsCanBoundStrictlyInside()
    {
        Element[] a = [Segment.Constant(0, 1, 0)];
        Element[] b = [new Segment(0, 1, 0, 1)];
        Assert.True(SequenceExtensions.StrictlyLessOnOverlap(a, b));
        Assert.False(SequenceExtensions.StrictlyLessOnOverlap(a, a));
    }

    [Fact]
    public void EquivalentFunctionsWithDifferentPartitionsHaveNoDifference()
    {
        Element[] a = [new Point(0, 0), new Segment(0, 4, 0, 1), new Point(4, 4)];
        Element[] b = [new Point(0, 0), new Segment(0, 2, 0, 1), new Point(2, 2), new Segment(2, 4, 2, 1), new Point(4, 4)];
        Assert.Null(SequenceExtensions.FirstInequivalenceOnOverlap(a, b));
    }

    [Fact]
    public void AFirstPointWitnessDoesNotConsumeTheTail()
    {
        var disposed = 0;
        IEnumerable<Element> Source(int value)
        {
            try
            {
                yield return new Point(0, value);
                throw new InvalidOperationException("The tail must not be read.");
            }
            finally { disposed++; }
        }
        Assert.Equal(0, SequenceExtensions.FirstInequivalenceOnOverlap(Source(1), Source(0)));
        Assert.False(SequenceExtensions.StrictlyLessOnOverlap(Source(1), Source(0)));
        Assert.Equal(4, disposed);
    }
}
