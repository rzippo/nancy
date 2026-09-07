using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Xunit;

namespace Unipi.Nancy.Tests.Plots;

/// <summary>
/// The three x-ranges a plot works with, and the rules that keep them apart.
/// </summary>
public class PlotXWindows
{
    [Fact]
    public void ACurveIsSampledOverTheFrameAndNotOverTheDataRange()
    {
        var window = PlotXWindow.ForCurves(
            [new RateLatencyServiceCurve(rate: 1, latency: 3)],
            new PlotSettings { XLimit = new Interval(0, 10) });

        Assert.Equal(new Interval(0, 10), window.Data);
        // 3% of the length, half of it below and the rest above, since the values are non-negative
        Assert.Equal(-0.15m, window.Frame.Lower);
        Assert.Equal(10.3m, window.Frame.Upper);
        // the samples run to the frame, so the last point drawn is the frame and not an end
        Assert.Equal(10.3m, window.Sampling.Upper);
        Assert.True(window.SamplesReachTheFrame);
        Assert.True(window.DataContinuesPastSamples);
    }

    /// <summary>
    /// An explicit limit below 0 is a window the reader asked for; a curve has no values there.
    /// Clamping the data range instead of the sampling range would silently move the frame.
    /// </summary>
    [Fact]
    public void AnExplicitNegativeLowerBoundIsFramedButNotSampled()
    {
        var window = PlotXWindow.ForCurves(
            [new RateLatencyServiceCurve(rate: 3, latency: 1)],
            new PlotSettings { XLimit = new Interval(-1, 10) });

        Assert.Equal(new Interval(-1, 10), window.Data);
        Assert.Equal(-1.33m, window.Frame.Lower);
        Assert.Equal(10.33m, window.Frame.Upper);
        // sampled only where the curve is defined
        Assert.Equal(0, window.Sampling.Lower);
        Assert.Equal(10.33m, window.Sampling.Upper);
    }

    [Fact]
    public void WithoutAnExplicitLimitTheDataRangeIsWhereTheCurvesAreWorthShowing()
    {
        var curve = new RateLatencyServiceCurve(rate: 1, latency: 3);

        var window = PlotXWindow.ForCurves([curve], new PlotSettings());

        Assert.Equal(new Interval(0, curve.PseudoPeriodStart * 2), window.Data);
        Assert.Equal(6.18m, window.Frame.Upper);
        Assert.Equal(6.18m, window.Sampling.Upper);
    }

    /// <summary>
    /// A single value has no range to scale a margin against, so it is opened up first — before the margin, not after.
    /// </summary>
    [Fact]
    public void ADegenerateDataRangeIsOpenedBeforeTheMarginIsApplied()
    {
        var window = PlotXWindow.ForCurves(
            [new RateLatencyServiceCurve(rate: 1, latency: 3)],
            new PlotSettings { XLimit = new Interval(0, 0) });

        Assert.Equal(new Interval(0, 0), window.Data);
        Assert.Equal(1.03m, window.Frame.Upper);
    }

    /// <summary>
    /// Nothing is known past a sequence's end, so it is not sampled past it and its last point stays an end.
    /// </summary>
    [Fact]
    public void ASequenceIsNotSampledPastItself()
    {
        var sequence = new RateLatencyServiceCurve(rate: 1, latency: 3).Cut(0, 5);

        var window = PlotXWindow.ForSequences([sequence], new PlotSettings { XLimit = new Interval(0, 5) });

        Assert.Equal(new Interval(0, 5), window.Data);
        Assert.Equal(5.15m, window.Frame.Upper);
        Assert.Equal(5, window.Sampling.Upper);
        Assert.False(window.SamplesReachTheFrame);
        Assert.False(window.DataContinuesPastSamples);
    }

    /// <summary>
    /// A sequence reaching past the frame has still not been sampled to it: its end is its own, and keeps its marks.
    /// </summary>
    [Fact]
    public void ASequenceReachingPastTheFrameStillEndsOnItsOwnTerms()
    {
        var sequence = new RateLatencyServiceCurve(rate: 1, latency: 3).Cut(0, 5);

        var window = PlotXWindow.ForSequences([sequence], new PlotSettings { XLimit = new Interval(0, 2) });

        Assert.True(window.Sampling.Upper > window.Frame.Upper);
        Assert.False(window.SamplesReachTheFrame);
    }

    /// <summary>
    /// The frame comes from the data range, so sampling past it does not push the frame out again.
    /// Deriving the frame from the samples instead would add the margin to a range that already carried it.
    /// </summary>
    [Fact]
    public void FramingOversampledSequencesDoesNotApplyTheMarginTwice()
    {
        var curve = new RateLatencyServiceCurve(rate: 1, latency: 3);
        var window = PlotXWindow.ForCurves([curve], new PlotSettings());
        var samples = curve.Cut(window.Sampling);

        var limits = PlotAxisLimitAlgorithms.SuggestFramingLimits([samples], new PlotSettings(), window);

        Assert.Equal(window.Frame, limits.XFramingLimit);
        Assert.Equal(window.Data, limits.DataLimits.XDataLimit);
        // and the samples do reach past the data range, so the case is the one being guarded
        Assert.True(samples.DefinedUntil > window.Data.Upper);
    }
}
