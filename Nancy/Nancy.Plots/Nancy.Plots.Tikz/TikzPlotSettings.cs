namespace Unipi.Nancy.Plots.Tikz;

/// <summary>
/// Settings controlling TikZ plot rendering.
/// </summary>
public record TikzPlotSettings : PlotSettings
{
    /// <summary>
    /// The line styles to cycle, written as TikZ option strings.
    /// </summary>
    /// <remarks>
    /// Takes precedence over <see cref="PlotSettings.LineStyles"/> when set, and accepts any TikZ style, such as <c>densely dash dot dot</c>.
    /// </remarks>
    public IReadOnlyList<string>? RawLineStyles { get; set; } = null;

    /// Controls the fontsize used for the plot.
    public FontSize FontSize { get; set; } = FontSize.small;

    /// Controls the grid and ticks layout.
    public GridTickLayout GridTickLayout { get; set; } = GridTickLayout.RoundValues;

    /// Controls curve layout.
    public CurveLayout CurveLayout { get; set; } = CurveLayout.SimplifyContinuous;

    /// <summary>
    /// Controls whether the plotted curves are clipped to the axis limits.
    /// </summary>
    /// <remarks>
    /// <see cref="ClipStrategy.ToLimits"/> by default, so that a curve leaving the axis limits is cut at the frame rather than drawn past it.
    /// <see cref="ClipStrategy.Off"/> reproduces the legacy behavior, which keeps marks drawn exactly on the frame whole, at the cost of curves escaping the plot.
    /// </remarks>
    public ClipStrategy ClipStrategy { get; set; } = ClipStrategy.ToLimits;
}

/// <summary>
/// Extension class for <see cref="LegendPosition"/>.
/// </summary>
/// <exclude />
public static class LegendPositionExtension
{
    /// Returns the LaTeX string corresponding to the position.
    public static string ToLatex(this LegendPosition position)
    {
        switch (position)
        {
            case LegendPosition.North: 
                return "north";
            case LegendPosition.NorthEast: 
                return "north east";
            case LegendPosition.East: 
                return "east";
            case LegendPosition.SouthEast: 
                return "south east";
            case LegendPosition.South: 
                return "south";
            case LegendPosition.SouthWest: 
                return "south west";
            case LegendPosition.West: 
                return "west";
            case LegendPosition.NorthWest: 
                return "north west";

            default:
                return "south east";
        }
    }
}


/// <summary>
/// Extension class placing the legend outside the axis box.
/// </summary>
/// <exclude />
public static class LegendPlacementExtension
{
    /// <summary>
    /// Returns the pgfplots anchor and position for a legend placed beside the axis.
    /// </summary>
    /// <remarks>
    /// The coordinates are relative to the axis box, so a legend anchored past 1 or below 0 sits outside it whatever its size.
    /// </remarks>
    public static (string At, string Anchor) ToOutsideLatex(this LegendPosition position)
        => position switch
        {
            LegendPosition.North => ("0.5,1.05", "south"),
            LegendPosition.NorthEast => ("1.05,1", "north west"),
            LegendPosition.East => ("1.05,0.5", "west"),
            LegendPosition.SouthEast => ("1.05,0", "south west"),
            LegendPosition.South => ("0.5,-0.15", "north"),
            LegendPosition.SouthWest => ("-0.05,0", "south east"),
            LegendPosition.West => ("-0.05,0.5", "east"),
            LegendPosition.NorthWest => ("-0.05,1", "north east"),
            _ => ("1.05,0", "south west")
        };
}

/// <summary>
/// Options for plot font size.
/// </summary>
public enum FontSize {
    // ReSharper disable IdentifierTypo
    // ReSharper disable CommentTypo
    // ReSharper disable InconsistentNaming
    
    /// Corresponds, in LaTeX, to \tiny .
    tiny,
    
    /// Corresponds, in LaTeX, to \scriptsize .
    scriptsize,
    
    /// Corresponds, in LaTeX, to \footnotesize .
    footnotesize,
    
    /// Corresponds, in LaTeX, to \small .
    small,
    
    /// Corresponds, in LaTeX, to \normalsize .
    normalsize,
    
    /// Corresponds, in LaTeX, to \large .
    large,
    
    /// Corresponds, in LaTeX, to \Large .
    Large,
    
    /// Corresponds, in LaTeX, to \LARGE .
    LARGE,
    
    /// Corresponds, in LaTeX, to \huge .
    huge,
    
    /// Corresponds, in LaTeX, to \Huge .
    Huge
    
    // ReSharper restore CommentTypo
    // ReSharper restore IdentifierTypo
    // ReSharper restore InconsistentNaming
}

/// <summary>
/// Extension class for <see cref="FontSize"/>.
/// </summary>
/// <exclude />
public static class FontSizeExtensions 
{
    /// Returns the LaTeX string corresponding to the fontsize.
    public static string ToLatex(this FontSize size)
    {
        switch(size)
        {
            // ReSharper disable StringLiteralTypo
            case(FontSize.tiny):
                return "\\tiny";
            case(FontSize.scriptsize):
                return "\\scriptsize";
            case(FontSize.footnotesize):
                return "\\footnotesize";
            case(FontSize.small):
                return "\\small";
            case(FontSize.normalsize):
                return "\\normalsize";
            case(FontSize.large):
                return "\\large";
            case(FontSize.Large):
                return "\\Large";
            case(FontSize.LARGE):
                return "\\LARGE";
            case(FontSize.huge):
                return "\\huge";
            case(FontSize.Huge):            
                return "\\Huge";

            default:
                return "\\small";
            // ReSharper restore StringLiteralTypo
        }
    }
}

/// Options for grid and ticks layout.
public enum GridTickLayout
{
    /// The tick marks are set to the breakpoints of the curves plotted,
    /// and the grid layout is left to TikZ's automatic algorithms.
    /// Every breakpoint is labelled, which reads well for few curves and crowds the axes for many.
    Breakpoints,
    
    /// The grid marks all natural numbers, but the ticks are not labelled.
    SquareGridNoLabels,
    
    /// The ticks and grid mark all natural numbers.
    SquareGrid,

    /// The tick marks are placed at regularly spaced round values, chosen by pgfplots.
    /// It keeps the axes readable however many curves are plotted.
    RoundValues
}

/// Options for curve layout.
public enum CurveLayout
{
    /// Continuous sequences are plotted as single uninterrupted lines.
    /// The period start is always highlighted mark.
    SimplifyContinuous,
    
    /// Continuous sequences are plotted as single uninterrupted lines.
    /// All breakpoints are highlighted with a mark.
    SimplifyContinuousWithMarks,
    
    /// All segments and points are plotted separately.
    /// Best to highlight the objects that compose the curve or sequence. 
    SplitAllElements
}

/// Options for how the plotted curves are clipped to the axis limits.
public enum ClipStrategy
{
    /// Curves are clipped to the axis box, so that they do not run off the plot.
    /// Marks and labels sitting exactly on the frame are cut in half.
    ToLimits,

    /// Curves are not clipped, and are drawn whole even past the axis box.
    /// Keeps marks on the frame whole, at the cost of curves escaping the plot.
    Off
}
