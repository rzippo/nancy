using System.ComponentModel;
using System.Globalization;
using System.Text;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.TikzPlot;

namespace Unipi.Nancy.Plots.Tikz;

public class TikzPlot
{
    public List<SequenceToPlot> SequencesToPlot { get; set; }

    public TikzPlotSettings Settings { get; set; } = new();

    public TikzPlot(
        List<Sequence> sequences,
        List<string>? names,
        TikzPlotSettings? settings
    )
    {
        if (sequences.Count == 0)
            throw new ArgumentException("Empty trace collection.");

        if (settings != null)
            Settings = settings;

        names ??= sequences.Count > 1 ?
            GetDefaultNames(sequences.Count) :
            [ string.Empty ];
        
        if(names.Count < sequences.Count)
            throw new ArgumentException("Mismatch between number of traces and their names.");

        var colors = GetDefaultColors(sequences.Count);

        var sequencesToPlot = Enumerable.Range(0, sequences.Count)
            .Select(i => new SequenceToPlot()
            {
                Sequence = sequences[i],
                Name = names[i],
                Color = colors[i],
            })
            .ToList();
        SequencesToPlot = sequencesToPlot;
    }

    /// <summary>
    /// Produces the TikZ code for this plot,
    /// which can be written to file and compiled with LaTeX.
    /// </summary>
    /// <remarks>
    /// To compile the TikZ code produced, you need in your .tex preamble:
    /// <code>
    /// \usepackage{tikz}
    /// \usepackage{pgfplots}
    /// \usetikzlibrary{arrows}
    /// </code>
    /// </remarks>
    public string ToTikzCode()
    {
        var sequences = SequencesToPlot
            .Select(stp => stp.Sequence)
            .ToList();
        var names = SequencesToPlot
            .Select(stp => stp.Name)
            .ToList();
        var colors = SequencesToPlot
            .Select(stp => stp.Color)
            .ToList();
        // todo: expose this setting?
        var lineStyles = GetDefaultLineStyles(sequences.Count);

        // todo: use XLim and YLim, if specified

        var xmax = sequences.Max(s => s.DefinedUntil);
        var ymax = sequences.Max(s => s.IsRightClosed ? s.ValueAt(s.DefinedUntil) : s.LeftLimitAt(s.DefinedUntil));

        var xmarks = sequences
            .SelectMany(s => s
                .EnumerateBreakpoints()
                .Select(bp => bp.center.Time))
            .Where(x => x.IsFinite)
            .OrderBy(x => x)
            .Distinct()
            .ToList();

        var ymarks = sequences
            .SelectMany(s => s
                .EnumerateBreakpoints()
                .GetBreakpointsBoundaryValues())
            .Where(y => y.IsFinite)
            .OrderBy(y => y)
            .Distinct()
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLines(GetTikzHeader(xmax, ymax, xmarks, ymarks, Settings));

        var includeLegend = Settings.LegendStrategy switch
        {
            // By default, the legend is omitted if a single curve or sequence is being plotted,
            // unless the name was specified manually
            LegendStrategy.Auto => sequences.Count > 1 || !string.IsNullOrWhiteSpace(SequencesToPlot.Single().Name),
            LegendStrategy.ForceEnable => true,
            LegendStrategy.ForceDisable => false,
            _ => true
        };
        
        sb.AppendLines(GetTikzContent(sequences, names, colors, lineStyles, Settings, includeLegend));

        // todo: how to handle UPP marks?
        // if (curves.Count == 1 && curves.Single() is { IsUltimatelyInfinite: false } f)
        // {
        //     sb.AppendLines(GetUppMarks(f, names[0]));
        // }
        
        sb.AppendLines(GetTikzFooter());
        
        return sb.ToString();
    }

    /// <summary>
    /// Default colors.
    /// </summary>
    public static List<string> DefaultColorList =
    [
        "blue!60!black",
        "green!60!black",
        "red!60!black"
    ];

