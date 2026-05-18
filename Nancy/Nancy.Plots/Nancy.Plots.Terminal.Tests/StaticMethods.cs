using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Unipi.Nancy.Plots.Terminal;
using Xunit;

namespace Nancy.Plots.Terminal.Tests;

public class StaticMethods
{
    private readonly ITestOutputHelper _testOutputHelper;

    public StaticMethods(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void PlotSingleCurveAsPlainText()
    {
        var rl = new RateLatencyServiceCurve(1, 3);
        var terminalPlot = rl.ToTerminalPlot(settings: new TerminalPlotSettings
        {
            Width = 32,
            Height = 10,
            AnsiMode = TerminalPlotAnsiMode.PlainText,
            LegendStrategy = LegendStrategy.ForceEnable
        });

        Assert.Contains("*", terminalPlot);
        Assert.Contains("Legend:", terminalPlot);
        Assert.Contains("Symbols:", terminalPlot);
        Assert.Contains("* point", terminalPlot);
        Assert.Contains("o discontinuity", terminalPlot);
        Assert.Contains("# overlap", terminalPlot);
        Assert.Contains("Segments:", terminalPlot);
        Assert.Contains("- horizontal", terminalPlot);
        Assert.Contains("/ rising", terminalPlot);
        Assert.Contains("\\ falling", terminalPlot);
        Assert.Contains("| vertical", terminalPlot);
        Assert.Contains("rl", terminalPlot);
        Assert.DoesNotContain("\u001b[", terminalPlot);
        _testOutputHelper.WriteLine(terminalPlot);
    }

    [Fact]
    public void PlotSingleCurveCanForceAnsi()
    {
        var rl = new RateLatencyServiceCurve(1, 3);
        var terminalPlot = rl.ToTerminalPlot(settings: new TerminalPlotSettings
        {
            Width = 32,
            Height = 10,
            AnsiMode = TerminalPlotAnsiMode.Ansi,
            LegendStrategy = LegendStrategy.ForceEnable
        });

        Assert.Contains("\u001b[", terminalPlot);
    }

    [Fact]
    public void PlotMultipleCurvesWithExplicitLimits()
    {
        var sc = new RateLatencyServiceCurve(3, 1);
        var ac = new SigmaRhoArrivalCurve(2, 2);
        var terminalPlot = TerminalPlots.ToTerminalPlot([sc, ac], settings: new TerminalPlotSettings
        {
            Width = 36,
            Height = 12,
            AnsiMode = TerminalPlotAnsiMode.PlainText,
            Title = "terminal plot",
            XLimit = new Interval(-1, 10),
            YLimit = new Interval(-2, 30)
        });

        Assert.Contains("terminal plot", terminalPlot);
        Assert.Contains("-1", terminalPlot);
        Assert.Contains("10", terminalPlot);
        Assert.Contains("1", terminalPlot);
        Assert.Contains("Legend:", terminalPlot);
        _testOutputHelper.WriteLine(terminalPlot);
    }

    [Fact]
    public void AxisTicksPreferSequenceBreakpoints()
    {
        var terminalPlot = TerminalPlots.ToTerminalPlot([BuildSampleSequence()], settings: new TerminalPlotSettings
        {
            Width = 44,
            Height = 12,
            AnsiMode = TerminalPlotAnsiMode.PlainText,
            LegendStrategy = LegendStrategy.ForceEnable,
            XLimit = new Interval(0, 8),
            YLimit = new Interval(0, 10)
        });

        Assert.Contains(terminalPlot.Split(Environment.NewLine), line =>
            line.Contains('0') &&
            line.Contains('2') &&
            line.Contains('5') &&
            line.Contains('8'));
        Assert.Contains(terminalPlot.Split(Environment.NewLine), line => line.TrimStart().StartsWith("5 |"));
        Assert.Contains(terminalPlot.Split(Environment.NewLine), line => line.TrimStart().StartsWith("3 |"));
        _testOutputHelper.WriteLine(terminalPlot);
    }

    [Fact]
    public void AxisTicksCanUseEvenSpacing()
    {
        var terminalPlot = TerminalPlots.ToTerminalPlot([BuildSampleSequence()], settings: new TerminalPlotSettings
        {
            Width = 44,
            Height = 12,
            AnsiMode = TerminalPlotAnsiMode.PlainText,
            LegendStrategy = LegendStrategy.ForceEnable,
            XAxisTickStrategy = TerminalPlotTickStrategy.EvenlySpaced,
            YAxisTickStrategy = TerminalPlotTickStrategy.EvenlySpaced,
            XLimit = new Interval(0, 8),
            YLimit = new Interval(0, 10)
        });

        Assert.Contains(terminalPlot.Split(Environment.NewLine), line =>
            line.Contains('0') &&
            line.Contains('2') &&
            line.Contains('4') &&
            line.Contains('6') &&
            line.Contains('8'));
        Assert.Contains(terminalPlot.Split(Environment.NewLine), line => line.TrimStart().StartsWith("7.5 |"));
        Assert.Contains(terminalPlot.Split(Environment.NewLine), line => line.TrimStart().StartsWith("2.5 |"));
        _testOutputHelper.WriteLine(terminalPlot);
    }

    [Fact]
    public void EvenlySpacedTicksCanUseCompactRationalLabels()
    {
        var terminalPlot = TerminalPlots.ToTerminalPlot([BuildSampleSequence()], settings: new TerminalPlotSettings
        {
            Width = 72,
            Height = 16,
            AnsiMode = TerminalPlotAnsiMode.PlainText,
            LegendStrategy = LegendStrategy.ForceEnable,
            XAxisTickCount = 7,
            YAxisTickCount = 7,
            XAxisTickStrategy = TerminalPlotTickStrategy.EvenlySpaced,
            YAxisTickStrategy = TerminalPlotTickStrategy.EvenlySpaced,
            XLimit = new Interval(0, 10),
            YLimit = new Interval(0, 10)
        });

        Assert.Contains("10/3", terminalPlot);
        Assert.Contains("20/3", terminalPlot);
        Assert.DoesNotContain("3.333", terminalPlot);
        Assert.DoesNotContain("6.667", terminalPlot);
        _testOutputHelper.WriteLine(terminalPlot);
    }

    [Fact]
    public void TickLabelsCanUseDecimalStyle()
    {
        var terminalPlot = TerminalPlots.ToTerminalPlot([BuildSampleSequence()], settings: new TerminalPlotSettings
        {
            Width = 72,
            Height = 16,
            AnsiMode = TerminalPlotAnsiMode.PlainText,
            LegendStrategy = LegendStrategy.ForceEnable,
            TickLabelStyle = TerminalPlotTickLabelStyle.Decimal,
            XAxisTickCount = 7,
            YAxisTickCount = 7,
            XAxisTickStrategy = TerminalPlotTickStrategy.EvenlySpaced,
            YAxisTickStrategy = TerminalPlotTickStrategy.EvenlySpaced,
            XLimit = new Interval(0, 10),
            YLimit = new Interval(0, 10)
        });

        Assert.Contains("3.333", terminalPlot);
        Assert.Contains("6.667", terminalPlot);
        Assert.DoesNotContain("10/3", terminalPlot);
        Assert.DoesNotContain("20/3", terminalPlot);
        _testOutputHelper.WriteLine(terminalPlot);
    }

    private static Sequence BuildSampleSequence()
    {
        return new Sequence([
            Point.Origin(),
            new Segment(startTime: 0, endTime: 2, rightLimitAtStartTime: 1, slope: 1),
            new Point(time: 2, value: 5),
            new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 3, slope: 0),
            new Point(time: 5, value: 3),
            new Segment(startTime: 5, endTime: 8, rightLimitAtStartTime: 3, slope: 2)
        ]);
    }
}
