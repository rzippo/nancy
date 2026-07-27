using System.ComponentModel;
using System.Globalization;
using System.Text;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Unipi.Nancy.TikzPlot;

namespace Unipi.Nancy.Plots.Tikz;

/// <summary>
/// Represents a TikZ plot built from one or more sequences.
/// </summary>
public class TikzPlot
{
    /// <summary>
    /// Sequences included in the plot.
    /// </summary>
    public List<SequenceToPlot> SequencesToPlot { get; set; }

    /// <summary>
    /// Plot rendering settings.
    /// </summary>
    public TikzPlotSettings Settings { get; set; } = new();

    private sealed record UppMarksAnnotation(
        string Name,
        Rational T,
        Rational D,
        Rational C,
        Rational FT,
        Rational FTd,
        Rational FT2d
    );

    private readonly Dictionary<string, UppMarksAnnotation> _uppMarks = new();

    private void EnsureNamePresent(string name)
    {
        if (!SequencesToPlot.Any(s => s.Name == name))
            throw new ArgumentException(
                $"Name '{name}' does not match any plotted sequence.", nameof(name));
    }

    /// <summary>
    /// Builds a TikZ plot from the given sequences.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="names">The names to use for the sequences.</param>
    /// <param name="settings">Optional settings for the plot.</param>
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
    /// Adds UPP (Ultimate Pseudo-Periodic) marks on the plot, annotating
    /// the period start (T), period duration (d), and period height (c)
    /// from a Curve's pseudo-periodic parameters.
    /// </summary>
    public void AddUppMarks(Curve curve,
        [System.Runtime.CompilerServices.CallerArgumentExpression("curve")]
        string name = "f")
    {
        if (curve.IsUltimatelyInfinite)
            throw new ArgumentException("Curve must be ultimately periodic (not UltimatelyInfinite).", nameof(curve));

        // EnsureNamePresent(name);
        _uppMarks[name] = new UppMarksAnnotation(
            Name: name,
            T: curve.PseudoPeriodStart,
            D: curve.FirstPseudoPeriodEnd - curve.PseudoPeriodStart,
            C: curve.PseudoPeriodHeight,
            FT: curve.ValueAt(curve.PseudoPeriodStart),
            FTd: curve.ValueAt(curve.FirstPseudoPeriodEnd),
            FT2d: curve.ValueAt(curve.SecondPseudoPeriodEnd)
        );
    }

    /// <summary>
    /// Adds UPP (Ultimate Pseudo-Periodic) marks on the plot from raw parameters.
    /// </summary>
    /// <param name="T">Pseudo-period start.</param>
    /// <param name="d">Pseudo-period length.</param>
    /// <param name="c">Pseudo-period height.</param>
    /// <param name="f_T">Value at the period start.</param>
    /// <param name="f_Td">Value at T + d.</param>
    /// <param name="f_T2d">Value at T + 2d.</param>
    /// <param name="name">The name to use in annotations.</param>
    public void AddUppMarks(Rational T, Rational d, Rational c,
        Rational f_T, Rational f_Td, Rational f_T2d, string name)
    {
        // EnsureNamePresent(name);
        _uppMarks[name] = new UppMarksAnnotation(
            Name: name,
            T: T,
            D: d,
            C: c,
            FT: f_T,
            FTd: f_Td,
            FT2d: f_T2d
        );
    }

    /// <summary>
    /// Removes any UPP marks associated with the given sequence name.
    /// </summary>
    public void RemoveUppMarks(string name) => _uppMarks.Remove(name);

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

        var axisLimits = PlotAxisLimitAlgorithms.GetSequenceAxisLimits(sequences, Settings);

        var xmarks = sequences
            .SelectMany(s => s
                .EnumerateBreakpoints()
                .Select(bp => bp.center.Time))
            .Where(x => x.IsFinite)
            .Where(axisLimits.XLimit.Contains)
            .OrderBy(x => x)
            .Distinct()
            .ToList();

        var ymarks = sequences
            .SelectMany(s => s
                .EnumerateBreakpoints()
                .GetBreakpointsBoundaryValues())
            .Where(y => y.IsFinite)
            .Where(axisLimits.YLimit.Contains)
            .OrderBy(y => y)
            .Distinct()
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLines(GetTikzHeader(axisLimits, xmarks, ymarks, Settings));

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