    /// <summary>
    /// Get a list of <paramref name="n"/> colors from the default ones.
    /// If <paramref name="n"/> is more than the number of default colors, they are repeated.
    /// </summary>
    public static List<string> GetDefaultColors(int n)
    {
        var result = new List<string>();
        for (int i = 0; i < n; i++)
        {
            result.Add(DefaultColorList[i % DefaultColorList.Count]);
        }

        return result;
    }

    /// <summary>
    /// Get a list of <paramref name="n"/> default names.
    /// These are lowercase letters, e.g., [f, g, h].
    /// </summary>
    /// <param name="n">The number of names to return.</param>
    /// <param name="firstLetter">The starting letter, defaults to 'f'.</param>
    public static List<string> GetDefaultNames(int n, char firstLetter = 'f')
    {
        var result = new List<string>();
        for (int i = 0; i < n; i++)
        {
            var round = i / 27;
            var index = i % 27;
            var indexFromA = ((firstLetter - 'a') + index) % 27;
            var letter = (char) ('a' + indexFromA);
            if(round > 0)
                result.Add($"{letter}{round}");
            else
                result.Add(letter.ToString());
        }

        return result;
    }

    /// <summary>
    /// Get a list of <paramref name="n"/> default line styles.
    /// </summary>
    public static List<string> GetDefaultLineStyles(int n)
    {
        var result = new List<string>();
        for (int i = 0; i < n; i++)
        {
            result.Add("solid");
        }

        return result;
    }

    /// <summary>
    /// Handy method that concatenates n tabs.  
    /// </summary>
    private static string Tabs(int n)
    {
        var sbt = new StringBuilder();
        for (int i = 0; i < n; i++)
            sbt.Append("\t");
        return sbt.ToString();
    }

