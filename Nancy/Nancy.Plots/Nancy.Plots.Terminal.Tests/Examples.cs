using System.Text;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Unipi.Nancy.Plots.Terminal;
using Xunit;

namespace Nancy.Plots.Terminal.Tests;

/// <summary>
/// Writes the example plots used to verify the infinity areas by eye.
/// </summary>
/// <remarks>
/// Not assertions: these exist so that the rendering can be looked at.
/// Written without ANSI escapes, so that the file reads as plain text.
/// </remarks>
public class Examples
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Examples(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <remarks>
    /// The size is left at the defaults, which fit the 80 columns by 24 rows of a standard terminal.
    /// </remarks>
    private static readonly TerminalPlotSettings PlainText = new()
    {
        AnsiMode = TerminalPlotAnsiMode.PlainText
    };

    private static TerminalPlotSettings Plain(string title, InfinityStrategy strategy = InfinityStrategy.Areas)
        => PlainText with { Title = title, InfinityStrategy = strategy };

    private static TerminalPlotSettings Ansi(string title)
        => new() { AnsiMode = TerminalPlotAnsiMode.Ansi, Title = title };

    private static IEnumerable<(string Caption, string Plot)> AnsiPlots()
    {
        yield return ("delay(0) and delay(10)", TerminalPlots.ToTerminalPlot(
            (IReadOnlyCollection<Curve>)new List<Curve>
                { new DelayServiceCurve(0), new DelayServiceCurve(10) },
            (IEnumerable<string>)new List<string> { "delta_0", "delta_10" },
            Ansi("delay(0) and delay(10)")));

        yield return ("rate-latency and delay(10)", TerminalPlots.ToTerminalPlot(
            (IReadOnlyCollection<Curve>)new List<Curve>
                { new RateLatencyServiceCurve(1, 3), new DelayServiceCurve(10) },
            (IEnumerable<string>)new List<string> { "beta", "delta_10" },
            Ansi("rate-latency and delay(10)")));
    }

    [Fact]
    public void WriteDocument()
    {
        var sb = new StringBuilder();

        void Add(string caption, string plot)
        {
            sb.AppendLine(new string('=', 78));
            sb.AppendLine(caption);
            sb.AppendLine(new string('=', 78));
            sb.AppendLine(plot);
            sb.AppendLine();
        }

        Add("delay(10)", TerminalPlots.ToTerminalPlot(
            new DelayServiceCurve(10), "delta_10", Plain("delay(10)")));

        Add("delay(0) and delay(10)", TerminalPlots.ToTerminalPlot(
            (IReadOnlyCollection<Curve>)new List<Curve>
                { new DelayServiceCurve(0), new DelayServiceCurve(10) },
            (IEnumerable<string>)new List<string> { "delta_0", "delta_10" },
            Plain("delay(0) and delay(10)")));

        Add("rate-latency and delay(10)", TerminalPlots.ToTerminalPlot(
            (IReadOnlyCollection<Curve>)new List<Curve>
                { new RateLatencyServiceCurve(1, 3), new DelayServiceCurve(10) },
            (IEnumerable<string>)new List<string> { "beta", "delta_10" },
            Plain("rate-latency and delay(10)")));

        var bothInfinities = new Sequence(
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
        Add("both infinities (a raw sequence)", TerminalPlots.ToTerminalPlot(
            (IReadOnlyCollection<Sequence>)new List<Sequence> { bothInfinities },
            (IEnumerable<string>)new List<string> { "f" },
            Plain("both infinities")));

        Add("delay(10), InfinityStrategy.Ignore", TerminalPlots.ToTerminalPlot(
            new DelayServiceCurve(10), "delta_10",
            Plain("delay(10), ignored", InfinityStrategy.Ignore)));

        var directory = Path.Combine(
            Path.GetDirectoryName(typeof(Examples).Assembly.Location)!, "examples");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "examples.txt");
        File.WriteAllText(path, sb.ToString());
        _testOutputHelper.WriteLine(path);

        // the same plots with the escapes kept, since colour is how sequences are told apart here
        var ansi = new StringBuilder();
        foreach (var (caption, plot) in AnsiPlots())
        {
            ansi.AppendLine(new string('=', 78));
            ansi.AppendLine(caption);
            ansi.AppendLine(new string('=', 78));
            ansi.AppendLine(plot);
            ansi.AppendLine();
        }
        var ansiPath = Path.Combine(directory, "examples-ansi.txt");
        File.WriteAllText(ansiPath, ansi.ToString());
        _testOutputHelper.WriteLine(ansiPath);
    }
}
