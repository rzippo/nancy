using ScottPlot;
using SkiaSharp;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using PlotAxisLimitAlgorithms = global::Unipi.Nancy.Plots.PlotAxisLimitAlgorithms;
using Hatches = global::ScottPlot.Hatches;

namespace Unipi.Nancy.Plots.ScottPlot;

/// <summary>
/// Builds ScottPlot plot models from Nancy sequences.
/// </summary>
public class ScottNancyPlotModeler : NancyPlotModeler<ScottPlotSettings, Plot>
{
    static ScottNancyPlotModeler()
    {
        // register custom font
        var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Fonts", "Lato-Regular.ttf");
        if (!File.Exists(fontPath))
            throw new FileNotFoundException("Font file not found", fontPath);
        Fonts.AddFontFile("Lato", fontPath);
    }

    /// <inheritdoc />
    public override Plot GetPlot(
        IEnumerable<Sequence> sequences,
        IEnumerable<string> names)
    {
        var sequencesList = sequences.ToList();
        var namesList = names.ToList();

        var lineStyles = PlotSettings.UseLineStyles
            ? PlotSettings.LineStyles ?? PlotStyleCycles.DefaultLineStyles
            : [ PlotLineStyle.Solid ];
        var fillPatterns = PlotSettings.FillPatterns ?? PlotStyleCycles.DefaultFillPatterns;

        var plot = new Plot();
        plot.Font.Set("Lato");

        // ScottPlot's own palette, rather than one copied from another backend
        var palette = plot.Add.Palette;
        var colorCount = palette.Colors.Length;

        if(!string.IsNullOrWhiteSpace(PlotSettings.XLabel))
            plot.XLabel(PlotSettings.XLabel);
        if(!string.IsNullOrWhiteSpace(PlotSettings.YLabel))
            plot.YLabel(PlotSettings.YLabel);

        if(!string.IsNullOrEmpty(PlotSettings.Title))
            plot.Title(PlotSettings.Title);

        if(PlotSettings.SameScaleAxes)
            plot.Axes.SquareUnits();
        
        var axisLimits = PlotAxisLimitAlgorithms.SuggestAxisLimits(
            sequencesList, PlotSettings, SequencesContinuePastCut);

        // set the axes limits
        plot.Axes.SetLimitsX(
            (double)axisLimits.XLimit.Lower,
            (double)axisLimits.XLimit.Upper);
        plot.Axes.SetLimitsY(
            (double)axisLimits.YLimit.Lower,
            (double)axisLimits.YLimit.Upper);

        // the areas are added first, so that the curves are drawn over them
        if (PlotSettings.InfinityStrategy == InfinityStrategy.Areas)
        {
            var withInfinities = sequencesList
                .Select((sequence, idx) => (sequence, idx))
                .Where(p => p.sequence.HasPlusInfinity || p.sequence.HasMinusInfinity)
                .ToList();

            foreach (var ((sequence, idx), position) in withInfinities.WithIndex())
            {
                var color = palette.GetColor(idx);
                var pattern = PlotStyleCycles.Pick(idx, fillPatterns);
                foreach (var region in sequence.EnumerateVisibleInfiniteRegions(
                             axisLimits.XLimit, SequencesContinuePastCut))
                    AddInfinityArea(
                        plot, region, axisLimits, color, pattern, position, withInfinities.Count);
            }
        }

        foreach (var (sequence, idx) in sequencesList.WithIndex())
        {
            var color = palette.GetColor(idx);
            var linePattern = ToLinePattern(PlotStyleCycles.Pick(idx, lineStyles));
            var sequenceTrace = new SequenceTraces(
                sequence,
                sequence.GetTrailingContinuation(axisLimits.XLimit, SequencesContinuePastCut));

            if (sequenceTrace.Points.Any())
            {
                var pointCoordinates = sequenceTrace.Points
                    .Select(p => new Coordinates(p.x, p.y))
                    .ToArray();
                var pointsScatter = plot.Add.ScatterPoints(pointCoordinates);
                pointsScatter.Color = color;
                pointsScatter.MarkerShape = MarkerShape.FilledCircle;
                if (!sequenceTrace.ContinuousLines.Any())
                    pointsScatter.LegendText = namesList[idx];
            }

            if (sequenceTrace.Discontinuities.Any())
            {
                var discontinuityCoordinates = sequenceTrace.Discontinuities
                    .Select(p => new Coordinates(p.x, p.y))
                    .ToArray();
                var discontinuityScatter = plot.Add.ScatterPoints(discontinuityCoordinates);
                discontinuityScatter.Color = color;
                discontinuityScatter.MarkerShape = MarkerShape.OpenCircle;
                // discontinuityScatter.LegendText = namesList[idx];
            }

            if (sequenceTrace.ContinuousLines.Any())
            {
                var legendApplied = false;
                foreach (var continuousLine in sequenceTrace.ContinuousLines)
                {
                    var coordinates = continuousLine
                        .Select(p => new Coordinates(p.x, p.y))
                        .ToArray();
                    var lineScatter = plot.Add.ScatterLine(coordinates);
                    lineScatter.Color = color;
                    lineScatter.LinePattern = linePattern;
                    if (!legendApplied)
                    {
                        lineScatter.LegendText = namesList[idx];
                        legendApplied = true;
                    }
                }
            }
        }

        var showLegend = PlotSettings.LegendStrategy switch
        {
            LegendStrategy.Auto => sequencesList.Count > 1 || namesList.Any(n => !string.IsNullOrWhiteSpace(n)),
            LegendStrategy.ForceEnable => true,
            LegendStrategy.ForceDisable => false,
            _ => true
        };
        plot.Legend.IsVisible = showLegend;
        if (showLegend && PlotSettings.LegendPlacement == LegendPlacement.Outside)
            plot.ShowLegend(PlotSettings.LegendPosition switch
            {
                LegendPosition.North => Edge.Top,
                LegendPosition.South => Edge.Bottom,
                LegendPosition.West or LegendPosition.NorthWest or LegendPosition.SouthWest => Edge.Left,
                _ => Edge.Right
            });
        plot.Legend.Alignment = PlotSettings.LegendPosition switch
        {
            LegendPosition.North => Alignment.UpperCenter,
            LegendPosition.NorthEast => Alignment.UpperRight,
            LegendPosition.East => Alignment.MiddleRight,
            LegendPosition.SouthEast => Alignment.LowerRight,
            LegendPosition.South => Alignment.LowerCenter,
            LegendPosition.SouthWest => Alignment.LowerLeft,
            LegendPosition.West => Alignment.MiddleLeft,
            LegendPosition.NorthWest => Alignment.UpperLeft,
            _ => Alignment.LowerRight
        };

        return plot;
    }

