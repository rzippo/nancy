using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Pins the normalization invariant: a stretch over which the graph is a single line is a single element.
/// </summary>
/// <remarks>
/// Callers that read a maximal run off the elements depend on this, since a run arriving split would be
/// indistinguishable from two runs with a genuine gap between them.
/// Should the merge conditions ever change, these fail rather than the dependents failing obscurely.
/// </remarks>
public class Normalization
{
    public static IEnumerable<object[]> Curves()
    {
        yield return [new RateLatencyServiceCurve(1, 3)];
        yield return [new RateLatencyServiceCurve(3, 0)];
        yield return [new SigmaRhoArrivalCurve(2, 1)];
        yield return [new DelayServiceCurve(0)];
        yield return [new DelayServiceCurve(10)];
        yield return [new ConstantCurve(5)];
        yield return [new StairCurve(1, 3)];
    }

    [Theory]
    [MemberData(nameof(Curves))]
    public void CutIsNormalized(Curve curve)
    {
        // well past the pseudo-period start, so the periodic repetitions are included
        var cut = curve.Cut(0, curve.SecondPseudoPeriodEnd + 10, isEndIncluded: true);

        Assert.True(cut.IsNormalized, $"Cut of {curve.GetType().Name} is not normalized");
    }

    /// <summary>
    /// A curve whose period starts inside a segment, so its base sequence keeps a breakpoint there: 1 + t, with the period from 5.
    /// </summary>
    private static readonly Curve SplitAtThePeriodStart = new(
        new Sequence([new Point(0, 1), new Segment(0, 5, 1, 1), new Point(5, 6), new Segment(5, 6, 6, 1)]),
        pseudoPeriodStart: 5, pseudoPeriodLength: 1, pseudoPeriodHeight: 1);

    [Fact]
    public void CutAcrossThePeriodStartIsNormalized()
    {
        // within the base sequence, and past it
        Assert.False(SplitAtThePeriodStart.BaseSequence.IsNormalized);
        Assert.True(SplitAtThePeriodStart.Cut(0, new Rational(11, 2)).IsNormalized);
        Assert.True(SplitAtThePeriodStart.Cut(0, 8).IsNormalized);
        Assert.True(SplitAtThePeriodStart.CutAsEnumerable(0, new Rational(11, 2)).ToSequence().IsNormalized);
        Assert.True(SplitAtThePeriodStart.CutAsEnumerable(0, 8).ToSequence().IsNormalized);
    }

