using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.TikzPlot;

/// <summary>
/// This class provides methods to generate plots with TikZ. 
/// </summary>
public static class ToTikzPlotExtension
{
    /// <summary>
    /// Default colors.
    /// </summary>
    public static List<string> DefaultColorList = new ()
    {
        "blue!60!black",
        "green!60!black",
        "red!60!black"
    };
    
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
    /// Computes the x-axis right boundary.
    /// </summary>
    /// <param name="curves">The curves to be plotted.</param>
    /// <param name="strategy">Computation strategy, defaults to <see cref="PlotEndStrategy.TwoPeriodsEach"/>.</param>
    /// <param name="upTo">The preferred right boundary. If it is 0, it will be overridden.</param>
    public static Rational ComputePlotEnd(
        IReadOnlyList<Curve> curves,
        PlotEndStrategy strategy = PlotEndStrategy.TwoPeriodsEach,
        Rational? upTo = null)
    {
        if (upTo is not null && upTo > 0)
        {
            return (Rational) upTo;
        }
        else
        {
            switch (strategy)
            {
                case PlotEndStrategy.OnePeriodEach:
                {
                    if (curves.All(c => c.IsUltimatelyAffine || c.IsUltimatelyInfinite ))
                    {
                        if (curves.All(c => c.PseudoPeriodStart == 0))
                            return 1;
                        else
                            return curves.Max(c => c.PseudoPeriodStart) * 2;
                    }
                    else
                    {
                        return curves
                            .Where(c => !(c.IsUltimatelyAffine || c.IsUltimatelyInfinite))
                            .Max(c => c.FirstPseudoPeriodEnd);
                    }
                }

                case PlotEndStrategy.TwoPeriodsEach:
                {
                    if (curves.All(c => c.IsUltimatelyAffine))
                    {
                        if (curves.All(c => c.PseudoPeriodStart == 0))
                            return 1;
                        else
                            return curves.Max(c => c.PseudoPeriodStart) * 2;
                    }
                    else
                    {
                        return curves
                            .Where(c => !(c.IsUltimatelyAffine || c.IsUltimatelyInfinite))
                            .Max(c => c.SecondPseudoPeriodEnd);
                    }
                }
                
                default:
                    throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// Plots the curves using TikZ.
    /// </summary>
    /// <param name="curves"></param>
    /// <param name="names"></param>
    /// <param name="colors"></param>
    /// <param name="lineStyles"></param>
    /// <param name="settings"></param>
    /// <param name="plotEndStrategy"></param>
    /// <param name="upTo"></param>
    /// <returns>
    /// A string with the TikZ code of the plot.
    /// Can be written to file and compiled with LaTeX.
    /// </returns>
    public static string ToTikzPlot(
        this IReadOnlyList<Curve> curves, 
        IReadOnlyList<string>? names = null, 
        IReadOnlyList<string>? colors = null, 
        IReadOnlyList<string>? lineStyles = null,
        TikzLayoutSettings? settings = null,
        PlotEndStrategy plotEndStrategy = PlotEndStrategy.TwoPeriodsEach,
        Rational? upTo = null)
    {
        settings ??= new TikzLayoutSettings();
        
        var t = ComputePlotEnd(curves, plotEndStrategy, upTo);
        
        var sequences = curves
            .Select(c => c.Cut(0, t, isEndIncluded: true))
            .ToList();

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
        sb.AppendLines(GetTikzHeader(xmax, ymax, xmarks, ymarks, settings));

        var includeLegend = settings.LegendStrategy switch
        {
            // By default, the legend is omitted if a single curve or sequence is being plotted,
            // unless the name was specified manually
            LegendStrategy.Auto => curves.Count > 1 || names != null,
            LegendStrategy.ForceEnable => true,
            LegendStrategy.ForceDisable => false,
            _ => true
        };
        names ??= GetDefaultNames(curves.Count);
        colors ??= GetDefaultColors(curves.Count);
        lineStyles ??= GetDefaultLineStyles(curves.Count);
        
        sb.AppendLines(GetTikzContent(sequences, names, colors, lineStyles, settings, includeLegend));

        if (curves.Count == 1 && curves.Single() is { IsUltimatelyInfinite: false } f)
        {
            sb.AppendLines(GetUppMarks(f, names[0]));
        }
        
        sb.AppendLines(GetTikzFooter());

        return sb.ToString();
    }

    /// <summary>
    /// Plots the curve using TikZ.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="name"></param>
    /// <param name="color"></param>
    /// <param name="lineStyle"></param>
    /// <param name="settings"></param>
    /// <param name="plotEndStrategy"></param>
    /// <param name="upTo"></param>
    /// <returns>
    /// A string with the TikZ code of the plot.
    /// Can be written to file and compiled with LaTeX.
    /// </returns>
    public static string ToTikzPlot(
        this Curve curve,
        string? name = null,
        string? color = null,
        string? lineStyle = null,
        TikzLayoutSettings? settings = null,
        PlotEndStrategy plotEndStrategy = PlotEndStrategy.TwoPeriodsEach,
        Rational? upTo = null
    )
    {
        var names = name != null ? new List<string>{name} : null;
        var colors = color != null ? new List<string>{color} : null;
        var lineStyles = lineStyle != null ? new List<string>{lineStyle} : null;
        return ToTikzPlot(new[] { curve }, names, colors, lineStyles, settings, plotEndStrategy, upTo);
    }
    
    /// <summary>
    /// Plots the curves using TikZ, using default settings.
    /// </summary>
    /// <param name="curves"></param>
    /// <returns>
    /// A string with the TikZ code of the plot.
    /// Can be written to file and compiled with LaTeX.
    /// </returns>
    public static string ToTikzPlot(params Curve[] curves)
    {
        var names = GetDefaultNames(curves.Length);
        var colors = GetDefaultColors(curves.Length);
        var lineStyles = GetDefaultLineStyles(curves.Length);
        return ToTikzPlot(curves, names, colors, lineStyles);
    }
    
    /// <summary>
    /// Handy method that concatenates n tabs.  
    /// </summary>
    private static string tabs(int n)
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
        TikzLayoutSettings? settings = null)
    {
        settings ??= new TikzLayoutSettings();

        yield return $"\\begin{{tikzpicture}}";
        yield return $"{tabs(1)}\\begin{{axis}}[";
        yield return $"{tabs(2)}font = {settings.FontSize.ToLatex()},";
        yield return $"{tabs(2)}clip = false,";

        switch (settings.GridTickLayout)
        {
            case GridTickLayout.Auto:
                yield return $"{tabs(2)}grid = both,";
                yield return $"{tabs(2)}minor tick num = 1,";
                break;
            
            case GridTickLayout.SquareGrid:
            case GridTickLayout.SquareGridNoLabels:
                yield return $"{tabs(2)}grid = major,";
                break;
            
            default:
                yield return $"{tabs(2)}grid = both,";
                break;
        }

        yield return $"{tabs(2)}grid style = {{draw=gray!30}},";
        yield return $"{tabs(2)}axis lines = left,";
        yield return $"{tabs(2)}axis equal image,";
        yield return $"{tabs(2)}xlabel = time,";
        yield return $"{tabs(2)}ylabel = data,";
        var xLabelAnchor = settings.GridTickLayout switch {
            GridTickLayout.SquareGridNoLabels => "north",
            _ => "north west"
        };
        yield return $"{tabs(2)}x label style = {{at={{(axis description cs:1,0)}},anchor={xLabelAnchor}}},";
        yield return $"{tabs(2)}y label style = {{at={{(axis description cs:0,1)}},rotate=-90,anchor=south}},";
        yield return $"{tabs(2)}xmin = 0,";
        yield return $"{tabs(2)}ymin = 0,";

        switch (settings.GridTickLayout)
        {
            case GridTickLayout.Auto:
            {
                yield return FormattableString.Invariant($"{tabs(2)}xmax = {(decimal) xmax + 1},");
                yield return FormattableString.Invariant($"{tabs(2)}ymax = {(decimal) ymax + 1},");
                yield return $"{tabs(2)}xticklabels = \\empty,";
                yield return $"{tabs(2)}yticklabels = \\empty,";

                if (xmarks != null)
                {   
                    var sb = new StringBuilder();
                    sb.Append($"{tabs(2)}extra x ticks = {{ ");
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
                    sb.Append($"{tabs(2)}extra y ticks = {{ ");
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
                yield return FormattableString.Invariant($"{tabs(2)}xmax = {xceil},");
                yield return FormattableString.Invariant($"{tabs(2)}ymax = {yceil},");

                // xtick
                {
                    var sb = new StringBuilder();
                    sb.Append($"{tabs(2)}xtick = {{ ");
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
                    sb.Append($"{tabs(2)}ytick = {{ ");
                    for (int i = 1; i <= yceil; i++)
                    {
                        sb.Append(i.ToString(CultureInfo.InvariantCulture));
                        sb.Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(" },");
                    yield return sb.ToString();
                }
            
                yield return $"{tabs(2)}minor xtick = {{}},";
                yield return $"{tabs(2)}minor ytick = {{}},";
                if (settings.GridTickLayout == GridTickLayout.SquareGridNoLabels)
                {
                    yield return $"{tabs(2)}xticklabels = \\empty,";
                    yield return $"{tabs(2)}yticklabels = \\empty,";
                }
                break;
            }
        }
        
        yield return $"{tabs(2)}legend pos = {settings.LegendPosition.ToLatex()}";
        yield return $"{tabs(1)}]";
    }

    /// <summary>
    /// Computes the footer for the plot.
    /// </summary>
    private static IEnumerable<string> GetTikzFooter()
    {
        yield return $"{tabs(1)}\\end{{axis}}";
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
                yield return $"{tabs(2)}\\addplot [ color = {marksColor}, {marksStyle} ] coordinates {{ ({tf}, 0) ({tf}, {ftf}) }};";
            yield return $"{tabs(2)}\\node [ anchor = north ] at (axis cs:{tf}, 0) {{$T_{{{name}}}$}};";
            yield return $"";

            var tfdf = (decimal)f.FirstPseudoPeriodEnd;
            var ftfdf = (decimal)f.ValueAt(f.FirstPseudoPeriodEnd);
            var ftf2df = (decimal)f.ValueAt(f.SecondPseudoPeriodEnd);

            yield return
                $"{tabs(2)}\\addplot [ color = {marksColor}, {marksStyle} ] coordinates {{ ({tfdf}, {ftfdf}) ({tfdf}, {(ftfdf + ftf2df) / 2}) }};";
            yield return
                $"{tabs(2)}\\addplot [ color = {marksColor}, {marksStyle} ] coordinates {{ ({tf}, {ftf}) ({tf}, {(ftfdf + ftf2df) / 2}) }};";
            yield return
                $"{tabs(2)}\\addplot [ color = {marksColor}, {arrowStyle} ] coordinates {{ ({tf}, {(ftfdf + ftf2df) / 2}) ({tfdf}, {(ftfdf + ftf2df) / 2}) }};";
            yield return $"{tabs(2)}\\node [ anchor = south ] at (axis cs:{(tf + tfdf) / 2}, {(ftfdf + ftf2df) / 2}) {{$d_{{{name}}}$}};";
            yield return $"";

            var tf2df = (decimal)f.SecondPseudoPeriodEnd;

            if (f.PseudoPeriodHeight > 0)
            {
                yield return
                    $"{tabs(2)}\\addplot [ color = {marksColor}, {marksStyle} ] coordinates {{ ({tf}, {ftf}) ({(tfdf + tf2df) / 2}, {ftf}) }};";
                yield return
                    $"{tabs(2)}\\addplot [ color = {marksColor}, {marksStyle} ] coordinates {{ ({tfdf}, {ftfdf}) ({(tfdf + tf2df) / 2}, {ftfdf}) }};";
                yield return
                    $"{tabs(2)}\\addplot [ color = {marksColor}, {arrowStyle} ] coordinates {{ ({(tfdf + tf2df) / 2}, {ftf}) ({(tfdf + tf2df) / 2}, {ftfdf}) }};";
                yield return
                    $"{tabs(2)}\\node [ anchor = west ] at (axis cs:{(tfdf + tf2df) / 2}, {(ftf + ftfdf) / 2}) {{$c_{{{name}}}$}};";
                yield return $"";
            }
            else
            {
                yield return
                    $"{tabs(2)}\\node [ anchor = south west ] at (axis cs:{(tfdf + tf2df) / 2}, {(ftf + ftfdf) / 2}) {{$c_{{{name}}}$}};";
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
        this IReadOnlyList<Sequence> sequences, 
        IReadOnlyList<string> names, 
        IReadOnlyList<string> colors,
        IReadOnlyList<string> lineStyles,
        TikzLayoutSettings settings,
        bool includeLegend
    )
    {
        if (sequences.Count != names.Count || sequences.Count != colors.Count)
            throw new InvalidEnumArgumentException("The arguments must be of the same length");
        if (sequences.Any(s => s.FirstFiniteTime.IsPlusInfinite))
            throw new InvalidEnumArgumentException("Cannot plot infinite-only sequences");

        var plots = sequences
            .Select((s, i) => s.ToTikzLines(colors[i], lineStyles[i], settings).ToList())
            .ToList();
        
        if (!includeLegend)
        {
            // we just plot each sequence in order
            foreach (var (plot, i) in plots.WithIndex())
            {
                yield return $"{tabs(2)}% {names[i]}";
                foreach (var line in plot)
                    yield return $"{tabs(2)}{line}";
                yield return "";
            }
        }
        else
        {
            // needs to be more complex to correctly plot the legend,
            // one element for each sequence must be selected and plotted out of order. 
            
            yield return $"{tabs(2)}% lines out of order, for the legend";
            var legendLines = plots.Select((plot, i) =>
                {
                    var firstSegmentLine = plot.FirstOrDefault(l => !l.Contains("only marks"));
                    var legendLine = firstSegmentLine ?? plot.First();
                    return legendLine;
                })
                .ToList();

            foreach (var legendLine in legendLines)
                yield return $"{tabs(2)}{legendLine}";

            foreach (var name in names)
                yield return $"{tabs(2)}\\addlegendentry{{$ {name} $}};";
            yield return "";


            foreach (var (plot, i) in plots.WithIndex())
            {
                yield return $"{tabs(2)}% {names[i]}";
                foreach (var line in plot)
                {
                    if (line == legendLines[i])
                        continue;
                    else
                        yield return $"{tabs(2)}{line}";
                }

                yield return "";
            }
        }
    }

    /// <summary>
    /// Computes the lines that plot the sequence.
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="color"></param>
    /// <param name="lineStyle"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    private static IEnumerable<string> ToTikzLines(
        this Sequence sequence, 
        string color, 
        string? lineStyle = null,
        TikzLayoutSettings? settings = null
    )
    {
        settings ??= new TikzLayoutSettings();
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
    private static string ToTikzLine(this Element element, string color, string? lineStyle = null)
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

// todo: extend with strategies based on intersections

/// <summary>
/// Different strategies to compute the x-axis right boundary.
/// </summary>
public enum PlotEndStrategy
{
    /// <summary>
    /// The boundary is chosen such that, for each function, at least a full period is plotted.
    /// In math, $\bigvee\{ T_i + d_i \}$. 
    /// </summary>
    OnePeriodEach,
    
    /// <summary>
    /// The boundary is chosen such that, for each function, at least two full periods are plotted.
    /// In math, $\bigvee\{ T_i + 2 \cdot d_i \}$. 
    /// </summary>
    TwoPeriodsEach
}