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

    /// <summary>
    /// True if the sequences are cuts of curves that go on past them.
    /// </summary>
    /// <remarks>
    /// The areas marking infinite values then reach the edge of the plot, rather than stopping at the cut as if the value ended there.
    /// It is false for sequences plotted directly, whose values past their end are not known.
    /// </remarks>
    /// <remarks>
    /// Ignored when <see cref="Window"/> is set, which says the same thing and more.
    /// </remarks>
    public bool SequencesContinuePastEnd { get; set; }

    /// <summary>
    /// The window the sequences were sampled over. When unset, it is worked out from the sequences as given.
    /// </summary>
    /// <remarks>
    /// Sampling a curve reaches past the range the plot is framed at, so the extent of the sequences is not the range
    /// the reader asked for: the frame and the tick marks come from <see cref="PlotXWindow.Data"/> and not from the samples.
    /// </remarks>
    public PlotXWindow? Window { get; set; }

    private PlotXWindow WindowFor(IReadOnlyCollection<Sequence> sequences)
        => Window ?? PlotXWindow.ForSequences(sequences, Settings, SequencesContinuePastEnd);

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
    /// \usetikzlibrary{patterns}
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
        var lineStyles = GetLineStyles(Settings, sequences.Count, DefaultColorList.Count);
        var fillPatterns = Settings.FillPatterns ?? PlotStyleCycles.DefaultFillPatterns;

        var window = WindowFor(sequences);
        var axisLimits = PlotAxisLimitAlgorithms.SuggestFramingLimits(
            sequences, Settings, window);

        // the ticks index the data the reader asked for; a sample taken only to fill the margin is not one of its breakpoints
        var xmarks = sequences
            .SelectMany(s => s
                .EnumerateBreakpoints()
                .Select(bp => bp.center.Time))
            .Where(x => x.IsFinite)
            .Where(window.Data.Contains)
            .OrderBy(x => x)
            .Distinct()
            .ToList();

        var ymarks = sequences
            .SelectMany(s => s
                .EnumerateBreakpoints()
                .GetBreakpointsBoundaryValues())
            .Where(y => y.IsFinite)
            .Where(axisLimits.YFramingLimit.Contains)
            .OrderBy(y => y)
            .Distinct()
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLines(GetTikzHeader(axisLimits, xmarks, ymarks, Settings));

        // the areas go first, so that the curves are drawn over them
        if (Settings.InfinityStrategy == InfinityStrategy.Areas)
            sb.AppendLines(GetInfinityAreaLines(
                sequences, colors, fillPatterns, axisLimits, window.DataContinuesPastSamples));

        var includeLegend = Settings.LegendStrategy switch
        {
            // By default, the legend is omitted if a single curve or sequence is being plotted,
            // unless the name was specified manually
            LegendStrategy.Auto => sequences.Count > 1 || !string.IsNullOrWhiteSpace(SequencesToPlot.Single().Name),
            LegendStrategy.ForceEnable => true,
            LegendStrategy.ForceDisable => false,
            _ => true
        };
        
        var continuations = sequences
            .Select(s => s.GetTrailingContinuation(window.Frame, window.DataContinuesPastSamples))
            .ToList();

        sb.AppendLines(GetTikzContent(
            sequences, names, colors, lineStyles, continuations, Settings, includeLegend,
            window.SamplesReachTheFrame));

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
    /// Get the line styles to use, one per sequence.
    /// </summary>
    /// <param name="settings">The settings of the plot.</param>
    /// <param name="n">The number of sequences.</param>
    /// <param name="colorCount">The number of colors being cycled.</param>
    public static List<string> GetLineStyles(TikzPlotSettings settings, int n, int colorCount)
    {
        var result = new List<string>();
        if (settings.RawLineStyles is { Count: > 0 } raw)
        {
            for (var i = 0; i < n; i++)
                result.Add(raw[i % raw.Count]);
            return result;
        }

        if (!settings.UseLineStyles)
        {
            for (var i = 0; i < n; i++)
                result.Add("solid");
            return result;
        }

        var styles = settings.LineStyles ?? PlotStyleCycles.DefaultLineStyles;
        for (var i = 0; i < n; i++)
            result.Add(ToTikzLineStyle(PlotStyleCycles.Pick(i, styles)));
        return result;
    }

    /// <summary>
    /// Maps a <see cref="PlotLineStyle"/> to the matching TikZ option.
    /// </summary>
    public static string ToTikzLineStyle(PlotLineStyle style)
        => style switch
        {
            PlotLineStyle.Solid => "solid",
            PlotLineStyle.Dashed => "dashed",
            PlotLineStyle.Dotted => "dotted",
            PlotLineStyle.DashDotted => "dash dot",
            PlotLineStyle.DenselyDashed => "densely dashed",
            PlotLineStyle.DenselyDotted => "densely dotted",
            _ => "solid"
        };

    /// <summary>
    /// Maps a <see cref="PlotFillPattern"/> to the matching TikZ pattern.
    /// </summary>
    /// <remarks>
    /// These require the <c>patterns</c> TikZ library in the preamble.
    /// </remarks>
    public static string ToTikzFillPattern(PlotFillPattern pattern)
        => pattern switch
        {
            PlotFillPattern.Dots => "dots",
            PlotFillPattern.DenseDots => "crosshatch dots",
            PlotFillPattern.DiagonalLines => "north east lines",
            PlotFillPattern.ReverseDiagonalLines => "north west lines",
            PlotFillPattern.Grid => "grid",
            PlotFillPattern.Crosshatch => "crosshatch",
            _ => "dots"
        };

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
        var displayLimits = axisLimits;

        yield return $"\\begin{{tikzpicture}}";
        yield return $"{Tabs(1)}\\begin{{axis}}[";
        yield return $"{Tabs(2)}font = {settings.FontSize.ToLatex()},";
        if(!string.IsNullOrWhiteSpace(settings.Title))
            yield return $"{Tabs(2)}title = {{{settings.Title}}},";
        yield return settings.ClipStrategy == ClipStrategy.ToLimits
            ? $"{Tabs(2)}clip = true,"
            : $"{Tabs(2)}clip = false,";

        switch (settings.GridTickLayout)
        {
            case GridTickLayout.Breakpoints:
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
        if (settings.SameScaleAxes)
            yield return $"{Tabs(2)}axis equal image,";
        yield return $"{Tabs(2)}xlabel = {{{settings.XLabel}}},";
        yield return $"{Tabs(2)}ylabel = {{{settings.YLabel}}},";
        var xLabelAnchor = settings.GridTickLayout switch {
            GridTickLayout.SquareGridNoLabels => "north",
            _ => "north west"
        };
        yield return $"{Tabs(2)}x label style = {{at={{(axis description cs:1,0)}},anchor={xLabelAnchor}}},";
        yield return $"{Tabs(2)}y label style = {{at={{(axis description cs:0,1)}},rotate=-90,anchor=south}},";
        yield return $"{Tabs(2)}xmin = {ToInvariantDecimal(displayLimits.XFramingLimit.Lower)},";
        yield return $"{Tabs(2)}ymin = {ToInvariantDecimal(displayLimits.YFramingLimit.Lower)},";

        switch (settings.GridTickLayout)
        {
            case GridTickLayout.RoundValues:
            {
                // the ticks are left to pgfplots, which places them at round values
                yield return $"{Tabs(2)}xmax = {ToInvariantDecimal(displayLimits.XFramingLimit.Upper)},";
                yield return $"{Tabs(2)}ymax = {ToInvariantDecimal(displayLimits.YFramingLimit.Upper)},";
                break;
            }

            case GridTickLayout.Breakpoints:
            {
                yield return $"{Tabs(2)}xmax = {ToInvariantDecimal(displayLimits.XFramingLimit.Upper)},";
                yield return $"{Tabs(2)}ymax = {ToInvariantDecimal(displayLimits.YFramingLimit.Upper)},";
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
                var xfloor = (int) Math.Floor((decimal) displayLimits.XFramingLimit.Lower);
                var yfloor = (int) Math.Floor((decimal) displayLimits.YFramingLimit.Lower);
                var xceil = (int) Math.Ceiling((decimal) displayLimits.XFramingLimit.Upper);
                var yceil = (int) Math.Ceiling((decimal) displayLimits.YFramingLimit.Upper);
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

        if (settings.LegendPlacement == LegendPlacement.Outside)
        {
            // placed against the outside of the axis box, where it cannot cover the curves
            var (at, anchor) = settings.LegendPosition.ToOutsideLatex();
            yield return $"{Tabs(2)}legend style = {{ at = {{({at})}}, anchor = {anchor} }}";
        }
        else
        {
            yield return $"{Tabs(2)}legend pos = {settings.LegendPosition.ToLatex()}";
        }
        yield return $"{Tabs(1)}]";
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
    /// Computes the lines that draw the areas marking infinite values.
    /// </summary>
    /// <param name="sequences">The sequences being plotted.</param>
    /// <param name="colors">The colors in use, one per sequence.</param>
    /// <param name="fillPatterns">The fill patterns being cycled.</param>
    /// <param name="axisLimits">The limits of the plot, which reserved the room for the areas.</param>
    /// <param name="continuesPastEnd">True if the sequences are cuts of curves that go on past them.</param>
    /// <remarks>
    /// These are drawn with plain TikZ rather than <c>\addplot</c>, so that they take no part in the legend or the color cycle.
    /// No border is drawn: at a curve's weight it would read as a segment of the curve itself.
    /// The label is staggered per sequence, so that overlapping areas do not write over each other.
    /// </remarks>
    private static IEnumerable<string> GetInfinityAreaLines(
        IReadOnlyList<Sequence> sequences,
        IReadOnlyList<string> colors,
        IReadOnlyList<PlotFillPattern> fillPatterns,
        PlotAxisLimits axisLimits,
        bool continuesPastEnd)
    {
        var withInfinities = sequences
            .Select((sequence, index) => (sequence, index))
            .Where(p => p.sequence.HasPlusInfinity || p.sequence.HasMinusInfinity)
            .ToList();
        if (withInfinities.Count == 0)
            yield break;

        foreach (var ((sequence, index), position) in withInfinities.WithIndex())
        {
            var color = colors[index % colors.Count];
            var pattern = ToTikzFillPattern(
                PlotStyleCycles.Pick(index, fillPatterns));

            foreach (var region in sequence.EnumerateVisibleInfiniteRegions(
                         axisLimits.XFramingLimit, continuesPastEnd))
            {
                var band = region.IsPlusInfinite
                    ? axisLimits.PlusInfinityBand
                    : axisLimits.MinusInfinityBand;
                if (region.EndTime <= region.StartTime || band.Upper <= band.Lower)
                    continue;

                var x0 = ToInvariantDecimal(region.StartTime);
                var x1 = ToInvariantDecimal(region.EndTime);
                var y0 = ToInvariantDecimal(band.Lower);
                var y1 = ToInvariantDecimal(band.Upper);

                yield return
                    $"{Tabs(2)}\\fill [ pattern = {pattern}, pattern color = {color}, opacity = 0.6 ] " +
                    $"(axis cs:{x0},{y0}) rectangle (axis cs:{x1},{y1});";

                var labelX = ToInvariantDecimal((region.StartTime + region.EndTime) / 2);
                var labelY = ToInvariantDecimal(
                    band.Lower + (band.Upper - band.Lower) * (position + 1) / (withInfinities.Count + 1));
                var sign = region.IsPlusInfinite ? "+" : "-";
                yield return
                    $"{Tabs(2)}\\node [ {color}, font = \\large, fill = white, fill opacity = 0.7, text opacity = 1, inner sep = 1pt ] at (axis cs:{labelX},{labelY}) {{$ {sign}\\infty $}};";
            }

            yield return "";
        }
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
    /// <param name="continuations">For each sequence, how the curve is drawn past the plot end.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <param name="includeLegend">Whether to include the legend.</param>
    /// <param name="samplesReachTheFrame">True when the samples run to the frame, so their last point is not an end of the data.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    private static IEnumerable<string> GetTikzContent(
        IReadOnlyList<Sequence> sequences, 
        IReadOnlyList<string> names, 
        IReadOnlyList<string> colors,
        IReadOnlyList<string> lineStyles,
        IReadOnlyList<TrailingContinuation?> continuations,
        TikzPlotSettings settings,
        bool includeLegend,
        bool samplesReachTheFrame = false
    )
    {
        if (sequences.Count != names.Count || sequences.Count != colors.Count)
            throw new ArgumentException("The arguments must be of the same length");
        if (sequences.Any(s => s.FirstFiniteTime.IsPlusInfinite))
            throw new ArgumentException("Cannot plot infinite-only sequences");

        var plots = sequences
            .Select((s, i) => ToTikzExtensions
                .ToTikzLines(s, colors[i], lineStyles[i], settings, continuations[i], samplesReachTheFrame)
                .ToList())
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
    /// <param name="continuation">How the curve is drawn past the plot end.</param>
    /// <param name="samplesReachTheFrame">
    /// True when the sequence was sampled all the way to the frame, so its last point is where the plot stops
    /// rather than where the data does, and carries none of the marks that say a function ended.
    /// </param>
    /// <returns>The result.</returns>
    public static IEnumerable<string> ToTikzLines(
        this Sequence sequence, 
        string color, 
        string? lineStyle = null,
        TikzPlotSettings? settings = null,
        TrailingContinuation? continuation = null,
        bool samplesReachTheFrame = false
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
                    var carriesOn = false;
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

                    // the last run reaches where the samples stop, which is not where the function ends
                    if (!keepLooping && breakpoints.Count > 0 && breakpoints[^1].time == sequence.DefinedUntil)
                    {
                        // a cut curve is carried on to the edge of the plot;
                        // samples that already reach the frame have nothing to carry there
                        if (continuation is { } trailing && breakpoints.Count > 1)
                        {
                            breakpoints.Add((trailing.Time, trailing.Value));
                            carriesOn = true;
                        }
                        else if (samplesReachTheFrame)
                            carriesOn = true;
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
                            var rightBracket = isEndClosed || carriesOn ? "" : "(";
                            var shortenLeft = isStartClosed ? "" : "shorten < = 1pt, ";
                            var shortenRight = isEndClosed || carriesOn ? "" : "shorten > = 1pt, ";

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

                        if (isEndClosed && !carriesOn)
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
