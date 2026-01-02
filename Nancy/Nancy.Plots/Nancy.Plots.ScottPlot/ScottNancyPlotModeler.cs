using ScottPlot;
using SkiaSharp;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Plots.ScottPlot;

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

    public override Plot GetPlot(
        IEnumerable<Sequence> sequences,
        IEnumerable<string> names)
    {
        var sequencesList = sequences.ToList();
        var namesList = names.ToList();

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

        var plot = new Plot();
        plot.Font.Set("Lato");

        if(!string.IsNullOrWhiteSpace(PlotSettings.XLabel))
            plot.XLabel(PlotSettings.XLabel);
        if(!string.IsNullOrWhiteSpace(PlotSettings.YLabel))
            plot.YLabel(PlotSettings.YLabel);

        if(!string.IsNullOrEmpty(PlotSettings.Title))
            plot.Title(PlotSettings.Title);

        if(PlotSettings.SameScaleAxes)
            plot.Axes.SquareUnits();
        
        // compute plot bounds, based on explicit settings or current values
        double xLower, xUpper, yLower, yUpper;
        if (PlotSettings.XLimit.HasValue)
        {
            xLower = (double)PlotSettings.XLimit.Value.Lower;
            xUpper = (double)PlotSettings.XLimit.Value.Upper;
        }
        else
        {
            xLower = (double) sequencesList
                .Select(s => s.DefinedFrom)
                .Aggregate(Rational.Min);
            xUpper = (double) sequencesList
                .Select(s => s.DefinedUntil)
                .Aggregate(Rational.Max);
        }
        
        if (PlotSettings.YLimit.HasValue)
        {
            yLower = (double)PlotSettings.YLimit.Value.Lower;
            yUpper = (double)PlotSettings.YLimit.Value.Upper;
        }
        else
        {
            yLower = (double) sequencesList
                .Select(s => s.InfValue())
                .Where(v => v.IsFinite)
                .Aggregate(Rational.Min);
            yUpper = (double) sequencesList
                .Select(s => s.SupValue())
                .Where(v => v.IsFinite)
                .Aggregate(Rational.Max);
        }

        // adjust limits for clarity
        if (PlotSettings.RelativeXAxisMargin != 0)
        {
            var xLength = xUpper - xLower;
            var xAdjustment = xLength > 0 ? xLength * PlotSettings.RelativeXAxisMargin : 1;
            xLower -= xAdjustment;
            xUpper += xAdjustment;
        }

        if (PlotSettings.RelativeYAxisMargin != 0)
        {
            var yLength = yUpper - yLower;
            var yAdjustment = yLength > 0 ? yLength * PlotSettings.RelativeYAxisMargin : 1;
            yLower -= yAdjustment;
            yUpper += yAdjustment;
        }

        // set the axes limits
        plot.Axes.SetLimitsX(xLower, xUpper);
        plot.Axes.SetLimitsY(yLower, yUpper);

        foreach (var (sequence, idx) in sequencesList.WithIndex())
        {
            var color = Color.FromHex(colors[idx % colors.Count]);
            var sequenceTrace = new SequenceTraces(sequence);

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
                    if (!legendApplied)
                    {
                        lineScatter.LegendText = namesList[idx];
                        legendApplied = true;
                    }
                }
            }
        }

        return plot;
    }

    private class SequenceTraces
    {
        public List<List<(double x, double y)>> ContinuousLines { get; } = [];
        
        public List<(double x, double y)> Points { get; } = [];
        
        public List<(double x, double y)> Discontinuities { get; } = [];
        
        public SequenceTraces(Sequence sequence)
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
                if (left is not null and not { IsPlusInfinite: true } &&
                    right is not null and not { IsPlusInfinite: true } &&
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
                if (lastSegment is not { IsPlusInfinite: true }) {
                    var lastCoord = EndCoord(lastSegment);
                    currentLine.Add(lastCoord);
                    ContinuousLines.Add(currentLine);
                    Discontinuities.Add(lastCoord);
                }
            }
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