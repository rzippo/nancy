using System.Runtime.CompilerServices;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using XPlot.Plotly;

namespace Unipi.Nancy.Interactive;

// ReSharper disable MemberCanBePrivate.Global

public static partial class Plots
{
    #region Curves

    /// <summary>
    /// Plots a set of curves.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <param name="names">The names of the curves.</param>
    /// <param name="upTo">
    /// The x-axis right edge of the plot.
    /// If null, it is set by default as $max_{i}(T_i + 2 * d_i)$.
    /// </param>
    /// <returns>A <see cref="PlotlyChart"/> object.</returns>
    public static PlotlyChart GetPlot(
        this IReadOnlyCollection<Curve> curves,
        IEnumerable<string> names,
        Rational? upTo = null
    )
    {
        Rational t;
        if (upTo is not null)
            t = (Rational)upTo;
        else
            t = curves.Max(c => c.SecondPseudoPeriodEnd);
        t = t == 0 ? 10 : t;

        var cuts = curves
            .Select(c => c.Cut(0, t, isEndIncluded: true))
            .ToList();

        return GetPlot(cuts, names);
    }

    /// <summary>
    /// Plots a curve.
    /// </summary>
    /// <param name="curve">The curve to plot.</param>
    /// <param name="name">
    /// The name of the curve.
    /// By default, it captures the expression used for <paramref name="curve"/>.
    /// </param>
    /// <param name="upTo">
    /// The x-axis right edge of the plot.
    /// If null, it is set by default as $T + 2 * d$.
    /// </param>
    /// <returns>A <see cref="PlotlyChart"/> object.</returns>
    public static PlotlyChart GetPlot(
        this Curve curve,
        [CallerArgumentExpression("curve")] string name = "f",
        Rational? upTo = null
    )
    {
        return GetPlot([curve], [name], upTo);
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <param name="upTo">
    /// The x-axis right edge of the plot.
    /// If null, it is set by default as $max_{i}(T_i + 2 * d_i)$.
    /// </param>
    /// <returns>A <see cref="PlotlyChart"/> object.</returns>
    public static PlotlyChart GetPlot(
        this IReadOnlyCollection<Curve> curves,
        Rational? upTo = null
    )
    {
        var names = curves.Select((_, i) => $"{(char)('f' + i)}");
        return GetPlot(curves, names, upTo);
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <returns>A <see cref="PlotlyChart"/> object.</returns>
    public static PlotlyChart GetPlot(
        params Curve[] curves
    )
    {
        return GetPlot(curves, null);
    }

    #endregion

    #region Sequences

    /// <summary>
    /// Plots a set of sequences.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="names">The names of the sequences.</param>
    /// <returns>A <see cref="PlotlyChart"/> object.</returns>
    public static PlotlyChart GetPlot(
        this IEnumerable<Sequence> sequences,
        IEnumerable<string> names
    )
    {
        var colors = new List<string>
        {
            "#636EFA",
            "#EF553B",
            "#00CC96",
            "#AB63FA",
            "#FFA15A",
            "#19D3F3",
            "#FF6692",
            "#B6E880",
            "#FF97FF",
            "#FECB52"
        };

        var traces = Enumerable.Zip(sequences, names)
            .SelectMany((ns, i) => GetTrace(ns.First, ns.Second, i));

        var chart = Chart.Plot(traces);

        chart.WithLayout(
            new Layout.Layout
            {
                xaxis = new Xaxis { zeroline = true, showgrid = true, title = "time" },
                yaxis = new Yaxis { zeroline = true, showgrid = true, title = "data" },
                showlegend = true,
                hovermode = "closest"
            }
        );

        return chart;

        IEnumerable<Scattergl> GetTrace(Sequence sequence, string name, int index)
        {
            var color = colors[index % colors.Count];

            if (sequence.IsContinuous)
            {
                var points = sequence.Elements
                    .OfType<Point>()
                    .Select(p => (x: (decimal)p.Time, y: (decimal)p.Value))
                    .ToList();

                if (sequence.IsRightOpen)
                {
                    var tail = sequence.Elements.Last() as Segment;
                    points.Add((x: (decimal)tail.EndTime, y: (decimal)tail.LeftLimitAtEndTime));
                }

                var trace = new Scattergl
                {
                    x = points.Select(p => p.x).ToArray(),
                    y = points.Select(p => p.y).ToArray(),
                    name = name,
                    fillcolor = color,
                    mode = "lines+markers",
                    line = new Line
                    {
                        color = color
                    },
                    marker = new Marker
                    {
                        symbol = "circle",
                        color = color
                    }
                };
                yield return trace;
            }
            else
            {
                var segments = new List<((decimal x, decimal y) a, (decimal x, decimal y) b)>();
                var points = new List<(decimal x, decimal y)>();
                var discontinuities = new List<(decimal x, decimal y)>();

                var breakpoints = sequence.EnumerateBreakpoints();
                foreach (var (left, center, right) in breakpoints)
                {
                    points.Add((x: (decimal)center.Time, y: (decimal)center.Value));
                    if (left is not null && left.LeftLimitAtEndTime != center.Value)
                    {
                        discontinuities.Add((x: (decimal)center.Time, y: (decimal)left.LeftLimitAtEndTime));
                    }

                    if (right is not null)
                    {
                        segments.Add((
                            a: (x: (decimal)right.StartTime, y: (decimal)right.RightLimitAtStartTime),
                            b: (x: (decimal)right.EndTime, y: (decimal)right.LeftLimitAtEndTime)
                        ));
                        if (right.RightLimitAtStartTime != center.Value)
                        {
                            discontinuities.Add((x: (decimal)center.Time, y: (decimal)right.RightLimitAtStartTime));
                        }
                    }
                }

                if (sequence.IsRightOpen)
                {
                    var tail = sequence.Elements.Last() as Segment;
                    segments.Add((
                        a: (x: (decimal)tail.StartTime, y: (decimal)tail.RightLimitAtStartTime),
                        b: (x: (decimal)tail.EndTime, y: (decimal)tail.LeftLimitAtEndTime)
                    ));
                }

                var segmentsLegend = segments.Any();

                bool isFirst = true;
                foreach (var (a, b) in segments)
                {
                    var trace = new Scattergl
                    {
                        x = new[] { a.x, b.x },
                        y = new[] { a.y, b.y },
                        name = name,
                        legendgroup = name,
                        fillcolor = color,
                        mode = "lines",
                        line = new Line
                        {
                            color = color
                        },
                        showlegend = segmentsLegend && isFirst
                    };
                    yield return trace;
                    isFirst = false;
                }

                var pointsTrace = new Scattergl
                {
                    x = points.Select(p => p.x).ToArray(),
                    y = points.Select(p => p.y).ToArray(),
                    name = name,
                    legendgroup = name,
                    fillcolor = color,
                    mode = "markers",
                    line = new Line
                    {
                        color = color
                    },
                    marker = new Marker
                    {
                        symbol = "circle",
                        color = color
                    },
                    showlegend = !segmentsLegend
                };
                yield return pointsTrace;

                var discontinuitiesTrace = new Scattergl
                {
                    x = discontinuities.Select(p => p.x).ToArray(),
                    y = discontinuities.Select(p => p.y).ToArray(),
                    name = name,
                    legendgroup = name,
                    fillcolor = color,
                    mode = "markers",
                    line = new Line
                    {
                        color = color
                    },
                    marker = new Marker
                    {
                        symbol = "circle-open",
                        color = color,
                        line = new Line
                        {
                            color = color
                        }
                    },
                    showlegend = false,
                };
                yield return discontinuitiesTrace;
            }
        }
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <returns>A <see cref="PlotlyChart"/> object.</returns>
    public static PlotlyChart GetPlot(
        this IReadOnlyCollection<Sequence> sequences
    )
    {
        var names = sequences.Select((_, i) => $"{(char)('f' + i)}");
        return GetPlot(sequences, names);
    }

    /// <summary>
    /// Plots a sequence.
    /// </summary>
    /// <param name="sequence">The sequence to plot.</param>
    /// <param name="name">
    /// The name of the sequence.
    /// By default, it captures the expression used for <paramref name="sequence"/>.
    /// </param>
    /// <returns>A <see cref="PlotlyChart"/> object.</returns>
    public static PlotlyChart GetPlot(
        this Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "f"
    )
    {
        return GetPlot([sequence], [name]);
    }

    #endregion
}