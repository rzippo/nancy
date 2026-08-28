using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using XPlot.Plotly;

namespace Unipi.Nancy.Plots.XPlot.Plotly;

/// <summary>
/// Builds XPlot.Plotly chart models from Nancy sequences.
/// </summary>
public class XPlotNancyPlotModeler : NancyPlotModeler<XPlotPlotSettings, PlotlyChart>
{
    /// <inheritdoc />
    public override PlotlyChart GetPlot(
        IEnumerable<Sequence> sequences,
        IEnumerable<string> names)
    {
        var sequencesList = sequences.ToList();
        var namesList = names.ToList();
        var axisLimits = PlotAxisLimitAlgorithms.SuggestFramingLimits(
            sequencesList, PlotSettings, SequencesContinuePastCut);

        // todo: move colors to settings
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

        var lineStyles = PlotSettings.UseLineStyles
            ? PlotSettings.LineStyles ?? PlotStyleCycles.DefaultLineStyles
            : [ PlotLineStyle.Solid ];

        var areas = GetInfinityAreas(sequencesList, colors, axisLimits, SequencesContinuePastCut);

        // the areas go first, so that plotly draws them under the curves:
        // a layout shape would have gone over them, having no layer of its own
        var traces = areas.Areas.Concat(
            Enumerable.Zip(sequencesList, namesList)
                .SelectMany((ns, i) => GetTrace(ns.First, ns.Second, i)));

        var chart = Chart.Plot(traces);

        var showLegend = PlotSettings.LegendStrategy switch
        {
            LegendStrategy.Auto => sequencesList.Count > 1 || namesList.Any(n => !string.IsNullOrWhiteSpace(n)),
            LegendStrategy.ForceEnable => true,
            LegendStrategy.ForceDisable => false,
            _ => true
        };

        chart.WithLayout(
            new Layout.Layout
            {
                annotations = areas.Annotations,
                xaxis = new Xaxis
                {
                    zeroline = true,
                    showgrid = true,
                    title = PlotSettings.XLabel,
                    range = ToPlotlyRange(axisLimits.XFramingLimit)
                },
                yaxis = new Yaxis
                {
                    zeroline = true,
                    showgrid = true,
                    title = PlotSettings.YLabel,
                    range = ToPlotlyRange(axisLimits.YFramingLimit)
                },
                showlegend = showLegend,
                hovermode = "closest",
                title = PlotSettings.Title,
                width = PlotSettings.Width,
                height = PlotSettings.Height,
                legend = PlotSettings.LegendPlacement == LegendPlacement.Outside
                    // plotly's own default is beside the plot, which is what Outside asks for
                    ? new Legend()
                    : new Legend
                    {
                        x = GetLegendX(PlotSettings.LegendPosition),
                        y = GetLegendY(PlotSettings.LegendPosition),
                        xanchor = GetLegendXAnchor(PlotSettings.LegendPosition),
                        yanchor = GetLegendYAnchor(PlotSettings.LegendPosition)
                    }
            }
        );

        return chart;

        (IEnumerable<Scattergl> Areas, IEnumerable<Annotation> Annotations) GetInfinityAreas(
            IReadOnlyList<Sequence> sequences,
            IReadOnlyList<string> palette,
            PlotAxisLimits limits,
            bool continuesPastEnd)
        {
            var areas = new List<Scattergl>();
            var annotations = new List<Annotation>();
            if (PlotSettings.InfinityStrategy != InfinityStrategy.Areas)
                return (areas, annotations);

            var withInfinities = sequences
                .Select((sequence, index) => (sequence, index))
                .Where(p => p.sequence.HasPlusInfinity || p.sequence.HasMinusInfinity)
                .ToList();

            foreach (var ((sequence, index), position) in withInfinities.WithIndex())
            {
                var color = palette[index % palette.Count];
                foreach (var region in sequence.EnumerateVisibleInfiniteRegions(
                             limits.XFramingLimit, continuesPastEnd))
                {
                    var band = region.IsPlusInfinite
                        ? limits.PlusInfinityBand
                        : limits.MinusInfinityBand;
                    if (region.EndTime <= region.StartTime || band.Upper <= band.Lower)
                        continue;

                    // a closed rectangle, drawn as a trace so that it can sit under the curves
                    var x0 = (decimal)region.StartTime;
                    var x1 = (decimal)region.EndTime;
                    var y0 = (decimal)band.Lower;
                    var y1 = (decimal)band.Upper;
                    areas.Add(new Scattergl
                    {
                        x = new[] { x0, x1, x1, x0, x0 },
                        y = new[] { y0, y0, y1, y1, y0 },
                        mode = "lines",
                        fill = "toself",
                        fillcolor = color,
                        opacity = PlotSettings.InfinityAreaOpacity,
                        line = new Line { width = 0 },
                        hoverinfo = "skip",
                        showlegend = false
                    });

                    annotations.Add(new Annotation
                    {
                        text = region.IsPlusInfinite ? "+∞" : "-∞",
                        xref = "x",
                        yref = "y",
                        x = (decimal)((region.StartTime + region.EndTime) / 2),
                        y = (decimal)(band.Lower + (band.Upper - band.Lower)
                            * (position + 1) / (withInfinities.Count + 1)),
                        showarrow = false,
                        font = new Font { color = color, size = 18 }
                    });
                }
            }

            return (areas, annotations);
        }

        /// <summary>
        /// Maps a line style to the matching plotly dash.
        /// </summary>
        static string ToPlotlyDash(PlotLineStyle style)
            => style switch
            {
                PlotLineStyle.Solid => "solid",
                PlotLineStyle.Dashed => "dash",
                PlotLineStyle.Dotted => "dot",
                PlotLineStyle.DashDotted => "dashdot",
                // plotly has no denser variants: longdash keeps the default cycle's four distinct,
                // while a densely dotted line is indistinguishable from a dotted one here
                PlotLineStyle.DenselyDashed => "longdash",
                PlotLineStyle.DenselyDotted => "dot",
                _ => "solid"
            };

        IEnumerable<Scattergl> GetTrace(Sequence sequence, string name, int index)
        {
            var color = colors[index % colors.Count];

            var dash = ToPlotlyDash(
                PlotStyleCycles.Pick(index, lineStyles));

            // a sequence that is infinite throughout is continuous, so this path has to
            // filter as the one below does: casting an infinity to decimal throws
            if (sequence.IsContinuous && !sequence.HasPlusInfinity && !sequence.HasMinusInfinity)
            {
                var points = sequence.Elements
                    .OfType<Point>()
                    .Select(p => (x: (decimal)p.Time, y: (decimal)p.Value))
                    .ToList();

                if (sequence.IsRightOpen)
                {
                    var tail = (Segment)sequence.Elements.Last();
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
                        color = color,
                        dash = dash
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
                    if (center.IsInfinite)
                    {
                        System.Diagnostics.Trace.WriteLine($"Warning: skipping infinite point at time {center.Time}");
                    }
                    else
                        points.Add((x: (decimal)center.Time, y: (decimal)center.Value));
                    if (left is not null && left.LeftLimitAtEndTime != center.Value)
                    {
                        if(left.IsInfinite)
                        {
                            System.Diagnostics.Trace.WriteLine($"Warning: skipping infinite discontinuity value at time {center.Time}");
                        }
                        else
                            discontinuities.Add((x: (decimal)center.Time, y: (decimal)left.LeftLimitAtEndTime));
                    }

                    if (right is not null)
                    {
                        if (right.IsInfinite)
                        {
                            System.Diagnostics.Trace.WriteLine($"Warning: skipping infinite segment [{right.StartTime}, {right.EndTime}]");
                        }
                        else
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
                }

                if (sequence.IsRightOpen)
                {
                    var tail = (Segment)sequence.Elements.Last();
                    if (tail.IsInfinite)
                    {
                        System.Diagnostics.Trace.WriteLine("Warning: skipping infinite tail segment");
                    }
                    else
                    {
                        segments.Add((
                            a: (x: (decimal)tail.StartTime, y: (decimal)tail.RightLimitAtStartTime),
                            b: (x: (decimal)tail.EndTime, y: (decimal)tail.LeftLimitAtEndTime)
                        ));
                    }
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
                            color = color,
                            dash = dash
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

        static object[] ToPlotlyRange(Interval limit)
        {
            return [
                (decimal)limit.Lower,
                (decimal)limit.Upper
            ];
        }

        static double GetLegendX(LegendPosition position) => position switch
        {
            LegendPosition.NorthWest or LegendPosition.West or LegendPosition.SouthWest => 0.0,
            LegendPosition.North or LegendPosition.South => 0.5,
            _ => 1.0
        };

        static double GetLegendY(LegendPosition position) => position switch
        {
            LegendPosition.NorthWest or LegendPosition.North or LegendPosition.NorthEast => 1.0,
            LegendPosition.West or LegendPosition.East => 0.5,
            _ => 0.0
        };

        static string GetLegendXAnchor(LegendPosition position) => position switch
        {
            LegendPosition.NorthWest or LegendPosition.West or LegendPosition.SouthWest => "left",
            LegendPosition.North or LegendPosition.South => "center",
            _ => "right"
        };

        static string GetLegendYAnchor(LegendPosition position) => position switch
        {
            LegendPosition.NorthWest or LegendPosition.North or LegendPosition.NorthEast => "top",
            LegendPosition.West or LegendPosition.East => "middle",
            _ => "bottom"
        };
    }
}