    /// <summary>
    /// Adds the area marking an infinite part of a sequence.
    /// </summary>
    /// <param name="plot">The plot to add to.</param>
    /// <param name="region">The infinite part to mark.</param>
    /// <param name="axisLimits">The limits of the plot, which reserved the room for the area.</param>
    /// <param name="color">The color of the sequence.</param>
    /// <param name="pattern">The fill pattern of the sequence.</param>
    /// <param name="position">The position of the sequence among those that have infinite parts.</param>
    /// <param name="count">The number of sequences that have infinite parts.</param>
    /// <remarks>
    /// The label is staggered by <paramref name="position"/>, so that overlapping areas do not write over each other.
    /// </remarks>
    private static void AddInfinityArea(
        Plot plot,
        InfiniteRegion region,
        PlotAxisLimits axisLimits,
        Color color,
        PlotFillPattern pattern,
        int position,
        int count)
    {
        var band = region.IsPlusInfinite
            ? axisLimits.PlusInfinityBand
            : axisLimits.MinusInfinityBand;

        var left = (double)region.StartTime;
        var right = (double)region.EndTime;
        var bottom = (double)band.Lower;
        var top = (double)band.Upper;
        if (right <= left)
            return;

        var area = plot.Add.Rectangle(left, right, bottom, top);
        // the background is kept faint so that overlapping areas still show both patterns,
        // but not transparent, which makes ScottPlot skip the hatch altogether
        area.FillColor = color.WithAlpha(.18);
        area.FillHatch = ToHatch(pattern);
        area.FillHatchColor = color;
        area.LineWidth = 0;

        var label = plot.Add.Text(
            region.IsPlusInfinite ? "+∞" : "-∞",
            (left + right) / 2,
            bottom + (top - bottom) * (position + 1) / (count + 1));
        label.LabelFontColor = color;
        label.LabelFontSize = 24;
        label.LabelBold = true;
        label.Alignment = Alignment.MiddleCenter;
    }

