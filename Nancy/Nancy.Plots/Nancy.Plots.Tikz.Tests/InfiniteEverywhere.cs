using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Plots.Tikz;
using Xunit;

namespace Nancy.Plots.Tikz.Tests;

/// <summary>
/// A curve with no finite value has no line to draw: its infinity is marked by an area, and the legend names it all the same.
/// </summary>
public class InfiniteEverywhere
{
    public static IEnumerable<object[]> InfiniteCurves()
    {
        yield return new object[] { Curve.MinusInfinite() };
        yield return new object[] { Curve.PlusInfinite() };
    }

    [Theory]
    [MemberData(nameof(InfiniteCurves))]
    public void TheLegendHasAnEntryForACurveInfiniteEverywhere(Curve curve)
    {
        var code = new List<Curve> { new RateLatencyServiceCurve(2, 3), curve }.ToTikzPlotCode(["f", "g"]);

        Assert.Equal(2, CountOf(code, "\\addlegendentry"));
        Assert.Equal(1, CountOf(code, "\\addlegendimage"));
    }

    private static int CountOf(string text, string token)
        => text.Split(token).Length - 1;
}
