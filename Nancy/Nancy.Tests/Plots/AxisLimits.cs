using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Xunit;

namespace Unipi.Nancy.Tests.Plots;

public class AxisLimits
{
    [Fact]
    public void CurveSamplingShowsTwiceTheTransientOfAnUltimatelyAffineCurve()
    {
        // the pseudo-period of an ultimately affine curve is an artifact of the representation,
        // so two of them say nothing: what is worth showing is the transient and as much again
        var curve = new RateLatencyServiceCurve(rate: 1, latency: 3);

        var limit = PlotAxisLimitAlgorithms.GetCurveSamplingXLimit(
            [curve],
            new PlotSettings());

        Assert.Equal(0, limit.Lower);
        Assert.Equal(curve.PseudoPeriodStart * 2, limit.Upper);
    }

    [Fact]
    public void CurveSamplingClampsExplicitNegativeLowerBound()
    {
        var curve = new RateLatencyServiceCurve(rate: 1, latency: 3);

        var limit = PlotAxisLimitAlgorithms.GetCurveSamplingXLimit(
            [curve],
            new PlotSettings
            {
                XLimit = new Interval(-1, 10)
            });

        Assert.Equal(0, limit.Lower);
        Assert.Equal(10, limit.Upper);
    }

    [Fact]
    public void SequenceAxisLimitsApplyMarginsLargerWhereTheValuesAre()
    {
        var sequence = GetSequence();

        var limits = PlotAxisLimitAlgorithms.SuggestFramingLimits(
            [sequence],
            new PlotSettings
            {
                RelativeXAxisMargin = 0.25,
                RelativeYAxisMargin = 0.25
            });

        // the data limits are the extent of the finite values
        Assert.Equal(new Interval(0, 4), limits.DataLimits.XDataLimit);
        Assert.Equal(new Interval(15, 20), limits.DataLimits.YDataLimit);
        // both axes are non-negative here, so the full margin goes above and half of it below
        Assert.Equal(new Interval(new Rational(-1, 2), 5), limits.XFramingLimit);
        Assert.Equal(new Interval(new Rational(115, 8), new Rational(85, 4)), limits.YFramingLimit);
    }

    [Fact]
    public void ExplicitLimitsAreTheDataLimitsAndMarginsStillFrame()
    {
        var sequence = GetSequence();
        var xDataLimit = new Interval(-1, 10);
        var yDataLimit = new Interval(-2, 20);

        var limits = PlotAxisLimitAlgorithms.SuggestFramingLimits(
            [sequence],
            new PlotSettings
            {
                XLimit = xDataLimit,
                YLimit = yDataLimit,
                RelativeXAxisMargin = 1,
                RelativeYAxisMargin = 1
            });

        Assert.Equal(xDataLimit, limits.DataLimits.XDataLimit);
        Assert.Equal(yDataLimit, limits.DataLimits.YDataLimit);
        // both ranges straddle 0, so the full margin is added on every side
        Assert.Equal(new Interval(-12, 21), limits.XFramingLimit);
        Assert.Equal(new Interval(-24, 42), limits.YFramingLimit);
    }

    }

    private static Sequence GetSequence()
    {
        return new Sequence(
            [
                new Point(
                    time: 0,
                    value: 20),
                new Segment(
                    startTime: 0,
                    endTime: 4,
                    rightLimitAtStartTime: 20,
                    slope: new Rational(-5, 4)),
                new Point(
                    time: 4,
                    value: 15)
            ]);
    }
}
