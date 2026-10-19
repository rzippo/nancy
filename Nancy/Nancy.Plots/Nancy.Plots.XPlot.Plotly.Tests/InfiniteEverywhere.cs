using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Plots.XPlot.Plotly;
using Xunit;

namespace Nancy.Plots.XPlot.Plotly.Tests;

/// <summary>
/// A curve with no finite value is plotted as the area of its infinity.
/// </summary>
/// <remarks>
/// Smoke tests: they check that a chart is produced, not what it shows.
/// </remarks>
public class InfiniteEverywhere
{
    public static IEnumerable<object[]> InfiniteCurves()
    {
        yield return new object[] { Curve.MinusInfinite() };
        yield return new object[] { Curve.PlusInfinite() };
    }

    [Theory]
    [MemberData(nameof(InfiniteCurves))]
    [Trait("Category", "Smoke")]
    public void ACurveInfiniteEverywhereIsPlotted(Curve curve)
    {
        var html = curve.ToXPlotHtml("f");

        Assert.NotEmpty(html);
    }
}
