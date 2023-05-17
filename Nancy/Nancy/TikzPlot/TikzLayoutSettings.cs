namespace Unipi.Nancy.TikzPlot;

/// Settings for the plot layout.
public record TikzLayoutSettings
{
    /// Controls, if the legend is included in the plot, where it should be placed.
    public LegendPosition LegendPosition { get; set; } = LegendPosition.SouthEast;
    
    /// Controls whether the legend is included or not in the plot.
    public LegendStrategy LegendStrategy { get; set; } = LegendStrategy.Auto;

    /// Controls the fontsize used for the plot.
    public FontSize FontSize { get; set; } = FontSize.small;

    /// Controls the grid and ticks layout.
    public GridTickLayout GridTickLayout { get; set; } = GridTickLayout.Auto;

    /// Controls curve layout.
    public CurveLayout CurveLayout { get; set; } = CurveLayout.SimplifyContinuous;
}

/// Options to control whether the legend is included or not in the plot.
public enum LegendStrategy
{
    /// Decides automatically if to show the legend or not.
    /// By default, the legend is omitted if a single curve or sequence is being plotted,
    /// and the name was not specified manually.
    Auto, 
    
    /// Force the legend to be included in the plot.
    ForceEnable, 
    
    /// Force the legend to be omitted from the plot.
    ForceDisable
}

/// Options for legend position.
public enum LegendPosition
{
    /// Corresponds, in LaTeX, to "north".
    North,

    /// Corresponds, in LaTeX, to "north east".
    NorthEast,

    /// Corresponds, in LaTeX, to "east".
    East,

    /// Corresponds, in LaTeX, to "south east".
    SouthEast,

    /// Corresponds, in LaTeX, to "south".
    South,

    /// Corresponds, in LaTeX, to "south west".
    SouthWest,

    /// Corresponds, in LaTeX, to "west".
    West,

    /// Corresponds, in LaTeX, to "north west".
    NorthWest,
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
/// Options for plot font size.
/// </summary>
public enum FontSize {
    // ReSharper disable IdentifierTypo
    // ReSharper disable CommentTypo
    
    /// Corresponds, in LaTeX, to \tiny
    tiny,
    
    /// Corresponds, in LaTeX, to \scriptsize
    scriptsize,
    
    /// Corresponds, in LaTeX, to \footnotesize
    footnotesize,
    
    /// Corresponds, in LaTeX, to \small
    small,
    
    /// Corresponds, in LaTeX, to \normalsize
    normalsize,
    
    /// Corresponds, in LaTeX, to \large
    large,
    
    /// Corresponds, in LaTeX, to \Large
    Large,
    
    /// Corresponds, in LaTeX, to \LARGE
    LARGE,
    
    /// Corresponds, in LaTeX, to \huge
    huge,
    
    /// Corresponds, in LaTeX, to \Huge
    Huge
    
    // ReSharper restore CommentTypo
    // ReSharper restore IdentifierTypo
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
    /// and the grid layout is left to TikZ's automatic algorithms 
    Auto,
    
    /// The grid marks all natural numbers, but the ticks are not labelled
    SquareGridNoLabels,
    
    /// The ticks and grid mark all natural numbers
    SquareGrid
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