    /// <summary>
    /// Slopes 1, 2 and 1 over each period of length 3: the inverse has slopes 1, 1/2 and 1, so its consecutive periods meet in a line.
    /// </summary>
    private static readonly Curve InversePeriodsMeetInALine = new(
        new Sequence([new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, 2, 1, 2), new Point(2, 3), new Segment(2, 3, 3, 1)]),
        pseudoPeriodStart: 0, pseudoPeriodLength: 3, pseudoPeriodHeight: 4);

    [Fact]
    public void StreamedPseudoInversesAreNormalized()
    {
        Assert.True(InversePeriodsMeetInALine.LowerPseudoInverseAsEnumerable(20).ToSequence().IsNormalized);
        Assert.True(InversePeriodsMeetInALine.UpperPseudoInverseAsEnumerable(20).ToSequence().IsNormalized);
    }

    [Theory]
    [MemberData(nameof(Curves))]
    public void SubtractionIsNormalized(Curve curve)
    {
        var window = new Interval(0, curve.SecondPseudoPeriodEnd + 10);
        var other = new RateLatencyServiceCurve(2, 1);
        if (curve.IsUltimatelyInfinite)
            return; // the difference would be undetermined

        var difference = Sequence.Subtraction(curve.Cut(window), other.Cut(window));

        Assert.True(difference.IsNormalized, $"{curve.GetType().Name} - rate-latency is not normalized");
    }

    [Fact]
    public void APeriodicCurveIsNotSplitAtItsPeriodBoundaries()
    {
        // ultimately affine, so every repetition continues the same line: the whole tail is one segment
        var cut = new RateLatencyServiceCurve(rate: 1, latency: 3).Cut(0, 10, isEndIncluded: true);

        var tail = cut.Elements.OfType<Segment>().Last();
        Assert.Equal(3, tail.StartTime);
        Assert.Equal(10, tail.EndTime);
    }

    [Fact]
    public void IdenticalCurvesDifferToASingleStretch()
    {
        // the case the intersection API depends on: equality over a whole span, with a breakpoint
        // in the middle of it, must come back as one element and not as two touching ones
        var f = new Sequence(
        [
            new Point(0, 0),
            new Segment(0, 5, 0, 1),
            new Point(5, 5),
            new Segment(5, 10, 5, 2),
            new Point(10, 15)
        ]);

        var difference = Sequence.Subtraction(f, f);

        Assert.True(difference.IsNormalized);
        var zeroSegments = difference.Elements.OfType<Segment>().ToList();
        Assert.Single(zeroSegments);
        Assert.Equal(0, zeroSegments[0].StartTime);
        Assert.Equal(10, zeroSegments[0].EndTime);
    }

    [Fact]
    public void AGenuineGapIsNotClosed()
    {
        // the counterpart: equal either side of an instant but not at it, which must stay two runs
        var f = new Sequence(
        [
            new Point(0, 0),
            new Segment(0, 5, 0, 1),
            new Point(5, 5),
            new Segment(5, 10, 5, 1),
            new Point(10, 10)
        ]);
        var g = new Sequence(
        [
            new Point(0, 0),
            new Segment(0, 5, 0, 1),
            new Point(5, 8),
            new Segment(5, 10, 5, 1),
            new Point(10, 10)
        ]);

        var difference = Sequence.Subtraction(f, g);

        Assert.True(difference.IsNormalized);
        // the non-zero point at 5 keeps the two zero stretches apart
        Assert.Equal(2, difference.Elements.OfType<Segment>().Count(s => s.IsZero));
        Assert.Contains(difference.Elements.OfType<Point>(), p => p.Time == 5 && p.Value != 0);
    }

    [Fact]
    public void ABaseSequenceIsNotNormalized()
    {
        // documented exception: a breakpoint is forced at the pseudo-period start, so the two
        // identical infinite segments either side of it are left split
        var curve = new DelayServiceCurve(0);

        Assert.False(
            curve.BaseSequence.IsNormalized,
            "BaseSequence is documented as not normalized; if it now is, the remark on it is stale");
    }

    // The constructor's gates on the base sequence: merged, cut to [0, T + d[, split at T.

    [Fact]
    public void ConstructorMergesTheBaseSequence()
    {
        var curve = new Curve(new Sequence([new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, 2, 1, 1)]),
            pseudoPeriodStart: 0, pseudoPeriodLength: 2, pseudoPeriodHeight: 2);
        Assert.Equal(new Sequence([new Point(0, 0), new Segment(0, 2, 0, 1)]), curve.BaseSequence);
    }

    [Fact]
    public void ConstructorCutsThePointAtTheEndOfTheBaseSequence()
    {
        var curve = new Curve(new Sequence([new Point(0, 0), new Segment(0, 2, 0, 1), new Point(2, 2)]),
            pseudoPeriodStart: 0, pseudoPeriodLength: 2, pseudoPeriodHeight: 2);
        Assert.Equal(new Sequence([new Point(0, 0), new Segment(0, 2, 0, 1)]), curve.BaseSequence);
    }

    [Fact]
    public void ConstructorSplitsTheBaseSequenceAtThePeriodStart()
    {
        var curve = new Curve(new Sequence([new Point(0, 1), new Segment(0, 6, 1, 1)]),
            pseudoPeriodStart: 5, pseudoPeriodLength: 1, pseudoPeriodHeight: 1);
        Assert.Equal(SplitAtThePeriodStart.BaseSequence, curve.BaseSequence);
        Assert.Equal(4, curve.BaseSequence.Count);
    }

}
