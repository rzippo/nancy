using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// The pseudo-periodic sequence of a curve is the cut of its base sequence over $[T, T + d[$, whether read as a view of it or copied.
/// </summary>
public class PseudoPeriodView
{
    public static IEnumerable<object[]> CurveCases()
    {
        yield return [new RateLatencyServiceCurve(2, 3)];
        yield return [new SigmaRhoArrivalCurve(4, 1)];
        yield return [new StairCurve(2, 3)];
        yield return [new DelayServiceCurve(3)];
        yield return [Curve.Zero()];
        // the period starts inside a segment, which the curve splits there
        yield return [new Curve(new Sequence([new Point(0, 1), new Segment(0, 5, 1, 1), new Point(5, 6), new Segment(5, 6, 6, 1)]), 5, 1, 1)];
        // given as one segment across the period start
        yield return [new Curve(new Sequence([new Point(0, 1), new Segment(0, 6, 1, 1)]), 5, 1, 1)];
        // a partial curve, filled with +inf
        yield return [new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1)]), 1, 1, 0, isPartialCurve: true)];
        // an infinite height, which moves the period start one period on
        yield return [new Curve(new Sequence([Point.Origin(), Segment.Zero(0, 1), Point.PlusInfinite(1), Segment.PlusInfinite(1, 2)]), 0, 2, Rational.PlusInfinity)];
        // a copy, and results of operations
        yield return [new Curve(new StairCurve(2, 3))];
        yield return [Curve.Convolution(new RateLatencyServiceCurve(2, 3), new StairCurve(1, 2))];
        yield return [Curve.Minimum(new SigmaRhoArrivalCurve(4, 1), new StairCurve(3, 5))];
        yield return [new Point(1, 2).SubAdditiveClosure(pseudoPeriodLength: 2, pseudoPeriodHeight: 1)];
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ItIsTheCutOverThePeriod(Curve curve)
    {
        var expected = curve.BaseSequence.Cut(curve.PseudoPeriodStart, curve.FirstPseudoPeriodEnd);
        Assert.Equal(expected, curve.PseudoPeriodicSequence);
        Assert.Equal(expected.Elements, curve.PseudoPeriodicElements);
    }

    // The constructors split the base sequence at the pseudo-period start and end it, excluded, one period later, so the period is always a tail of its elements.
    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ThePeriodIsATailOfTheBaseSequence(Curve curve)
    {
        Assert.NotNull(curve.BaseSequence.SuffixFrom(curve.PseudoPeriodStart, curve.FirstPseudoPeriodEnd));
    }
}
