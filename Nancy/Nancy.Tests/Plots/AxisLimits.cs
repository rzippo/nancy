using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Xunit;

namespace Unipi.Nancy.Tests.Plots;

public class AxisLimits
{
    [Fact]
    public void CurveSamplingUsesSecondPseudoPeriodEndByDefault()
    {
        var curve = new RateLatencyServiceCurve(rate: 1, latency: 3);

        var limit = PlotAxisLimitAlgorithms.GetCurveSamplingXLimit(
            [curve],
            new PlotSettings());

        Assert.Equal(0, limit.Lower);
        Assert.Equal(curve.SecondPseudoPeriodEnd, limit.Upper);
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
    public void SequenceAxisLimitsApplyRelativeMarginsToDefaults()
    {
        var sequence = GetSequence();

        var limits = PlotAxisLimitAlgorithms.GetSequenceAxisLimits(
            [sequence],
            new PlotSettings
            {
                RelativeXAxisMargin = 0.25,
                RelativeYAxisMargin = 0.25
            });

        Assert.Equal(new Interval(-1, 5), limits.XLimit);
        Assert.Equal(new Interval(new Rational(55, 4), new Rational(85, 4)), limits.YLimit);
    }

    [Fact]
    public void ExplicitFiniteSequenceAxisLimitsTakePrecedenceOverMargins()
    {
        var sequence = GetSequence();
        var xLimit = new Interval(-1, 10);
        var yLimit = new Interval(-2, 20);

        var limits = PlotAxisLimitAlgorithms.GetSequenceAxisLimits(
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