    /// <summary>
    /// Maps a <see cref="PlotLineStyle"/> to the matching ScottPlot pattern.
    /// </summary>
    /// <remarks>
    /// ScottPlot has no dash-dot pattern, so <see cref="PlotLineStyle.DashDotted"/> is rendered as <see cref="PlotLineStyle.Dashed"/>.
    /// A cycle using both will have two entries that look alike.
    /// </remarks>
    private static LinePattern ToLinePattern(PlotLineStyle style)
    {
        return style switch
        {
            PlotLineStyle.Solid => LinePattern.Solid,
            PlotLineStyle.Dashed => LinePattern.Dashed,
            PlotLineStyle.Dotted => LinePattern.Dotted,
            PlotLineStyle.DashDotted => LinePattern.Dashed,
            PlotLineStyle.DenselyDashed => LinePattern.DenselyDashed,
            PlotLineStyle.DenselyDotted => LinePattern.Dotted,
            _ => LinePattern.Solid
        };
    }

    /// <summary>
    /// Maps a <see cref="PlotFillPattern"/> to the matching ScottPlot hatch.
    /// </summary>
    private static IHatch ToHatch(PlotFillPattern pattern)
    {
        return pattern switch
        {
            PlotFillPattern.Dots => new Hatches.Dots(),
            PlotFillPattern.DenseDots => new Hatches.Checker(),
            PlotFillPattern.DiagonalLines => new Hatches.Striped(Hatches.StripeDirection.DiagonalUp),
            PlotFillPattern.ReverseDiagonalLines => new Hatches.Striped(Hatches.StripeDirection.DiagonalDown),
            PlotFillPattern.Grid => new Hatches.Grid(rotate: false),
            PlotFillPattern.Crosshatch => new Hatches.Grid(rotate: true),
            _ => new Hatches.Dots()
        };
    }

    private class SequenceTraces
    {
        public List<List<(double x, double y)>> ContinuousLines { get; } = [];
        
        public List<(double x, double y)> Points { get; } = [];
        
        public List<(double x, double y)> Discontinuities { get; } = [];
        
