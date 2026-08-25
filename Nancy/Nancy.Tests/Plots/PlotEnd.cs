using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Xunit;

namespace Unipi.Nancy.Tests.Plots;

/// <summary>
/// Covers how far to the right a plot of curves reaches.
/// </summary>
public class PlotEnd
{
    #region Periods

    [Theory]
    [MemberData(nameof(PeriodCases))]
    public void PeriodStrategiesMatchTheDocumentedShape(
        Curve curve, Rational onePeriod, Rational twoPeriods)
    {
        Assert.Equal(
            onePeriod,
            PlotAxisLimitAlgorithms.GetPlotEnd([curve], PlotEndStrategy.OnePeriodEach));
        Assert.Equal(
            twoPeriods,
            PlotAxisLimitAlgorithms.GetPlotEnd([curve], PlotEndStrategy.TwoPeriodsEach));
    }

    public static IEnumerable<object[]> PeriodCases()
    {
        // ultimately affine: the pseudo-period is an artifact, so twice the transient is shown
        yield return [new RateLatencyServiceCurve(1, 3), (Rational)6, (Rational)6];
        // a staircase has a period worth showing, and the two strategies differ
        yield return [new StairCurve(1, 3), (Rational)3, (Rational)6];
    }

    [Fact]
    public void ACurveWithoutAPeriodKeepsItsTransientOnScreen()
    {
        // the latency is the whole of what an affine curve has to show
        var affine = new RateLatencyServiceCurve(1, 100);
        var stair = new StairCurve(1, 3);

        Assert.Equal(200, PlotAxisLimitAlgorithms.GetPlotEnd([affine]));
        Assert.Equal(6, PlotAxisLimitAlgorithms.GetPlotEnd([stair]));
        Assert.Equal(200, PlotAxisLimitAlgorithms.GetPlotEnd([affine, stair]));
    }

    [Fact]
    public void APeriodReachingPastATransientSetsTheEnd()
    {
        // the other direction of the same rule
        var affine = new RateLatencyServiceCurve(1, 1);
        var stair = new StairCurve(1, 30);

        Assert.Equal(60, PlotAxisLimitAlgorithms.GetPlotEnd([affine, stair]));
    }

    [Fact]
    public void AnUltimatelyInfiniteCurveHasAnEnd()
    {
        // the legacy ComputePlotEnd filtered these out and then took Max of nothing
        var end = PlotAxisLimitAlgorithms.GetPlotEnd(
            [new DelayServiceCurve(10)], PlotEndStrategy.TwoPeriodsEach);

        Assert.Equal(40, end);
    }

    [Fact]
    public void TwoPeriodsEachIsTheDefault()
    {
        var limit = PlotAxisLimitAlgorithms.GetCurveSamplingXLimit(
            [new StairCurve(1, 3)], new PlotSettings());

        Assert.Equal(6, limit.Upper);
    }

    [Fact]
    public void AnExplicitLimitOverridesTheStrategy()
    {
        var limit = PlotAxisLimitAlgorithms.GetCurveSamplingXLimit(
            [new StairCurve(1, 3)],
            new PlotSettings
            {
                XLimit = new Interval(0, 100),
                PlotEndStrategy = PlotEndStrategy.OnePeriodEach
            });

        Assert.Equal(100, limit.Upper);
    }

    #endregion

    #region Intersections

    [Fact]
    public void TheStrategyReachesPastTheIntersection()
    {
        var f = new RateLatencyServiceCurve(1, 0);
        var g = new ConstantCurve(5);
        IReadOnlyCollection<Curve> curves = [f, g];

        var baseline = PlotAxisLimitAlgorithms.GetPlotEnd(curves, PlotEndStrategy.TwoPeriodsEach);
        var untilIntersection = PlotAxisLimitAlgorithms.GetPlotEnd(
            curves, PlotEndStrategy.UntilLastIntersection);

        // the intersection at 5 is past what two periods would have shown, and is left room beyond
        Assert.True(baseline < 5, $"baseline {baseline} should not already reach the intersection");
        Assert.True(
            untilIntersection > 5,
            $"the plot should reach past the intersection at 5, but ends at {untilIntersection}");
    }

    [Fact]
    public void WithoutAnIntersectionTheStrategyFallsBack()
    {
        IReadOnlyCollection<Curve> curves =
            [new RateLatencyServiceCurve(1, 0), new RateLatencyServiceCurve(2, 0)];

        Assert.Equal(
            PlotAxisLimitAlgorithms.GetPlotEnd(curves, PlotEndStrategy.TwoPeriodsEach),
            PlotAxisLimitAlgorithms.GetPlotEnd(curves, PlotEndStrategy.UntilLastIntersection));
    }

    [Fact]
    public void ASingleCurveHasNothingToIntersect()
    {
        IReadOnlyCollection<Curve> curves = [new StairCurve(1, 3)];

        Assert.Equal(
            PlotAxisLimitAlgorithms.GetPlotEnd(curves, PlotEndStrategy.TwoPeriodsEach),
            PlotAxisLimitAlgorithms.GetPlotEnd(curves, PlotEndStrategy.UntilLastIntersection));
    }

    [Fact]
    public void CurvesMeetingForeverDoNotStallTheStrategy()
    {
        // the same curve twice: they meet at every time, so there is no last intersection
        IReadOnlyCollection<Curve> curves = [new StairCurve(1, 3), new StairCurve(1, 3)];

        Assert.Equal(
            PlotAxisLimitAlgorithms.GetPlotEnd(curves, PlotEndStrategy.TwoPeriodsEach),
            PlotAxisLimitAlgorithms.GetPlotEnd(curves, PlotEndStrategy.UntilLastIntersection));
    }

    #endregion
}
