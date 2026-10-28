using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves.Streams;

/// <summary>
/// Checks streamed vertical deviations against materialized differences, including the first time approaching the supremum.
/// </summary>
public class VerticalDeviation
{
    public static IEnumerable<(Curve Left, Curve Right)> CurvePairs()
    {
        Curve[] curves =
        [
            new StairCurve(7, 3), new StairCurve(13, 5).DelayBy(4),
            new StairCurve(14, 6), new ConstantCurve(-3),
            new RateLatencyServiceCurve(2, 1), new SigmaRhoArrivalCurve(3, 1),
            new Curve(new Sequence([new Point(0, -2), new Segment(0, 2, -1, -2)]), 0, 2, -4),
            Curve.PlusInfinite(), Curve.MinusInfinite(),
            new Curve(new Sequence([new Point(0, 1), Segment.PlusInfinite(0, 2)]), 0, 2, 0)
        ];
        foreach (var f in curves)
        foreach (var g in curves)
            yield return (f, g);
    }

    public static IEnumerable<object[]> Pairs() => CurvePairs().ToXUnitTestCases();



    [Theory]
    [MemberData(nameof(Pairs))]
    public void DeviationsEqualTheMaterializedDifference(Curve f, Curve g)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        Curve difference;
        try { difference = Curve.Subtraction(f, g, settings); }
        catch (UndeterminedResultException)
        {
            Assert.Throws<UndeterminedResultException>(() => Curve.VerticalDeviation(f, g, settings));
            Assert.Throws<UndeterminedResultException>(() => Curve.VerticalDeviationMeasuredAt(f, g, settings));
            return;
        }
        Assert.Equal(difference.SupValue(settings), Curve.VerticalDeviation(f, g, settings));
        Assert.Equal(difference.SupArg(settings), Curve.VerticalDeviationMeasuredAt(f, g, settings));
        var fCut = f.Cut(0, 20, true, true, settings);
        var gCut = g.Cut(0, 20, true, true, settings);
        Assert.Equal(Sequence.Subtraction(fCut, gCut)
            .SupValue(), Sequence.VerticalDeviation(fCut, gCut, settings));
    }
}
