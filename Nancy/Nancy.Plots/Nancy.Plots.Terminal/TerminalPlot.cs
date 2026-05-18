using System.Globalization;
using System.Text;
using Spectre.Console;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Plots.Terminal;

/// <summary>
/// Represents a terminal plot built from one or more sequences.
/// </summary>
public class TerminalPlot
{
    private static readonly IReadOnlyList<string> DefaultColors =
    [
        "blue",
        "red",
        "green",
        "yellow",
        "magenta",
        "cyan"
    ];

    /// <summary>
    /// Sequences included in the plot.
    /// </summary>
    public IReadOnlyList<TerminalSequenceToPlot> SequencesToPlot { get; }

    /// <summary>
    /// Plot rendering settings.
    /// </summary>
    public TerminalPlotSettings Settings { get; set; } = new();

    /// <summary>
    /// Builds a terminal plot from the given sequences.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="names">The names to use for the sequences.</param>
    /// <param name="settings">Optional settings for the plot.</param>
    public TerminalPlot(
        IReadOnlyList<Sequence> sequences,
        IReadOnlyList<string>? names,
        TerminalPlotSettings? settings)
    {
        if (sequences.Count == 0)
            throw new ArgumentException("Empty trace collection.", nameof(sequences));

        Settings = settings ?? new TerminalPlotSettings();

        if (Settings.Width < 10)
            throw new ArgumentOutOfRangeException(nameof(settings), "Terminal plot width must be at least 10.");
        if (Settings.Height < 5)
            throw new ArgumentOutOfRangeException(nameof(settings), "Terminal plot height must be at least 5.");
        if (Settings.XAxisTickCount < 2)
            throw new ArgumentOutOfRangeException(nameof(settings), "Terminal plot x-axis tick count must be at least 2.");
        if (Settings.YAxisTickCount < 2)
            throw new ArgumentOutOfRangeException(nameof(settings), "Terminal plot y-axis tick count must be at least 2.");

        names ??= sequences.Count > 1
            ? GetDefaultNames(sequences.Count)
            : [string.Empty];

        if (names.Count < sequences.Count)
            throw new ArgumentException("Mismatch between number of traces and their names.", nameof(names));

        SequencesToPlot = Enumerable.Range(0, sequences.Count)
            .Select(i => new TerminalSequenceToPlot(
                sequences[i],
                names[i],
                DefaultColors[i % DefaultColors.Count]))
            .ToList();
    }

    /// <summary>
    /// Writes the plot to a Spectre.Console console.
    /// </summary>
    public void WriteTo(IAnsiConsole console)
    {
        console.Markup(ToMarkup());
    }

