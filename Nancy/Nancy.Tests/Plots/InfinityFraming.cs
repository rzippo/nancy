using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Xunit;

namespace Unipi.Nancy.Tests.Plots;

/// <summary>
/// Covers the corners that plotting infinite values relies on.
/// </summary>
/// <remarks>
/// A degenerate y-range and a zero-width <see cref="Point"/> behave unlike the general case, and every defect found while implementing the areas came from one of the two.
/// </remarks>
public class InfinityFraming
{
    private static Sequence DelaySequence(Rational delay, Rational until)
        => new DelayServiceCurve(delay).Cut(0, until, true, true);

    #region Infinite regions

    [Fact]
    public void DelayZeroRegionStartsAtTheOrigin()
    {
        // the sequence opens with a finite point at 0, which has no width:
        // the region after it starts at 0 without covering it
        var regions = DelaySequence(0, 20).EnumerateInfiniteRegions().ToList();

        var region = Assert.Single(regions);
        Assert.Equal(0, region.StartTime);
        Assert.False(region.IsStartIncluded);
        Assert.True(region.IsPlusInfinite);
    }

    [Fact]
    public void RegionsAreNotExtendedBeforeTheFirstSample()
    {
        var regions = DelaySequence(0, 20)
            .EnumerateVisibleInfiniteRegions(new Interval(-5, 25), continuesPastEnd: true)
            .ToList();

        var region = Assert.Single(regions);
        Assert.Equal(0, region.StartTime);
    }

    [Fact]
    public void RegionsReachTheEdgeOnlyWhenTheDataContinues()
    {
        var sequence = DelaySequence(10, 40);
        var xLimit = new Interval(0, 50);

        var carried = sequence
            .EnumerateVisibleInfiniteRegions(xLimit, continuesPastEnd: true)
            .Single();
        Assert.Equal(50, carried.EndTime);

        // a sequence given directly ends where it ends: drawing past it would claim a value that is not known
        var stopped = sequence
            .EnumerateVisibleInfiniteRegions(xLimit, continuesPastEnd: false)
            .Single();
        Assert.Equal(40, stopped.EndTime);
    }

    [Fact]
    public void OppositeInfinitiesAreNeverMerged()
    {
        var sequence = new Sequence(
        [
            Segment.PlusInfinite(0, 2),
            Point.MinusInfinite(2),
            Segment.MinusInfinite(2, 4)
        ]);

        var regions = sequence.EnumerateInfiniteRegions().ToList();

        Assert.Equal(2, regions.Count);
        Assert.True(regions[0].IsPlusInfinite);
        Assert.False(regions[1].IsPlusInfinite);
    }

    #endregion

    #region Framing

    [Fact]
    public void DegenerateRangeIsScaledAgainstTheXAxis()
    {
        // a delay curve has 0 as its only finite value, so there is no y-range to scale the area against
        var settings = new PlotSettings { RelativeXAxisMargin = 0, RelativeYAxisMargin = 0 };
        var limits = PlotAxisLimitAlgorithms.SuggestAxisLimits([DelaySequence(10, 40)], settings);

        Assert.True(limits.HasPlusInfinityBand);
        Assert.False(limits.HasMinusInfinityBand);
        // 40 * 0.6
        Assert.Equal(24, limits.InfinityBandHeight);
        Assert.Equal(new Interval(0, 24), limits.YLimit);
    }

    [Fact]
    public void PlusInfinityAreaGrowsOutOfTheXAxis()
    {
        var settings = new PlotSettings { RelativeXAxisMargin = 0, RelativeYAxisMargin = 0 };
        var limits = PlotAxisLimitAlgorithms.SuggestAxisLimits([DelaySequence(10, 40)], settings);

        Assert.Equal(0, limits.PlusInfinityBand.Lower);
        Assert.Equal(limits.YLimit.Upper, limits.PlusInfinityBand.Upper);
    }

    [Fact]
    public void MinusInfinityReservesRoomBelowTheAxis()
    {
        var settings = new PlotSettings { RelativeXAxisMargin = 0, RelativeYAxisMargin = 0 };
        var limits = PlotAxisLimitAlgorithms.SuggestAxisLimits(
            [(-new DelayServiceCurve(10)).Cut(0, 40, true, true)],
            settings);

        Assert.True(limits.HasMinusInfinityBand);
        // the room is below 0, even though every finite value is non-negative
        Assert.True(limits.YLimit.Lower < 0);
        Assert.Equal(0, limits.MinusInfinityBand.Upper);
        Assert.Equal(limits.YLimit.Lower, limits.MinusInfinityBand.Lower);
    }

    [Fact]
    public void IgnoreReservesNoRoom()
    {
        var settings = new PlotSettings
        {
            InfinityStrategy = InfinityStrategy.Ignore,
            RelativeXAxisMargin = 0,
            RelativeYAxisMargin = 0
        };
        var limits = PlotAxisLimitAlgorithms.SuggestAxisLimits([DelaySequence(10, 40)], settings);

        Assert.False(limits.HasPlusInfinityBand);
        Assert.Equal(0, limits.InfinityBandHeight);
    }

    #endregion

    #region Trailing continuation

    [Fact]
    public void CutCurvesCarryTheirLastLineToTheEdge()
    {
        // cut over [0, 5], rising at slope 1 from t = 3, so f(5) = 2
        var sequence = new RateLatencyServiceCurve(rate: 1, latency: 3).Cut(0, 5, true, true);

        var continuation = sequence.GetTrailingContinuation(
            new Interval(0, 7), continuesPastEnd: true);

        Assert.NotNull(continuation);
        Assert.Equal(7, continuation!.Value.Time);
        // the slope it ended with is followed to the edge
        Assert.Equal(4, continuation.Value.Value);
    }