        public SequenceTraces(Sequence sequence, TrailingContinuation? continuation)
        {
            var currentLine = new List<(double x, double y)>();
            if (sequence.IsLeftOpen)
            {
                var firstSegment = (Segment)sequence.Elements.First();
                var startCoord = StartCoord(firstSegment);
                Discontinuities.Add(startCoord);
                currentLine = [ startCoord ];   
            }
            var breakpoints = sequence.EnumerateBreakpoints();
            foreach (var (left, center, right) in breakpoints)
            {
                if (left is not null and not { IsInfinite: true } &&
                    right is not null and not { IsInfinite: true } &&
                    left.LeftLimitAtEndTime == center.Value && center.Value == right.RightLimitAtStartTime
                   )
                {
                    // continue the current line
                    currentLine.Add(Coord(center));
                }
                else
                {
                    if (left is null or {IsInfinite: true})
                    {
                        // no line is running yet
                        if (center is not {IsInfinite: true})
                            Points.Add(Coord(center));
                        if (right is not null and not { IsInfinite: true })
                        {
                            var startCoord = ((double)right.StartTime, (double)right.RightLimitAtStartTime);
                            if(right.RightLimitAtStartTime != center.Value)
                                Discontinuities.Add(startCoord);
                            // start new line
                            currentLine = [startCoord];
                        }
                    }
                    else
                    {
                        // left is finite, and assumed within the sequence
                        // first, continue the running line
                        var leftEndCoord = EndCoord(left); 
                        currentLine.Add(leftEndCoord);
                        // if any discontinuity occurs, break the line
                        // by above checks, the discontinuity SHOULD occurr
                        if (center is { IsInfinite: true } ||
                            right is null or { IsInfinite: true } ||
                            left.LeftLimitAtEndTime != center.Value ||
                            center.Value != right.RightLimitAtStartTime
                           )
                        {
                            ContinuousLines.Add(currentLine);
                            if (left.LeftLimitAtEndTime != center.Value)
                                Discontinuities.Add(leftEndCoord);
                            if (center is not {IsInfinite:true})
                                Points.Add(Coord(center));
                            if (right is not null and not { IsInfinite: true })
                            {
                                // start new line immediately
                                var rightStartCoord = StartCoord(right);
                                currentLine = [rightStartCoord];
                                if(center.Value != right.RightLimitAtStartTime)
                                    Discontinuities.Add(rightStartCoord);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("Should never get here!");
                        }
                    }
                }
            }

            if (sequence.IsRightOpen)
            {
                var lastSegment = (Segment)sequence.Elements.Last();
                if (lastSegment is not { IsInfinite: true }) {
                    var lastCoord = EndCoord(lastSegment);
                    currentLine.Add(lastCoord);
                    ContinuousLines.Add(currentLine);
                    Discontinuities.Add(lastCoord);
                }
            }

            if (continuation is { } trailing)
                ExtendToRightEdge(trailing);
        }

        /// <summary>
        /// Carries the last line on to the right edge of the plot, dropping the mark at the cut.
        /// </summary>
        /// <remarks>
        /// Whether the data goes on past the plot is decided in the shared layer, so that every renderer answers it the same way.
        /// </remarks>
        private void ExtendToRightEdge(TrailingContinuation continuation)
        {
            if (ContinuousLines.Count == 0)
                return;

            var line = ContinuousLines[^1];
            if (line.Count == 0)
                return;

            var end = line[^1];
            line.Add((x: (double)continuation.Time, y: (double)continuation.Value));

            // the mark sat on the cut, which is not an endpoint of the curve
            Points.RemoveAll(p => p.x == end.x && p.y == end.y);
        }
    }

    private static (double x, double y) StartCoord(Segment segment)
    {
        return ((double)segment.StartTime, (double)segment.RightLimitAtStartTime);
    }
    
    private static (double x, double y) EndCoord(Segment segment)
    {
        return ((double)segment.EndTime, (double)segment.LeftLimitAtEndTime);
    }

    private static (double x, double y) Coord(Point point)
    {
        return ((double)point.Time, (double)point.Value);
    }

    /// <summary>
    /// Checks if the environment has a usable default system font.
    /// </summary>
    /// <param name="familyName">The family name of that font, if exists.</param>
    private static bool HasUsableSystemFont(out string? familyName)
    {
        familyName = null;
        try
        {
            using var typeface = SKTypeface.Default;

            if (typeface == null)
                return false;

            // Some broken environments return a typeface with no family name
            familyName = typeface.FamilyName;

            if (string.IsNullOrWhiteSpace(familyName))
                return false;

            // Optional sanity check: attempt to create a font
            using var font = new SKFont(typeface);
            return font.Size > 0;
        }
        catch
        {
            return false;
        }
    }
}
