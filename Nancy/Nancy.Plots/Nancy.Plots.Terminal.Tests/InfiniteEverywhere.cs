using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Plots.Terminal;
using Xunit;

namespace Nancy.Plots.Terminal.Tests;

/// <summary>
/// A curve with no finite value is plotted as the area of its infinity.
/// </summary>
public class InfiniteEverywhere
{
    public static IEnumerable<object[]> InfiniteCurves()
    {
        yield return new object[] { "-inf", Curve.MinusInfinite() };
        yield return new object[] { "+inf", Curve.PlusInfinite() };
    }

    [Theory]
    [MemberData(nameof(InfiniteCurves))]
    public void ACurveInfiniteEverywhereIsMarkedByItsInfinity(string label, Curve curve)
    {
        var settings = new TerminalPlotSettings { AnsiMode = TerminalPlotAnsiMode.PlainText };

        var plot = curve.ToTerminalPlot("f", settings);

        Assert.Contains(label, plot);
    }
}
