using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Unipi.Nancy.Plots.XPlot.Plotly;
using Xunit;

namespace Nancy.Plots.XPlot.Plotly.Tests;

/// <summary>
/// Writes the example pages used to verify line styles and infinity areas by eye.
/// </summary>
/// <remarks>
/// Not assertions: these exist so that the rendering can be looked at in a browser.
/// </remarks>
public class Examples
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Examples(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private static string OutputDirectory
    {
        get
        {
            var path = Path.Combine(
                Path.GetDirectoryName(typeof(Examples).Assembly.Location)!,
                "examples");
            Directory.CreateDirectory(path);
            return path;
        }
    }

    private void Write(string name, string html)
    {
        var path = Path.Combine(OutputDirectory, $"{name}.html");
        File.WriteAllText(path, html);
        _testOutputHelper.WriteLine(path);
    }

    private void Write(
        string name,
        IReadOnlyCollection<Curve> curves,
        IEnumerable<string> names,
        XPlotPlotSettings? settings = null)
    {
        settings ??= new XPlotPlotSettings();
        settings.Title = name;
        Write(name, XPlotPlots.ToXPlotHtml(curves, names, settings));
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public void Delay10()
        => Write("delay-10", [new DelayServiceCurve(10)], ["delta_10"]);

    [Fact]
    [Trait("Category", "Smoke")]
    public void Delay0AndDelay10()
        => Write(
            "delay-0-and-10",
            [new DelayServiceCurve(0), new DelayServiceCurve(10)],
            ["delta_0", "delta_10"]);

    [Fact]
    [Trait("Category", "Smoke")]
    public void RateLatencyAndDelay()
        => Write(
            "rate-latency-and-delay",
            [new RateLatencyServiceCurve(1, 3), new DelayServiceCurve(10)],
            ["beta", "delta_10"]);

    [Fact]
    [Trait("Category", "Smoke")]
    public void SixCurves()
        => Write(
            "six-curves",
            [
                new RateLatencyServiceCurve(1, 3),
                new RateLatencyServiceCurve(2, 5),
                new SigmaRhoArrivalCurve(2, 1),
                new SigmaRhoArrivalCurve(5, 2),
                new RateLatencyServiceCurve(3, 8),
                new SigmaRhoArrivalCurve(1, 3)
            ],
            ["a", "b", "c", "d", "e", "f"]);

    /// <summary>
    /// A finite curve beside one that is infinite throughout, which used to throw.
    /// </summary>
    /// <remarks>
    /// An everywhere-infinite sequence is continuous, so it took a fast path that cast infinity to decimal.
    /// </remarks>
    [Fact]
    [Trait("Category", "Smoke")]
    public void FiniteBesideAnEverywhereInfiniteSequence()
    {
        var infinite = new Sequence(
        [
            Point.PlusInfinite(0),
            Segment.PlusInfinite(0, 10),
            Point.PlusInfinite(10)
        ]);
        var finite = new Sequence(
        [
            new Point(0, 0),
            new Segment(0, 10, 0, 1),
            new Point(10, 10)
        ]);
        var settings = new XPlotPlotSettings { Title = "finite-and-everywhere-infinite" };

        Write(
            "finite-and-everywhere-infinite",
            XPlotPlots.ToXPlotHtml(
                (IEnumerable<Sequence>)new List<Sequence> { finite, infinite },
                (IEnumerable<string>)new List<string> { "f", "g" },
                settings));
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public void BothInfinities()
    {
        var sequence = new Sequence(
        [
            new Point(0, 0),
            new Segment(0, 4, 0, 1),
            Point.PlusInfinite(4),
            Segment.PlusInfinite(4, 7),
            new Point(7, 2),
            new Segment(7, 10, 2, 1),
            Point.MinusInfinite(10),
            Segment.MinusInfinite(10, 14)
        ]);
        var settings = new XPlotPlotSettings { Title = "both-infinities" };

        Write(
            "both-infinities",
            XPlotPlots.ToXPlotHtml(
                (IEnumerable<Sequence>)new List<Sequence> { sequence },
                (IEnumerable<string>)new List<string> { "f" },
                settings));
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public void Delay10Ignored()
        => Write(
            "delay-10-ignored",
            [new DelayServiceCurve(10)],
            ["delta_10"],
            new XPlotPlotSettings { InfinityStrategy = InfinityStrategy.Ignore });
}
