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
        var axisLimits = PlotAxisLimitAlgorithms.GetSequenceAxisLimits(sequencesList, PlotSettings);

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

        var traces = Enumerable.Zip(sequencesList, namesList)
            .SelectMany((ns, i) => GetTrace(ns.First, ns.Second, i));

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
                xaxis = new Xaxis
                {
                    zeroline = true,
                    showgrid = true,
                    title = PlotSettings.XLabel,
                    range = ToPlotlyRange(axisLimits.XLimit)
                },
                yaxis = new Yaxis
                {
                    zeroline = true,
                    showgrid = true,
                    title = PlotSettings.YLabel,
                    range = ToPlotlyRange(axisLimits.YLimit)
                },
                showlegend = showLegend,
                hovermode = "closest",
                title = PlotSettings.Title,
                width = PlotSettings.Width,
                height = PlotSettings.Height,
                legend = new Legend
                {
                    x = GetLegendX(PlotSettings.LegendPosition),
                    y = GetLegendY(PlotSettings.LegendPosition),
                    xanchor = GetLegendXAnchor(PlotSettings.LegendPosition),
                    yanchor = GetLegendYAnchor(PlotSettings.LegendPosition)
                }
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