    /// <summary>
    /// Renders the plot as terminal text.
    /// </summary>
    public string ToTerminalString(TerminalPlotAnsiMode? ansiMode = null)
    {
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = ToAnsiSupport(ansiMode ?? Settings.AnsiMode),
            ColorSystem = ToColorSystemSupport(ansiMode ?? Settings.AnsiMode),
            Out = new AnsiConsoleOutput(writer)
        });

        WriteTo(console);
        return writer.ToString();
    }

    /// <summary>
    /// Produces Spectre.Console markup for this plot.
    /// </summary>
    public string ToMarkup()
    {
        var sequences = SequencesToPlot.Select(stp => stp.Sequence).ToList();
        var axisLimits = GetSequenceAxisLimits(sequences, Settings);
        var grid = BuildGrid(axisLimits);
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(Settings.Title))
        {
            sb.Append("[bold]");
            sb.Append(Markup.Escape(Settings.Title));
            sb.AppendLine("[/]");
        }

        AppendPlotRows(sb, grid, axisLimits);

        if (Settings.DrawAxisLabels)
            AppendAxisLabels(sb, axisLimits);

        if (ShouldIncludeLegend())
            AppendLegend(sb);

        return sb.ToString();
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
        for (var i = 0; i < n; i++)
        {
            var round = i / 26;
            var index = i % 26;
            var indexFromA = ((firstLetter - 'a') + index) % 26;
            var letter = (char)('a' + indexFromA);
            result.Add(round > 0 ? $"{letter}{round}" : letter.ToString());
        }

        return result;
    }

    private Cell[,] BuildGrid(AxisLimits axisLimits)
    {
        var grid = new Cell[Settings.Height, Settings.Width];

        if (Settings.DrawAxes)
            DrawAxes(grid, axisLimits);

        foreach (var sequenceToPlot in SequencesToPlot)
            DrawSequence(grid, axisLimits, sequenceToPlot);

        return grid;
    }

    private void DrawAxes(Cell[,] grid, AxisLimits axisLimits)
    {
        if (axisLimits.YLimit.Contains(0))
        {
            var y = MapY((Rational)0, axisLimits);
            for (var x = 0; x < Settings.Width; x++)
                SetCell(grid, x, y, '-', "grey", 1);
        }

        if (axisLimits.XLimit.Contains(0))
        {
            var x = MapX((Rational)0, axisLimits);
            for (var y = 0; y < Settings.Height; y++)
                SetCell(grid, x, y, '|', "grey", 1);
        }

        if (axisLimits.XLimit.Contains(0) && axisLimits.YLimit.Contains(0))
            SetCell(grid, MapX((Rational)0, axisLimits), MapY((Rational)0, axisLimits), '+', "grey", 2);
    }

    private void DrawSequence(Cell[,] grid, AxisLimits axisLimits, TerminalSequenceToPlot sequenceToPlot)
    {
        foreach (var segment in sequenceToPlot.Sequence.Elements.OfType<Segment>())
            DrawSegment(grid, axisLimits, segment, sequenceToPlot.Color);

        foreach (var point in sequenceToPlot.Sequence.Elements.OfType<Point>())
            DrawPoint(grid, axisLimits, point.Time, point.Value, Settings.PointCharacter, sequenceToPlot.Color, 4);

        foreach (var (left, center, right) in sequenceToPlot.Sequence.EnumerateBreakpoints())
        {
            if (left is { IsFinite: true } && left.LeftLimitAtEndTime != center.Value)
                DrawPoint(
                    grid,
                    axisLimits,
                    center.Time,
                    left.LeftLimitAtEndTime,
                    Settings.DiscontinuityCharacter,
                    sequenceToPlot.Color,
                    3);

            if (right is { IsFinite: true } && right.RightLimitAtStartTime != center.Value)
                DrawPoint(
                    grid,
                    axisLimits,
                    center.Time,
                    right.RightLimitAtStartTime,
                    Settings.DiscontinuityCharacter,
                    sequenceToPlot.Color,
                    3);
        }
    }

    private void DrawSegment(Cell[,] grid, AxisLimits axisLimits, Segment segment, string color)
    {
        if (!segment.IsFinite)
            return;

        var startTime = Rational.Max(segment.StartTime, axisLimits.XLimit.Lower);
        var endTime = Rational.Min(segment.EndTime, axisLimits.XLimit.Upper);
        if (startTime > endTime)
            return;

        var startValue = ValueAtClosed(segment, startTime);
        var endValue = ValueAtClosed(segment, endTime);
        if (!startValue.IsFinite || !endValue.IsFinite)
            return;

        var x0 = MapX(startTime, axisLimits);
        var y0 = MapY(startValue, axisLimits);
        var x1 = MapX(endTime, axisLimits);
        var y1 = MapY(endValue, axisLimits);
        var character = GetLineCharacter(x0, y0, x1, y1);

        DrawLine(grid, x0, y0, x1, y1, character, color);
    }

    private void DrawLine(Cell[,] grid, int x0, int y0, int x1, int y1, char character, string color)
    {
        var dx = Math.Abs(x1 - x0);
        var sx = x0 < x1 ? 1 : -1;
        var dy = -Math.Abs(y1 - y0);
        var sy = y0 < y1 ? 1 : -1;
        var error = dx + dy;

        while (true)
        {
            SetCell(grid, x0, y0, character, color, 2);
            if (x0 == x1 && y0 == y1)
                break;

            var e2 = 2 * error;
            if (e2 >= dy)
            {
                error += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }

    private void DrawPoint(
        Cell[,] grid,
        AxisLimits axisLimits,
        Rational time,
        Rational value,
        char character,
        string color,
        int priority)
    {
        if (!time.IsFinite || !value.IsFinite)
            return;
        if (!axisLimits.XLimit.Contains(time) || !axisLimits.YLimit.Contains(value))
            return;

        SetCell(grid, MapX(time, axisLimits), MapY(value, axisLimits), character, color, priority);
    }

    private void SetCell(Cell[,] grid, int x, int y, char character, string style, int priority)
    {
        if (x < 0 || x >= Settings.Width || y < 0 || y >= Settings.Height)
            return;

        var current = grid[y, x];
        if (current.IsEmpty || priority > current.Priority)
        {
            grid[y, x] = new Cell(character, style, priority);
            return;
        }

        if (priority == current.Priority && current.Style != style)
            grid[y, x] = new Cell(Settings.CollisionCharacter, "white", priority + 1);
    }

    private void AppendPlotRows(StringBuilder sb, Cell[,] grid, AxisLimits axisLimits)
    {
        var yLabels = BuildYAxisLabels(axisLimits);
        for (var row = 0; row < Settings.Height; row++)
        {
            var label = yLabels.TryGetValue(row, out var yLabel) ? yLabel : string.Empty;
            sb.Append(Markup.Escape(label.PadLeft(Settings.YAxisLabelWidth)));
            sb.Append(' ');

            for (var column = 0; column < Settings.Width; column++)
                sb.Append(ToMarkup(grid[row, column]));

            sb.AppendLine();
        }
    }

    private void AppendAxisLabels(StringBuilder sb, AxisLimits axisLimits)
    {
        sb.Append(' ', Settings.YAxisLabelWidth + 1);
        sb.Append(Markup.Escape(BuildXAxisLabelLine(axisLimits)));
        sb.AppendLine();

        var labelLine = string.Join(
            "  ",
            new[]
            {
                string.IsNullOrWhiteSpace(Settings.XLabel) ? null : $"x: {Settings.XLabel}",
                string.IsNullOrWhiteSpace(Settings.YLabel) ? null : $"y: {Settings.YLabel}"
            }.Where(label => label is not null));

        if (!string.IsNullOrWhiteSpace(labelLine))
        {
            sb.Append(' ', Settings.YAxisLabelWidth + 1);
            sb.Append("[grey]");
            sb.Append(Markup.Escape(labelLine));
            sb.AppendLine("[/]");
        }
    }

    private void AppendLegend(StringBuilder sb)
    {
        sb.Append("[grey]Legend:[/] ");
        foreach (var sequenceToPlot in SequencesToPlot)
        {
            sb.Append('[');
            sb.Append(sequenceToPlot.Color);
            sb.Append(']');
            sb.Append(Settings.PointCharacter);
            sb.Append("[/] ");
            sb.Append(Markup.Escape(sequenceToPlot.Name));
            sb.Append("  ");
        }

        sb.AppendLine();
        sb.Append("[grey]Symbols:[/] ");
        AppendSymbolMeaning(sb, Settings.PointCharacter.ToString(), "point");
        AppendSymbolMeaning(sb, Settings.DiscontinuityCharacter.ToString(), "discontinuity");
        AppendSymbolMeaning(sb, Settings.CollisionCharacter.ToString(), "overlap");
        if (Settings.DrawAxes)
            AppendSymbolMeaning(sb, "+", "origin");

        sb.AppendLine();
        sb.Append("[grey]Segments:[/] ");
        AppendSymbolMeaning(sb, "-", "horizontal");
        AppendSymbolMeaning(sb, "/", "rising");
        AppendSymbolMeaning(sb, "\\", "falling");
        AppendSymbolMeaning(sb, "|", "vertical");
        sb.AppendLine();
    }

    private static void AppendSymbolMeaning(StringBuilder sb, string symbol, string meaning)
    {
        sb.Append(Markup.Escape(symbol));
        sb.Append(' ');
        sb.Append(Markup.Escape(meaning));
        sb.Append("  ");
    }

    private bool ShouldIncludeLegend()
    {
        return Settings.LegendStrategy switch
        {
            LegendStrategy.Auto => SequencesToPlot.Count > 1 ||
                                   !string.IsNullOrWhiteSpace(SequencesToPlot.Single().Name),
            LegendStrategy.ForceEnable => true,
            LegendStrategy.ForceDisable => false,
            _ => true
        };
    }

    private Dictionary<int, string> BuildYAxisLabels(AxisLimits axisLimits)
    {
        var labels = new Dictionary<int, string>();

        foreach (var tick in BuildTickLabels(
                     axisLimits.YLimit,
                     Settings.YAxisTickCount,
                     GetPreferredYAxisTickValues(axisLimits),
                     Settings.YAxisTickStrategy,
                     Settings.TickLabelStyle))
            labels[MapY(tick.Value, axisLimits)] = tick.Label;

        labels[0] = Format(axisLimits.YLimit.Upper, Settings.TickLabelStyle);
        labels[Settings.Height - 1] = Format(axisLimits.YLimit.Lower, Settings.TickLabelStyle);

        if (axisLimits.YLimit.Contains(0))
            labels[MapY(0, axisLimits)] = "0";

        return labels;
    }

    private string BuildXAxisLabelLine(AxisLimits axisLimits)
    {
        var labels = Enumerable.Repeat(' ', Settings.Width).ToArray();
        var lower = axisLimits.XLimit.Lower;
        var upper = axisLimits.XLimit.Upper;
        var lowerLabel = Format(axisLimits.XLimit.Lower, Settings.TickLabelStyle);
        var upperLabel = Format(axisLimits.XLimit.Upper, Settings.TickLabelStyle);

        PlaceLabel(labels, 0, lowerLabel, overwrite: true);
        PlaceLabel(labels, Settings.Width - upperLabel.Length, upperLabel, overwrite: true);

        foreach (var tick in BuildTickLabels(
                         axisLimits.XLimit,
                         Settings.XAxisTickCount,
                         GetPreferredXAxisTickValues(axisLimits),
                         Settings.XAxisTickStrategy,
                         Settings.TickLabelStyle)
                     .Where(tick => tick.Value != lower && tick.Value != upper)
                     .OrderBy(tick => tick.Value))
        {
            var column = MapX(tick.Value, axisLimits);
            TryPlaceLabel(labels, column - tick.Label.Length / 2, tick.Label);
        }

        if (axisLimits.XLimit.Contains(0) && lower != 0 && upper != 0)
        {
            const string zeroLabel = "0";
            TryPlaceLabel(labels, MapX((Rational)0, axisLimits), zeroLabel);
        }

        return new string(labels).TrimEnd();
    }

    private IEnumerable<Rational> GetPreferredXAxisTickValues(AxisLimits axisLimits)
    {
        return SequencesToPlot
            .SelectMany(sequenceToPlot => sequenceToPlot.Sequence.Elements.SelectMany(GetElementBoundaryTimes))
            .Where(time => time.IsFinite && axisLimits.XLimit.Contains(time));
    }

    private IEnumerable<Rational> GetPreferredYAxisTickValues(AxisLimits axisLimits)
    {
        return SequencesToPlot
            .SelectMany(sequenceToPlot => sequenceToPlot.Sequence.Elements.SelectMany(GetElementBoundaryValues))
            .Where(value => value.IsFinite && axisLimits.YLimit.Contains(value));
    }

    private static IReadOnlyList<TickLabel> BuildTickLabels(
        Interval limit,
        int tickCount,
        IEnumerable<Rational> preferredValues,
        TerminalPlotTickStrategy tickStrategy,
        TerminalPlotTickLabelStyle tickLabelStyle)
    {
        var lower = limit.Lower;
        var upper = limit.Upper;
        var maxInteriorTickCount = Math.Max(0, tickCount - 2);
        var labels = new List<Rational> { lower, upper };

        if (tickStrategy == TerminalPlotTickStrategy.PreferBreakpoints)
        {
            var preferredInteriorValues = preferredValues
                .Where(value => value > lower && value < upper)
                .Distinct()
                .Order()
                .ToList();

            foreach (var value in SelectPreferredInteriorTicks(
                         preferredInteriorValues,
                         lower,
                         upper,
                         maxInteriorTickCount))
            {
                AddDistinct(labels, value);
            }
        }

        foreach (var value in GetEvenlySpacedInteriorTicks(lower, upper, tickCount))
        {
            if (labels.Count >= tickCount)
                break;

            AddDistinct(labels, value);
        }

        return labels
            .Distinct()
            .Order()
            .Select(value => new TickLabel(value, Format(value, tickLabelStyle)))
            .ToList();
    }

    private static IEnumerable<Rational> SelectPreferredInteriorTicks(
        IReadOnlyCollection<Rational> preferredValues,
        Rational lower,
        Rational upper,
        int maxCount)
    {
        if (maxCount <= 0)
            return [];
        if (preferredValues.Count <= maxCount)
            return preferredValues;

        var selected = new List<Rational>();
        for (var i = 1; i <= maxCount; i++)
        {
            var target = lower + (upper - lower) * i / (maxCount + 1);
            var nearest = preferredValues
                .Where(value => selected.All(selectedValue => selectedValue != value))
                .OrderBy(value => Math.Abs((double)(value - target)))
                .First();
            selected.Add(nearest);
        }

        return selected.Order();
    }

    private static IEnumerable<Rational> GetEvenlySpacedInteriorTicks(Rational lower, Rational upper, int tickCount)
    {
        for (var i = 1; i < tickCount - 1; i++)
            yield return lower + (upper - lower) * i / (tickCount - 1);
    }

    private static void AddDistinct(ICollection<Rational> values, Rational value)
    {
        if (values.All(existingValue => existingValue != value))
            values.Add(value);
    }

    private static void PlaceLabel(char[] labels, int start, string label, bool overwrite)
    {
        label = TrimLabelToWidth(label, labels.Length);
        if (label.Length == 0)
            return;

        start = Math.Clamp(start, 0, labels.Length - label.Length);
        if (!overwrite && !CanPlaceLabel(labels, start, label))
            return;

        for (var i = 0; i < label.Length; i++)
            labels[start + i] = label[i];
    }

    private static void TryPlaceLabel(char[] labels, int start, string label)
    {
        PlaceLabel(labels, start, label, overwrite: false);
    }

    private static bool CanPlaceLabel(char[] labels, int start, string label)
    {
        if (start < 0 || start + label.Length > labels.Length)
            return false;

        return Enumerable.Range(start, label.Length).All(i => labels[i] == ' ');
    }

    private static string TrimLabelToWidth(string label, int width)
    {
        return label.Length <= width
            ? label
            : label[..width];
    }

    private int MapX(Rational x, AxisLimits axisLimits)
    {
        return MapX((double)x, axisLimits);
    }

    private int MapX(double x, AxisLimits axisLimits)
    {
        var lower = (double)axisLimits.XLimit.Lower;
        var upper = (double)axisLimits.XLimit.Upper;
        if (upper == lower)
            return 0;

        return (int)Math.Round((x - lower) / (upper - lower) * (Settings.Width - 1));
    }

    private int MapY(Rational y, AxisLimits axisLimits)
    {
        return MapY((double)y, axisLimits);
    }

    private int MapY(double y, AxisLimits axisLimits)
    {
        var lower = (double)axisLimits.YLimit.Lower;
        var upper = (double)axisLimits.YLimit.Upper;
        if (upper == lower)
            return Settings.Height - 1;

        return Settings.Height - 1 - (int)Math.Round((y - lower) / (upper - lower) * (Settings.Height - 1));
    }

    private static Rational ValueAtClosed(Segment segment, Rational time)
    {
        return segment.RightLimitAtStartTime + (time - segment.StartTime) * segment.Slope;
    }

    private static char GetLineCharacter(int x0, int y0, int x1, int y1)
    {
        if (x0 == x1)
            return '|';
        if (y0 == y1)
            return '-';

        return Math.Sign(x1 - x0) == Math.Sign(y1 - y0) ? '\\' : '/';
    }

    private static string ToMarkup(Cell cell)
    {
        if (cell.IsEmpty)
            return " ";

        var character = Markup.Escape(cell.Character.ToString());
        return cell.Style is null
            ? character
            : $"[{cell.Style}]{character}[/]";
    }

    private static string Format(Rational value, TerminalPlotTickLabelStyle tickLabelStyle)
    {
        if (!value.IsFinite)
            return value.ToString();

        var decimalLabel = FormatDecimal((double)value);
        if (tickLabelStyle == TerminalPlotTickLabelStyle.Decimal || value.IsInteger)
            return decimalLabel;

        var rationalLabel = value.ToString();
        return IsCompactRationalLabel(rationalLabel, decimalLabel)
            ? rationalLabel
            : decimalLabel;
    }

    private static bool IsCompactRationalLabel(string rationalLabel, string decimalLabel)
    {
        var decimalPointIndex = decimalLabel.IndexOf('.');
        return decimalPointIndex >= 0 &&
               decimalLabel.Length - decimalPointIndex - 1 >= 3 &&
               rationalLabel.Length <= decimalLabel.Length;
    }

    private static string FormatDecimal(double value)
    {
        return Math.Abs(value) < 0.0005
            ? "0"
            : value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static AxisLimits GetSequenceAxisLimits(
        IReadOnlyCollection<Sequence> sequences,
        TerminalPlotSettings settings)
    {
        if (sequences.Count == 0)
            throw new ArgumentException("Empty sequence collection.", nameof(sequences));

        var xLimit = GetFiniteLimit(settings.XLimit) ?? ApplyRelativeMargin(
            GetDefaultSequenceXLimit(sequences),
            settings.RelativeXAxisMargin);
        var yLimit = GetFiniteLimit(settings.YLimit) ?? ApplyRelativeMargin(
            GetDefaultSequenceYLimit(sequences),
            settings.RelativeYAxisMargin);

        return new AxisLimits(xLimit, yLimit);
    }

    private static Interval? GetFiniteLimit(Interval? limit)
    {
        return limit is { Lower.IsFinite: true, Upper.IsFinite: true }
            ? limit
            : null;
    }

    private static Interval ApplyRelativeMargin(Interval limit, double relativeMargin)
    {
        if (relativeMargin < 0)
            throw new ArgumentOutOfRangeException(
                nameof(relativeMargin),
                relativeMargin,
                "Relative axis margins cannot be negative.");

        if (relativeMargin == 0 || !limit.Lower.IsFinite || !limit.Upper.IsFinite)
            return limit;

        var length = limit.Upper - limit.Lower;
        var adjustment = length > 0
            ? length * (Rational)(decimal)relativeMargin
            : Rational.One;

        return new Interval(
            limit.Lower - adjustment,
            limit.Upper + adjustment,
            isLowerIncluded: true,
            isUpperIncluded: true);
    }

    private static Interval GetDefaultSequenceXLimit(IEnumerable<Sequence> sequences)
    {
        var finiteXValues = sequences
            .SelectMany(s => new[] { s.DefinedFrom, s.DefinedUntil })
            .Where(x => x.IsFinite)
            .ToList();

        if (finiteXValues.Count == 0)
            throw new ArgumentException("Cannot compute x-axis limits: no finite x values were found.");

        return new Interval(
            finiteXValues.Min(),
            finiteXValues.Max(),
            isLowerIncluded: true,
            isUpperIncluded: true);
    }

    private static Interval GetDefaultSequenceYLimit(IEnumerable<Sequence> sequences)
    {
        var finiteYValues = sequences
            .SelectMany(sequence => sequence.Elements.SelectMany(GetElementBoundaryValues))
            .Where(y => y.IsFinite)
            .ToList();

        if (finiteYValues.Count == 0)
            throw new ArgumentException("Cannot compute y-axis limits: no finite y values were found.");

        return new Interval(
            finiteYValues.Min(),
            finiteYValues.Max(),
            isLowerIncluded: true,
            isUpperIncluded: true);
    }

    private static IEnumerable<Rational> GetElementBoundaryValues(Element element)
    {
        if (element is Segment segment)
        {
            yield return segment.RightLimitAtStartTime;
            yield return segment.LeftLimitAtEndTime;
        }
        else if (element is Point point)
        {
            yield return point.Value;
        }
    }

    private static IEnumerable<Rational> GetElementBoundaryTimes(Element element)
    {
        if (element is Segment segment)
        {
            yield return segment.StartTime;
            yield return segment.EndTime;
        }
        else if (element is Point point)
        {
            yield return point.Time;
        }
    }

    private static AnsiSupport ToAnsiSupport(TerminalPlotAnsiMode ansiMode)
    {
        return ansiMode switch
        {
            TerminalPlotAnsiMode.Ansi => AnsiSupport.Yes,
            TerminalPlotAnsiMode.PlainText => AnsiSupport.No,
            _ => AnsiSupport.Detect
        };
    }

    private static ColorSystemSupport ToColorSystemSupport(TerminalPlotAnsiMode ansiMode)
    {
        return ansiMode switch
        {
            TerminalPlotAnsiMode.Ansi => ColorSystemSupport.TrueColor,
            TerminalPlotAnsiMode.PlainText => ColorSystemSupport.NoColors,
            _ => ColorSystemSupport.Detect
        };
    }

    private readonly record struct AxisLimits(Interval XLimit, Interval YLimit);

    private readonly record struct Cell(char Character, string? Style, int Priority)
    {
        public bool IsEmpty => Character == default;
    }

    private readonly record struct TickLabel(Rational Value, string Label);
}

/// <summary>
/// A sequence with display metadata for terminal plotting.
/// </summary>
/// <param name="Sequence">Sequence to plot.</param>
/// <param name="Name">Display name.</param>
/// <param name="Color">Spectre.Console markup color.</param>
public sealed record TerminalSequenceToPlot(Sequence Sequence, string Name, string Color);
