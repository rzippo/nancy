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
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ItIsTheCutOverThePeriod(Curve curve)
    {
        var expected = curve.BaseSequence.Cut(curve.PseudoPeriodStart, curve.FirstPseudoPeriodEnd);
        Assert.Equal(expected, curve.PseudoPeriodicSequence);
        Assert.Equal(expected.Elements, curve.PseudoPeriodicElements);
    }

    [Fact]
    public void ABaseSequenceWithoutAPointAtThePeriodStartIsCut()
    {
        // replacing the base sequence with its merged form removes the point at 5
        var split = new Curve(new Sequence([new Point(0, 1), new Segment(0, 5, 1, 1), new Point(5, 6), new Segment(5, 6, 6, 1)]), 5, 1, 1);
        var merged = new Curve(split) { BaseSequence = split.BaseSequence.Optimize() };

        Assert.Equal(2, merged.BaseSequence.Count);
        Assert.Equal(new Sequence([new Point(5, 6), new Segment(5, 6, 6, 1)]), merged.PseudoPeriodicSequence);
        Assert.Equal(split.PseudoPeriodSlope, merged.PseudoPeriodSlope);
    }
}
