using System.Text;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Unipi.Nancy.Plots.Tikz;
using Xunit;

namespace Nancy.Plots.Tikz.Tests;

/// <summary>
/// Writes a LaTeX document gathering the cases used to verify line styles and infinity areas by eye.
/// </summary>
/// <remarks>
/// Not assertions: these exist so that the rendering can be looked at.
/// Compile the result with the Docker image, never the local installation.
/// </remarks>
public class Examples
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Examples(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private static Sequence InteriorPlateau() => new (new Element[]
    {
        new Point(0, 0),
        new Segment(0, 5, 0, 1),
        Point.PlusInfinite(5),
        Segment.PlusInfinite(5, 8),
        new Point(8, 3),
        new Segment(8, 12, 3, 1),
        new Point(12, 7)
    });

    private static Sequence BothInfinities() => new (new Element[]
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

    private static readonly IReadOnlyList<Curve> SixCurves =
    [
        new RateLatencyServiceCurve(1, 3),
        new RateLatencyServiceCurve(2, 5),
        new SigmaRhoArrivalCurve(2, 1),
        new SigmaRhoArrivalCurve(5, 2),
        new RateLatencyServiceCurve(3, 8),
        new SigmaRhoArrivalCurve(1, 3)
    ];

    [Fact]
    [Trait("Category", "Smoke")]
    public void WriteDocument()
    {
        var cases = new List<(string Caption, string Code)>
        {
            ("delay(0)", TikzPlots.ToTikzPlotCode(new DelayServiceCurve(0), "delta_0")),
            ("delay(10)", TikzPlots.ToTikzPlotCode(new DelayServiceCurve(10), "delta_{10}")),
            ("delay(0) and delay(10)", TikzPlots.ToTikzPlotCode(
                (IReadOnlyCollection<Curve>)new List<Curve>
                    { new DelayServiceCurve(0), new DelayServiceCurve(10) },
                (IEnumerable<string>)new List<string> { "delta_0", "delta_{10}" })),
            ("rate-latency and delay(10)", TikzPlots.ToTikzPlotCode(
                (IReadOnlyCollection<Curve>)new List<Curve>
                    { new RateLatencyServiceCurve(1, 3), new DelayServiceCurve(10) },
                (IEnumerable<string>)new List<string> { "beta", "delta_{10}" })),
            ("six curves, line styles cycling", TikzPlots.ToTikzPlotCode(
                SixCurves, (IEnumerable<string>)new List<string> { "a", "b", "c", "d", "e", "f" })),
            ("six curves, LegendPlacement.Outside", TikzPlots.ToTikzPlotCode(
                SixCurves, (IEnumerable<string>)new List<string> { "a", "b", "c", "d", "e", "f" },
                new TikzPlotSettings { LegendPlacement = LegendPlacement.Outside })),
            ("six curves, GridTickLayout.Breakpoints (a tick per breakpoint)", TikzPlots.ToTikzPlotCode(
                SixCurves, (IEnumerable<string>)new List<string> { "a", "b", "c", "d", "e", "f" },
                new TikzPlotSettings { GridTickLayout = GridTickLayout.Breakpoints })),
            ("six curves, UseLineStyles = false", TikzPlots.ToTikzPlotCode(
                SixCurves, (IEnumerable<string>)new List<string> { "a", "b", "c", "d", "e", "f" },
                new TikzPlotSettings { UseLineStyles = false })),
            ("interior plateau (a raw sequence: must not reach the edge)", TikzPlots.ToTikzPlotCode(
                (IReadOnlyCollection<Sequence>)new List<Sequence> { InteriorPlateau() },
                (IEnumerable<string>)new List<string> { "f" })),
            ("both infinities (a raw sequence)", TikzPlots.ToTikzPlotCode(
                (IReadOnlyCollection<Sequence>)new List<Sequence> { BothInfinities() },
                (IEnumerable<string>)new List<string> { "f" })),
            ("-delay(10)", TikzPlots.ToTikzPlotCode(-new DelayServiceCurve(10), "-delta_{10}")),
            ("-rate-latency(1,1)", TikzPlots.ToTikzPlotCode(
                -new RateLatencyServiceCurve(1, 1), "-beta")),
            ("delay(10), InfinityStrategy.Ignore", TikzPlots.ToTikzPlotCode(
                new DelayServiceCurve(10), "delta_{10}",
                new TikzPlotSettings { InfinityStrategy = InfinityStrategy.Ignore }))
        };

        var sb = new StringBuilder();
        sb.AppendLine(@"\documentclass[a4paper]{article}");
        sb.AppendLine(@"\usepackage[margin=1.5cm]{geometry}");
        sb.AppendLine(@"\usepackage{tikz}");
        sb.AppendLine(@"\usepackage{pgfplots}");
        sb.AppendLine(@"\usetikzlibrary{arrows}");
        sb.AppendLine(@"\usetikzlibrary{patterns}");
        sb.AppendLine(@"\pgfplotsset{compat=1.18}");
        sb.AppendLine(@"\begin{document}");
        foreach (var (caption, code) in cases)
        {
            sb.AppendLine(@"\section*{" + caption.Replace("_", @"\_") + "}");
            sb.AppendLine(code);
            sb.AppendLine(@"\clearpage");
        }
        sb.AppendLine(@"\end{document}");

        var directory = Path.Combine(
            Path.GetDirectoryName(typeof(Examples).Assembly.Location)!, "examples");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "examples.tex");
        File.WriteAllText(path, sb.ToString());
        _testOutputHelper.WriteLine(path);
    }
}