    [Fact]
    public void SequencesGivenDirectlyStopWhereTheyEnd()
    {
        var sequence = new RateLatencyServiceCurve(rate: 1, latency: 3).Cut(0, 5, true, true);

        Assert.Null(sequence.GetTrailingContinuation(
            new Interval(0, 7), continuesPastEnd: false));
    }

    [Fact]
    public void NothingIsCarriedWhenThePlotEndsAtTheData()
    {
        var sequence = new RateLatencyServiceCurve(rate: 1, latency: 3).Cut(0, 5, true, true);

        Assert.Null(sequence.GetTrailingContinuation(
            new Interval(0, 5), continuesPastEnd: true));
    }

    [Fact]
    public void NothingIsCarriedWhenTheSequenceEndsInfinite()
    {
        // the area marks the value past the delay: there is no finite line to carry
        Assert.Null(DelaySequence(10, 40).GetTrailingContinuation(
            new Interval(0, 50), continuesPastEnd: true));
    }

    #endregion

    [Fact]
    public void TheCarriedValueFitsInsideTheAxis()
    {
        // the curve reaches 12 at the cut and keeps rising, so the value it reaches
        // at the edge is what the y-axis has to hold, not the one at the cut
        var curve = new SigmaRhoArrivalCurve(sigma: 2, rho: 10);
        var sequence = curve.Cut(0, 10, true, true);
        // the x-margin is what leaves room past the data for the line to be carried into
        var settings = new PlotSettings { RelativeXAxisMargin = 0.1, RelativeYAxisMargin = 0 };

        var limits = PlotAxisLimitAlgorithms.SuggestAxisLimits(
            [sequence], settings, continuesPastEnd: true);
        var continuation = sequence.GetTrailingContinuation(limits.XLimit, continuesPastEnd: true);

        Assert.NotNull(continuation);
        Assert.True(
            continuation!.Value.Value <= limits.YLimit.Upper,
            $"the carried value {continuation.Value.Value} must fit under ymax {limits.YLimit.Upper}");
    }

    #region Provenance

    /// <summary>
    /// Records what the modeler reports about the sequences it is asked to plot.
    /// </summary>
    private sealed class ProvenanceProbe : NancyPlotModeler<PlotSettings, bool>
    {
        public override bool GetPlot(IEnumerable<Sequence> sequences, IEnumerable<string> names)
            => SequencesContinuePastCut;
    }

    [Fact]
    public void CutsOfCurvesAreKnownToContinue()
    {
        var probe = new ProvenanceProbe();

        var fromCurves = probe.GetPlot(
            (IReadOnlyCollection<Curve>)new List<Curve> { new RateLatencyServiceCurve(1, 3) },
            (IEnumerable<string>)new List<string> { "f" });

        Assert.True(fromCurves);
    }

    [Fact]
    public void SequencesPlottedDirectlyAreNotKnownToContinue()
    {
        var probe = new ProvenanceProbe();

        var fromSequences = probe.GetPlot(
            (IEnumerable<Sequence>)new List<Sequence> { DelaySequence(10, 40) },
            (IEnumerable<string>)new List<string> { "f" });

        Assert.False(fromSequences);
    }

    [Fact]
    public void PlottingCurvesDoesNotLeaveTheFlagSetForLaterSequences()
    {
        var probe = new ProvenanceProbe();

        probe.GetPlot(
            (IReadOnlyCollection<Curve>)new List<Curve> { new RateLatencyServiceCurve(1, 3) },
            (IEnumerable<string>)new List<string> { "f" });

        // the same modeler reused: a sequence must never inherit the provenance of an earlier curve
        var fromSequences = probe.GetPlot(
            (IEnumerable<Sequence>)new List<Sequence> { DelaySequence(10, 40) },
            (IEnumerable<string>)new List<string> { "f" });

        Assert.False(fromSequences);
    }

    #endregion

    #region Margins

    [Fact]
    public void EnsureNonDegenerateOpensOnTheSideTheValuesOccupy()
    {
        var atZero = PlotAxisLimitAlgorithms.EnsureNonDegenerate(
            new Interval(0, 0), 1, prefersRoomBelow: false);
        Assert.Equal(new Interval(0, 1), atZero);

        var atZeroGoingDown = PlotAxisLimitAlgorithms.EnsureNonDegenerate(
            new Interval(0, 0), 1, prefersRoomBelow: true);
        Assert.Equal(new Interval(-1, 0), atZeroGoingDown);

        var negative = PlotAxisLimitAlgorithms.EnsureNonDegenerate(
            new Interval(-5, -5), 1, prefersRoomBelow: false);
        Assert.Equal(new Interval(-6, -5), negative);
    }

    [Fact]
    public void EnsureNonDegenerateLeavesARangeAlone()
    {
        var limit = new Interval(1, 3);
        Assert.Equal(limit, PlotAxisLimitAlgorithms.EnsureNonDegenerate(limit, 1, false));
    }

    [Theory]
    [MemberData(nameof(SignedMarginCases))]
    public void SignedMarginIsLargerWhereTheValuesAre(
        Interval limit, Interval expected)
    {
        Assert.Equal(expected, PlotAxisLimitAlgorithms.ApplySignedMargin(limit, 0.1));
    }

    public static IEnumerable<object[]> SignedMarginCases()
    {
        // non-negative: full margin above, half below
        yield return [new Interval(0, 10), new Interval(new Rational(-1, 2), 11)];
        // non-positive: full margin below, half above
        yield return [new Interval(-10, 0), new Interval(-11, new Rational(1, 2))];
        // straddling 0: the same on both sides
        yield return [new Interval(-10, 10), new Interval(-12, 12)];
    }

    #endregion
}
