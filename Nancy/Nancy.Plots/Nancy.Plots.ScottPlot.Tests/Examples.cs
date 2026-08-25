using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Unipi.Nancy.Plots.ScottPlot;
using Xunit;

namespace Nancy.Plots.ScottPlot.Tests;

/// <summary>
/// Writes the example images used to verify line styles and infinity areas by eye.
/// </summary>
/// <remarks>
/// Not assertions: these exist so that the rendering can be looked at.
/// The output directory is printed by each test.
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

    private void Write(string name, byte[] image)
    {
        var path = Path.Combine(OutputDirectory, $"{name}.png");
        File.WriteAllBytes(path, image);
        _testOutputHelper.WriteLine(path);
    }

    private void Write(
        string name,
        IReadOnlyCollection<Curve> curves,
        IEnumerable<string> names,
        ScottPlotSettings? settings = null)
    {
        settings ??= new ScottPlotSettings();
        settings.Title = name;
        Write(name, ScottPlots.ToScottPlotImage(curves, names, settings));
    }

    [Fact]
    public void Delay0()
    {
        Write("delay-0", [new DelayServiceCurve(0)], ["\\delta_0"]);
    }

    [Fact]
    public void Delay10()
    {
        Write("delay-10", [new DelayServiceCurve(10)], ["\\delta_10"]);
    }

    [Fact]
    public void Delay0AndDelay10()
    {
        Write(
            "delay-0-and-10",
            [new DelayServiceCurve(0), new DelayServiceCurve(10)],
            ["delta_0", "delta_10"]);
    }

    [Fact]
    public void RateLatencyAndDelay()
    {
        Write(
            "rate-latency-and-delay",
            [new RateLatencyServiceCurve(1, 3), new DelayServiceCurve(10)],
            ["beta", "delta_10"]);
    }

    [Fact]
    public void SixCurves()
    {
        Write(
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
    }

    [Fact]
    public void SixCurvesLegendOutside()
        => Write(
            "six-curves-legend-outside",
            [
                new RateLatencyServiceCurve(1, 3),
                new RateLatencyServiceCurve(2, 5),
                new SigmaRhoArrivalCurve(2, 1),
                new SigmaRhoArrivalCurve(5, 2),
                new RateLatencyServiceCurve(3, 8),
                new SigmaRhoArrivalCurve(1, 3)
            ],
            ["a", "b", "c", "d", "e", "f"],
            new ScottPlotSettings { LegendPlacement = LegendPlacement.Outside });

    [Fact]
    public void SixCurvesWithoutLineStyles()
    {
        Write(
            "six-curves-no-line-styles",
            [
                new RateLatencyServiceCurve(1, 3),
                new RateLatencyServiceCurve(2, 5),
                new SigmaRhoArrivalCurve(2, 1),
                new SigmaRhoArrivalCurve(5, 2),
                new RateLatencyServiceCurve(3, 8),
                new SigmaRhoArrivalCurve(1, 3)
            ],
            ["a", "b", "c", "d", "e", "f"],
            new ScottPlotSettings { UseLineStyles = false });
    }

    /// <summary>
    /// A curve that is finite, then $+\infty$ over an interval, then finite again.
    /// </summary>
    /// <remarks>
    /// Exercises an area whose two vertical edges are both drawn, one excluded and one excluded.
    /// </remarks>
    [Fact]
    public void InteriorPlateau()
    {
        var sequence = new Sequence(new Element[]
        {
            new Point(0, 0),
            new Segment(0, 5, 0, 1),
            Point.PlusInfinite(5),
            Segment.PlusInfinite(5, 8),
            new Point(8, 3),
            new Segment(8, 12, 3, 1),
            new Point(12, 7)
        });
        var settings = new ScottPlotSettings { Title = "interior-plateau" };
        Write("interior-plateau", ScottPlots.ToScottPlotImage([sequence], ["f"], settings));
    }

    /// <summary>
    /// A curve reaching $-\infty$, for the area below the x-axis and the reserved lower limit.
    /// </summary>
    [Fact]
    public void MinusInfinity()
    {
        var sequence = new Sequence(new Element[]
        {
            new Point(0, 0),
            new Segment(0, 5, 0, 1),
            Point.MinusInfinite(5),
            Segment.MinusInfinite(5, 12)
        });
        var settings = new ScottPlotSettings { Title = "minus-infinity" };
        Write("minus-infinity", ScottPlots.ToScottPlotImage([sequence], ["f"], settings));
    }

    /// <summary>
    /// A curve with both a $+\infty$ and a $-\infty$ part, so both areas appear at once.
    /// </summary>
    [Fact]
    public void BothInfinities()
    {
        var sequence = new Sequence(new Element[]
        {
            new Point(0, 0),
            new Segment(0, 4, 0, 1),
            Point.PlusInfinite(4),
            Segment.PlusInfinite(4, 7),
            new Point(7, 2),
            new Segment(7, 10, 2, 1),
            Point.MinusInfinite(10),
            Segment.MinusInfinite(10, 14)
        });
        var settings = new ScottPlotSettings { Title = "both-infinities" };
        Write("both-infinities", ScottPlots.ToScottPlotImage([sequence], ["f"], settings));
    }

    /// <summary>
    /// A non-positive curve, which should get its room below and only a sliver above.
    /// </summary>
    [Fact]
    public void NegativeRateLatency()
    {
        Write("negative-rate-latency", [-new RateLatencyServiceCurve(1, 1)], ["-beta"]);
    }

    /// <summary>
    /// A curve whose only finite value is 0, and whose sign is told by its $-\infty$ part.
    /// </summary>
    [Fact]
    public void NegativeDelay10()
    {
        Write("negative-delay-10", [-new DelayServiceCurve(10)], ["-delta_10"]);
    }

    /// <summary>
    /// The same curve without the areas, where the sign is still read off the infinite part.
    /// </summary>
    [Fact]
    public void NegativeDelay10Ignored()
    {
        Write(
            "negative-delay-10-ignored",
            [-new DelayServiceCurve(10)],
            ["-delta_10"],
            new ScottPlotSettings { InfinityStrategy = InfinityStrategy.Ignore });
    }

    [Fact]
    public void Delay10Ignored()
    {
        Write(
            "delay-10-ignored",
            [new DelayServiceCurve(10)],
            ["delta_10"],
            new ScottPlotSettings { InfinityStrategy = InfinityStrategy.Ignore });
    }
}