        foreach (var annotation in _uppMarks.Values)
            sb.AppendLines(GetUppMarksLines(annotation));

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
    /// <param name="axisLimits">The plot axis limits.</param>
    /// <param name="xmarks">The tick marks for the x axis.</param>
    /// <param name="ymarks">The tick marks for the y axis.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    private static IEnumerable<string> GetTikzHeader(
        PlotAxisLimits axisLimits,
        List<Rational>? xmarks = null,
        List<Rational>? ymarks = null,
        TikzPlotSettings? settings = null)
    {
        settings ??= new ();
        var displayLimits = GetDisplayLimits(axisLimits, settings);

        yield return $"\\begin{{tikzpicture}}";
        yield return $"{Tabs(1)}\\begin{{axis}}[";
        yield return $"{Tabs(2)}font = {settings.FontSize.ToLatex()},";
        if(!string.IsNullOrWhiteSpace(settings.Title))
            yield return $"{Tabs(2)}title = {{{settings.Title}}},";
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
        yield return $"{Tabs(2)}xlabel = {{{settings.XLabel}}},";
        yield return $"{Tabs(2)}ylabel = {{{settings.YLabel}}},";
        var xLabelAnchor = settings.GridTickLayout switch {
            GridTickLayout.SquareGridNoLabels => "north",
            _ => "north west"
        };
        yield return $"{Tabs(2)}x label style = {{at={{(axis description cs:1,0)}},anchor={xLabelAnchor}}},";
        yield return $"{Tabs(2)}y label style = {{at={{(axis description cs:0,1)}},rotate=-90,anchor=south}},";
        yield return $"{Tabs(2)}xmin = {ToInvariantDecimal(displayLimits.XLimit.Lower)},";
        yield return $"{Tabs(2)}ymin = {ToInvariantDecimal(displayLimits.YLimit.Lower)},";

        switch (settings.GridTickLayout)
        {
            case GridTickLayout.Auto:
            {
                yield return $"{Tabs(2)}xmax = {ToInvariantDecimal(displayLimits.XLimit.Upper)},";
                yield return $"{Tabs(2)}ymax = {ToInvariantDecimal(displayLimits.YLimit.Upper)},";
                yield return $"{Tabs(2)}xticklabels = \\empty,";
                yield return $"{Tabs(2)}yticklabels = \\empty,";

                if (xmarks is { Count: > 0 })
                {   
                    var sb = new StringBuilder();
                    sb.Append($"{Tabs(2)}extra x ticks = {{ ");
                    foreach (var xmark in xmarks)
                    {
                        sb.Append(ToInvariantDecimal(xmark));
                        sb.Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(" },");
                    yield return sb.ToString();
                }

                if (ymarks is { Count: > 0 })
                {
                    var sb = new StringBuilder();
                    sb.Append($"{Tabs(2)}extra y ticks = {{ ");
                    foreach (var ymark in ymarks)
                    {
                        sb.Append(ToInvariantDecimal(ymark));
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
                var xfloor = (int) Math.Floor((decimal) displayLimits.XLimit.Lower);
                var yfloor = (int) Math.Floor((decimal) displayLimits.YLimit.Lower);
                var xceil = (int) Math.Ceiling((decimal) displayLimits.XLimit.Upper);
                var yceil = (int) Math.Ceiling((decimal) displayLimits.YLimit.Upper);
                yield return FormattableString.Invariant($"{Tabs(2)}xmax = {xceil},");
                yield return FormattableString.Invariant($"{Tabs(2)}ymax = {yceil},");

                // xtick
                {
                    var sb = new StringBuilder();
                    sb.Append($"{Tabs(2)}xtick = {{ ");
                    for (int i = xfloor; i <= xceil; i++)
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
                    for (int i = yfloor; i <= yceil; i++)
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

    private static PlotAxisLimits GetDisplayLimits(PlotAxisLimits axisLimits, TikzPlotSettings settings)
    {
        var xLimit = settings.XLimit.HasValue || settings.RelativeXAxisMargin != 0
            ? axisLimits.XLimit
            : GetLegacyTikzLimit(axisLimits.XLimit);
        var yLimit = settings.YLimit.HasValue || settings.RelativeYAxisMargin != 0
            ? axisLimits.YLimit
            : GetLegacyTikzLimit(axisLimits.YLimit);

        return new PlotAxisLimits(xLimit, yLimit);
    }

    private static Interval GetLegacyTikzLimit(Interval limit)
    {
        var upper = limit.Upper + 1;
        var lower = Rational.Min(0, upper);

        return new Interval(
            lower,
            upper,
            isLowerIncluded: true,
            isUpperIncluded: true);
    }

    private static string ToInvariantDecimal(Rational value)
    {
        return ((decimal) value).ToString(CultureInfo.InvariantCulture);
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
    /// <param name="f">The first operand.</param>
    /// <param name="name">The name to use.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IEnumerable<string> GetUppMarks(Curve f, string name)
    {
        var tf = (decimal)f.PseudoPeriodStart;
        var ftf = (decimal)f.ValueAt(f.PseudoPeriodStart);
        var tfdf = (decimal)f.FirstPseudoPeriodEnd;
        var ftfdf = (decimal)f.ValueAt(f.FirstPseudoPeriodEnd);
        var tf2df = (decimal)f.SecondPseudoPeriodEnd;
        var ftf2df = (decimal)f.ValueAt(f.SecondPseudoPeriodEnd);
        var c = (decimal)f.PseudoPeriodHeight;
        return GetUppMarksLines(name, tf, tfdf, tf2df, ftf, ftfdf, ftf2df, c);
    }

    private static IEnumerable<string> GetUppMarksLines(UppMarksAnnotation a)
    {
        var tf = (decimal)a.T;
        var tfdf = (decimal)(a.T + a.D);
        var tf2df = (decimal)(a.T + 2 * a.D);
        var ftf = (decimal)a.FT;
        var ftfdf = (decimal)a.FTd;
        var ftf2df = (decimal)a.FT2d;
        var c = (decimal)a.C;
        return GetUppMarksLines(a.Name, tf, tfdf, tf2df, ftf, ftfdf, ftf2df, c);
    }

    private static IEnumerable<string> GetUppMarksLines(
        string name, decimal tf, decimal tfdf, decimal tf2df,
        decimal ftf, decimal ftfdf, decimal ftf2df, decimal c)
    {
        return _getUppMarks().Select(FormattableString.Invariant);

        IEnumerable<FormattableString> _getUppMarks()
        {
            var marksColor = "black!60";
            var marksStyle = "thick, densely dashed";
            var arrowStyle = "thick, <->";

            if(ftf > 0)
                yield return $"{Tabs(2)}\\addplot [ color = {marksColor}, {marksStyle} ] coordinates {{ ({tf}, 0) ({tf}, {ftf}) }};";
            yield return $"{Tabs(2)}\\node [ anchor = north ] at (axis cs:{tf}, 0) {{$T_{{{name}}}$}};";
            yield return $"";

            yield return
                $"{Tabs(2)}\\addplot [ color = {marksColor}, {marksStyle} ] coordinates {{ ({tfdf}, {ftfdf}) ({tfdf}, {(ftfdf + ftf2df) / 2}) }};";
            yield return
                $"{Tabs(2)}\\addplot [ color = {marksColor}, {marksStyle} ] coordinates {{ ({tf}, {ftf}) ({tf}, {(ftfdf + ftf2df) / 2}) }};";
            yield return
                $"{Tabs(2)}\\addplot [ color = {marksColor}, {arrowStyle} ] coordinates {{ ({tf}, {(ftfdf + ftf2df) / 2}) ({tfdf}, {(ftfdf + ftf2df) / 2}) }};";
            yield return $"{Tabs(2)}\\node [ anchor = south ] at (axis cs:{(tf + tfdf) / 2}, {(ftfdf + ftf2df) / 2}) {{$d_{{{name}}}$}};";
            yield return $"";

            if (c > 0)
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
    /// <param name="sequences">The sequences to process.</param>
    /// <param name="names">The names to use.</param>
    /// <param name="colors">The colors to use.</param>
    /// <param name="lineStyles">The line styles to use.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <param name="includeLegend">Whether to include the legend.</param>
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

/// <summary>
/// Sequence and style information used by a TikZ plot.
/// </summary>
public record SequenceToPlot
{
    /// <summary>
    /// Sequence to plot.
    /// </summary>
    public required Sequence Sequence { get; init; }

    /// <summary>
    /// Color used to render the sequence.
    /// </summary>
    public required string Color { get; init; }

    /// <summary>
    /// Name used in the plot legend.
    /// </summary>
    public string Name { get; init; } = string.Empty;
}

static class ToTikzExtensions
{
    /// <summary>
    /// Computes the lines that plot the sequence.
    /// </summary>
    /// <param name="sequence">The sequence to process.</param>
    /// <param name="color">The color to use.</param>
    /// <param name="lineStyle">The line style to use.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result.</returns>
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
