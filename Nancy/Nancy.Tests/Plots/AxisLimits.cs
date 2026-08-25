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

        var limits = PlotAxisLimitAlgorithms.SuggestAxisLimits(
            [sequence],
            new PlotSettings
            {
                RelativeXAxisMargin = 0.25,
                RelativeYAxisMargin = 0.25
            });

        // both axes are non-negative here, so the full margin goes above and half of it below
        Assert.Equal(new Interval(new Rational(-1, 2), 5), limits.XLimit);
        Assert.Equal(new Interval(new Rational(115, 8), new Rational(85, 4)), limits.YLimit);
    }

    [Fact]
    public void ExplicitFiniteSequenceAxisLimitsTakePrecedenceOverMargins()
    {
        var sequence = GetSequence();
        var xLimit = new Interval(-1, 10);
        var yLimit = new Interval(-2, 20);

        var limits = PlotAxisLimitAlgorithms.SuggestAxisLimits(
            [sequence],
            new PlotSettings
            {
                XLimit = xLimit,
                YLimit = yLimit,
                RelativeXAxisMargin = 1,
                RelativeYAxisMargin = 1
            });

        Assert.Equal(xLimit, limits.XLimit);
        Assert.Equal(yLimit, limits.YLimit);
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