    /// <summary>
    /// Computes the header for the plot.
    /// </summary>
    /// <param name="xmax"></param>
    /// <param name="ymax"></param>
    /// <param name="xmarks"></param>
    /// <param name="ymarks"></param>
    /// <param name="settings"></param>
    private static IEnumerable<string> GetTikzHeader(
        Rational xmax, 
        Rational ymax,
        List<Rational>? xmarks = null,
        List<Rational>? ymarks = null,
        TikzPlotSettings? settings = null)
    {
        settings ??= new ();

        yield return $"\\begin{{tikzpicture}}";
        yield return $"{Tabs(1)}\\begin{{axis}}[";
        yield return $"{Tabs(2)}font = {settings.FontSize.ToLatex()},";
        yield return $"{Tabs(2)}clip = false,";

        switch (settings.GridTickLayout)
        {
            case GridTickLayout.Auto:
                yield return $"{Tabs(2)}grid = both,";
                yield return $"{Tabs(2)}minor tick num = 1,";
                break;

            case GridTickLayout.SquareGrid:
            case GridTickLayout.SquareGridNoLabels:
                yield return $"{Tabs(2)}grid = major,";
                break;

            default:
                yield return $"{Tabs(2)}grid = both,";
                break;
        }

        yield return $"{Tabs(2)}grid style = {{draw=gray!30}},";
        yield return $"{Tabs(2)}axis lines = left,";
        yield return $"{Tabs(2)}axis equal image,";
        yield return $"{Tabs(2)}xlabel = time,";
        yield return $"{Tabs(2)}ylabel = data,";
        var xLabelAnchor = settings.GridTickLayout switch {
            GridTickLayout.SquareGridNoLabels => "north",
            _ => "north west"
        };
        yield return $"{Tabs(2)}x label style = {{at={{(axis description cs:1,0)}},anchor={xLabelAnchor}}},";
        yield return $"{Tabs(2)}y label style = {{at={{(axis description cs:0,1)}},rotate=-90,anchor=south}},";
        yield return $"{Tabs(2)}xmin = 0,";
        yield return $"{Tabs(2)}ymin = 0,";

        switch (settings.GridTickLayout)
        {
            case GridTickLayout.Auto:
            {
                yield return FormattableString.Invariant($"{Tabs(2)}xmax = {(decimal) xmax + 1},");
                yield return FormattableString.Invariant($"{Tabs(2)}ymax = {(decimal) ymax + 1},");
                yield return $"{Tabs(2)}xticklabels = \\empty,";
                yield return $"{Tabs(2)}yticklabels = \\empty,";

                if (xmarks != null)
                {   
                    var sb = new StringBuilder();
                    sb.Append($"{Tabs(2)}extra x ticks = {{ ");
                    foreach (var xmark in xmarks)
                    {
                        sb.Append(((decimal) xmark).ToString(CultureInfo.InvariantCulture));
                        sb.Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(" },");
                    yield return sb.ToString();
                }

                if (ymarks != null)
                {
                    var sb = new StringBuilder();
                    sb.Append($"{Tabs(2)}extra y ticks = {{ ");
                    foreach (var ymark in ymarks)
                    {
                        sb.Append(((decimal) ymark).ToString(CultureInfo.InvariantCulture));
                        sb.Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(" },");
                    yield return sb.ToString();
                }
                break;
            }

            case GridTickLayout.SquareGrid:
            case GridTickLayout.SquareGridNoLabels:
            {
                var xceil = (int) Math.Ceiling((decimal) xmax + 1);
                var yceil = (int) Math.Ceiling((decimal) ymax + 1);
                yield return FormattableString.Invariant($"{Tabs(2)}xmax = {xceil},");
                yield return FormattableString.Invariant($"{Tabs(2)}ymax = {yceil},");

                // xtick
                {
                    var sb = new StringBuilder();
                    sb.Append($"{Tabs(2)}xtick = {{ ");
                    for (int i = 1; i <= xceil; i++)
                    {
                        sb.Append(i.ToString(CultureInfo.InvariantCulture));
                        sb.Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(" },");
                    yield return sb.ToString();
                }

                // ytick
                {
                    var sb = new StringBuilder();
                    sb.Append($"{Tabs(2)}ytick = {{ ");
                    for (int i = 1; i <= yceil; i++)
                    {
                        sb.Append(i.ToString(CultureInfo.InvariantCulture));
                        sb.Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(" },");
                    yield return sb.ToString();
                }

                yield return $"{Tabs(2)}minor xtick = {{}},";
                yield return $"{Tabs(2)}minor ytick = {{}},";
                if (settings.GridTickLayout == GridTickLayout.SquareGridNoLabels)
                {
                    yield return $"{Tabs(2)}xticklabels = \\empty,";
                    yield return $"{Tabs(2)}yticklabels = \\empty,";
                }
                break;
            }
        }

        yield return $"{Tabs(2)}legend pos = {settings.LegendPosition.ToLatex()}";
        yield return $"{Tabs(1)}]";
    }

    /// <summary>
    /// Computes the footer for the plot.
    /// </summary>
    private static IEnumerable<string> GetTikzFooter()
    {
        yield return $"{Tabs(1)}\\end{{axis}}";
        yield return "\\end{tikzpicture}";
    }

    /// <summary>
    /// Computes the marks to denote the periodic behavior of the curve.
    /// </summary>
    /// <param name="f"></param>
    /// <param name="name"></param>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IEnumerable<string> GetUppMarks(Curve f, string name)
    {
        return _getUppMarks().Select(FormattableString.Invariant);

        IEnumerable<FormattableString> _getUppMarks()
        {
            var marksColor = "black!60";
            var marksStyle = "thick, densely dashed";
            var arrowStyle = "thick, <->";

            var tf = (decimal)f.PseudoPeriodStart;
            var ftf = (decimal)f.ValueAt(f.PseudoPeriodStart);

            if(ftf > 0)
                yield return $"{Tabs(2)}\\addplot [ color = {marksColor}, {marksStyle} ] coordinates {{ ({tf}, 0) ({tf}, {ftf}) }};";
            yield return $"{Tabs(2)}\\node [ anchor = north ] at (axis cs:{tf}, 0) {{$T_{{{name}}}$}};";
            yield return $"";

            var tfdf = (decimal)f.FirstPseudoPeriodEnd;
            var ftfdf = (decimal)f.ValueAt(f.FirstPseudoPeriodEnd);
            var ftf2df = (decimal)f.ValueAt(f.SecondPseudoPeriodEnd);

            yield return
                $"{Tabs(2)}\\addplot [ color = {marksColor}, {marksStyle} ] coordinates {{ ({tfdf}, {ftfdf}) ({tfdf}, {(ftfdf + ftf2df) / 2}) }};";
            yield return
                $"{Tabs(2)}\\addplot [ color = {marksColor}, {marksStyle} ] coordinates {{ ({tf}, {ftf}) ({tf}, {(ftfdf + ftf2df) / 2}) }};";
            yield return
                $"{Tabs(2)}\\addplot [ color = {marksColor}, {arrowStyle} ] coordinates {{ ({tf}, {(ftfdf + ftf2df) / 2}) ({tfdf}, {(ftfdf + ftf2df) / 2}) }};";
            yield return $"{Tabs(2)}\\node [ anchor = south ] at (axis cs:{(tf + tfdf) / 2}, {(ftfdf + ftf2df) / 2}) {{$d_{{{name}}}$}};";
            yield return $"";

            var tf2df = (decimal)f.SecondPseudoPeriodEnd;

            if (f.PseudoPeriodHeight > 0)
            {
                yield return
                    $"{Tabs(2)}\\addplot [ color = {marksColor}, {marksStyle} ] coordinates {{ ({tf}, {ftf}) ({(tfdf + tf2df) / 2}, {ftf}) }};";
                yield return
                    $"{Tabs(2)}\\addplot [ color = {marksColor}, {marksStyle} ] coordinates {{ ({tfdf}, {ftfdf}) ({(tfdf + tf2df) / 2}, {ftfdf}) }};";
                yield return
                    $"{Tabs(2)}\\addplot [ color = {marksColor}, {arrowStyle} ] coordinates {{ ({(tfdf + tf2df) / 2}, {ftf}) ({(tfdf + tf2df) / 2}, {ftfdf}) }};";
                yield return
                    $"{Tabs(2)}\\node [ anchor = west ] at (axis cs:{(tfdf + tf2df) / 2}, {(ftf + ftfdf) / 2}) {{$c_{{{name}}}$}};";
                yield return $"";
            }
            else
            {
                yield return
                    $"{Tabs(2)}\\node [ anchor = south west ] at (axis cs:{(tfdf + tf2df) / 2}, {(ftf + ftfdf) / 2}) {{$c_{{{name}}}$}};";
                yield return $"";
            }
        }
    }

    /// <summary>
    /// Get the TikZ lines that plot the sequences using TikZ.
    /// </summary>
    /// <param name="sequences"></param>
    /// <param name="names"></param>
    /// <param name="colors"></param>
    /// <param name="lineStyles"></param>
    /// <param name="settings"></param>
    /// <param name="includeLegend"></param>
    // ReSharper disable once MemberCanBePrivate.Global
    private static IEnumerable<string> GetTikzContent(
        IReadOnlyList<Sequence> sequences, 
        IReadOnlyList<string> names, 
        IReadOnlyList<string> colors,
        IReadOnlyList<string> lineStyles,
        TikzPlotSettings settings,
        bool includeLegend
    )
    {
        if (sequences.Count != names.Count || sequences.Count != colors.Count)
            throw new InvalidEnumArgumentException("The arguments must be of the same length");
        if (sequences.Any(s => s.FirstFiniteTime.IsPlusInfinite))
            throw new InvalidEnumArgumentException("Cannot plot infinite-only sequences");

        var plots = sequences
            .Select((s, i) => ToTikzExtensions.ToTikzLines(s, colors[i], lineStyles[i], settings).ToList())
            .ToList();

        if (!includeLegend)
        {
            // we just plot each sequence in order
            foreach (var (plot, i) in plots.WithIndex())
            {
                yield return $"{Tabs(2)}% {names[i]}";
                foreach (var line in plot)
                    yield return $"{Tabs(2)}{line}";
                yield return "";
            }
        }
        else
        {
            // needs to be more complex to correctly plot the legend,
            // one element for each sequence must be selected and plotted out of order. 

            yield return $"{Tabs(2)}% lines out of order, for the legend";
            var legendLines = plots.Select((plot, i) =>
                {
                    var firstSegmentLine = plot.FirstOrDefault(l => !l.Contains("only marks"));
                    var legendLine = firstSegmentLine ?? plot.First();
                    return legendLine;
                })
                .ToList();

            foreach (var legendLine in legendLines)
                yield return $"{Tabs(2)}{legendLine}";

            foreach (var name in names)
                yield return $"{Tabs(2)}\\addlegendentry{{$ {name} $}};";
            yield return "";

            foreach (var (plot, i) in plots.WithIndex())
            {
                yield return $"{Tabs(2)}% {names[i]}";
                foreach (var line in plot)
                {
                    if (line == legendLines[i])
                        continue;
                    else
                        yield return $"{Tabs(2)}{line}";
                }

                yield return "";
            }
        }
    }
}

public record SequenceToPlot
{
    public required Sequence Sequence { get; init; }
    public required string Color { get; init; }
    public string Name { get; init; } = string.Empty;
}

static class ToTikzExtensions
{
    /// <summary>
    /// Computes the lines that plot the sequence.
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="color"></param>
    /// <param name="lineStyle"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static IEnumerable<string> ToTikzLines(
        this Sequence sequence, 
        string color, 
        string? lineStyle = null,
        TikzPlotSettings? settings = null
    )
    {
        settings ??= new TikzPlotSettings();
        if (lineStyle != null)
        {
            if (lineStyle.EndsWith(","))
                lineStyle += " ";
            else if (!lineStyle.EndsWith(", "))
                lineStyle += ", ";
        }
        else
        {
            lineStyle = "";
        }

        switch (settings.CurveLayout)
        {
            case CurveLayout.SimplifyContinuous:
            case CurveLayout.SimplifyContinuousWithMarks:
            {
                using var enumerator = sequence.Elements.GetEnumerator();
                enumerator.MoveNext();

                // global terminator, it is false when there are no more elements to plot
                var keepLooping = true;

                // this while body runs once for each continuous sequence
                while (keepLooping)
                {
                    var isStartClosed = enumerator.Current is Point;
                    var isEndClosed = false;
                    var breakpoints = new List<(Rational time, Rational value)> { };

                    // this while body runs once per element
                    while (keepLooping)
                    {
                        // first, it skips all next infinite elements
                        while (enumerator.Current.IsInfinite)
                        {
                            if (!enumerator.MoveNext())
                            {
                                keepLooping = false;
                                break;
                            }
                        }

                        if (!keepLooping)
                            break;

                        var nextValue = enumerator.Current switch
                        {
                            Point p => p.Value,
                            Segment s => s.RightLimitAtStartTime,
                            _ => throw new InvalidCastException()
                        };
                        Rational? lastValue = breakpoints.Count > 0 ? breakpoints.Last().value : null;

                        if (lastValue != null && nextValue != lastValue)
                        {
                            // we found a discontinuity, break the inner loop to plot the current continuous sequence
                            isEndClosed = enumerator.Current is Segment;
                            break;
                        }
                        else
                        {
                            // sequence is still continuous, move on
                            isEndClosed = enumerator.Current is Point;
                            if (breakpoints.Count == 0)
                            {
                                var point = enumerator.Current switch
                                {
                                    Point p => (p.Time, p.Value),
                                    Segment s => (s.StartTime, s.RightLimitAtStartTime),
                                    _ => throw new InvalidCastException()
                                };
                                breakpoints.Add(point);
                            }

                            if (enumerator.Current is Segment seg)
                            {
                                var point = (seg.EndTime, seg.LeftLimitAtEndTime);
                                breakpoints.Add(point);
                            }
                        }

                        // move to next element, if any, and continue the inner loop
                        if (!enumerator.MoveNext())
                        {
                            keepLooping = false;
                            break;
                        }
                    }

                    // the plotting step, after the inner loop ends
                    if(breakpoints.Count > 0)
                        foreach (var line in plotContinuousSequence())
                            yield return line;

                    // Logic for the actual plotting
                    IEnumerable<string> plotContinuousSequence()
                    {
                        if (isStartClosed || breakpoints.Count == 1)
                        {
                            // plot mark for starting point
                            var x = (decimal) breakpoints.First().time;
                            var y = (decimal) breakpoints.First().value;
                            yield return FormattableString.Invariant($"\\addplot [ color = {color}, thick, only marks, mark size = 1pt ] coordinates {{ ({x},{y}) }};");
                        }

                        if(breakpoints.Count == 1)
                            yield break;

                        {
                            // plot line for continuous sequence
                            var sb = new StringBuilder();
                            var leftBracket = isStartClosed ? "" : ")";
                            var rightBracket = isEndClosed ? "" : "(";
                            var shortenLeft = isStartClosed ? "" : "shorten < = 1pt, ";
                            var shortenRight = isEndClosed ? "" : "shorten > = 1pt, ";

                            var header =
                                $"\\addplot [ color = {color}, thick, {leftBracket}-{rightBracket}, {lineStyle}{shortenLeft}{shortenRight} ] coordinates {{ ";
                            header = header.Replace(",  ]", " ]");
                            sb.Append(header);

                            foreach (var breakpoint in breakpoints)
                            {
                                var x = (decimal)breakpoint.time;
                                var y = (decimal)breakpoint.value;
                                sb.Append(FormattableString.Invariant($"({x}, {y}) "));
                            }

                            sb.AppendLine($"}};");
                            yield return sb.ToString();
                        }

                        if (settings.CurveLayout == CurveLayout.SimplifyContinuousWithMarks)
                        {
                            // plot marks for internal points
                            var sb = new StringBuilder();
                            sb.Append(
                                $"\\addplot [ color = {color}, thick, only marks, mark size = 1pt ] coordinates {{ ");
                            foreach (var breakpoint in breakpoints.Skip(1).SkipLast(1))
                            {
                                var x = (decimal)breakpoint.time;
                                var y = (decimal)breakpoint.value;
                                sb.Append(FormattableString.Invariant($"({x}, {y}) "));
                            }
                            sb.AppendLine($"}};");
                        }

                        if (isEndClosed)
                        {
                            // plot mark for ending point
                            var x = (decimal) breakpoints.Last().time;
                            var y = (decimal) breakpoints.Last().value;
                            yield return FormattableString.Invariant($"\\addplot [ color = {color}, thick, only marks, mark size = 1pt ] coordinates {{ ({x},{y}) }};");
                        }
                    }
                }

                // ends the case statement
                break;
            }

            case CurveLayout.SplitAllElements:
            default:
            {
                foreach (var element in sequence.Elements.Where(e => e.IsFinite))
                    yield return element.ToTikzLine(color, lineStyle);
                break;
            }
        }
    }

    /// <summary>
    /// Computes the lines that plot the element.
    /// </summary>
    public static string ToTikzLine(this Element element, string color, string? lineStyle = null)
    {
        if (element.IsInfinite)
            throw new InvalidOperationException("Cannot plot infinities.");

        if (lineStyle != null)
        {
            if (lineStyle.EndsWith(","))
                lineStyle += " ";
            else if (!lineStyle.EndsWith(", "))
                lineStyle += ", ";
        }
        else
        {
            lineStyle = "";
        }

        FormattableString line;
        if(element is Point p)
        {
            var x = (decimal)p.Time;
            var y = (decimal)p.Value;
            line = $"\\addplot [ color = {color}, thick, only marks, mark size = 1pt ] coordinates {{ ({x},{y}) }};";
        }
        else if (element is Segment s)
        {
            var x1 = (decimal) s.StartTime;
            var y1 = (decimal) s.RightLimitAtStartTime;
            var x2 = (decimal) s.EndTime;
            var y2 = (decimal) s.LeftLimitAtEndTime;
            line = $"\\addplot [ color = {color}, thick, )-(, {lineStyle}shorten > = 1pt, shorten < = 1pt ] coordinates {{ ({x1},{y1}) ({x2},{y2}) }};";
        }
        else
        {
            throw new InvalidCastException();
        }

        return FormattableString.Invariant(line);
    }
}