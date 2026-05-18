namespace Unipi.Nancy.Plots.Terminal;

/// <summary>
/// Settings controlling terminal plot rendering.
/// </summary>
public record TerminalPlotSettings : PlotSettings
{
    /// <summary>
    /// Width of the plot area in terminal cells.
    /// </summary>
    public int Width { get; set; } = 72;

    /// <summary>
    /// Height of the plot area in terminal cells.
    /// </summary>
    public int Height { get; set; } = 20;

    /// <summary>
    /// Controls whether Spectre.Console emits ANSI escape sequences.
    /// </summary>
    public TerminalPlotAnsiMode AnsiMode { get; set; } = TerminalPlotAnsiMode.Auto;

    /// <summary>
    /// Width reserved for y-axis tick labels.
    /// </summary>
    public int YAxisLabelWidth { get; set; } = 9;

    /// <summary>
    /// Target number of x-axis tick labels to draw, including the endpoints.
    /// </summary>
    public int XAxisTickCount { get; set; } = 5;

    /// <summary>
    /// Strategy used to choose x-axis tick label values.
    /// </summary>
    public TerminalPlotTickStrategy XAxisTickStrategy { get; set; } = TerminalPlotTickStrategy.PreferBreakpoints;

    /// <summary>
    /// Target number of y-axis tick labels to draw, including the endpoints.
    /// </summary>
    public int YAxisTickCount { get; set; } = 5;

    /// <summary>
    /// Strategy used to choose y-axis tick label values.
    /// </summary>
    public TerminalPlotTickStrategy YAxisTickStrategy { get; set; } = TerminalPlotTickStrategy.PreferBreakpoints;

    /// <summary>
    /// Style used to format axis tick labels.
    /// </summary>
    public TerminalPlotTickLabelStyle TickLabelStyle { get; set; } = TerminalPlotTickLabelStyle.RationalWhenCompact;

    /// <summary>
    /// Character used for sampled points.
    /// </summary>
    public char PointCharacter { get; set; } = '*';

    /// <summary>
    /// Character used when multiple traces share the same terminal cell.
    /// </summary>
    public char CollisionCharacter { get; set; } = '#';

    /// <summary>
    /// Character used for discontinuity markers.
    /// </summary>
    public char DiscontinuityCharacter { get; set; } = 'o';

    /// <summary>
    /// If true, draw the x and y axes when their zero coordinate is visible.
    /// </summary>
    public bool DrawAxes { get; set; } = true;

    /// <summary>
    /// If true, include axis range and label information below the plot.
    /// </summary>
    public bool DrawAxisLabels { get; set; } = true;
}
