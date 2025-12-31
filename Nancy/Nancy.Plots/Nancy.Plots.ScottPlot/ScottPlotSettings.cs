namespace Unipi.Nancy.Plots.ScottPlot;

public record ScottPlotSettings : PlotSettings
{
    /// Controls the fontsize used for the plot.
    public FontSize FontSize { get; set; } = FontSize.small;

    /// Controls the grid and ticks layout.
    public GridTickLayout GridTickLayout { get; set; } = GridTickLayout.Auto;

    /// Controls curve layout.
    public CurveLayout CurveLayout { get; set; } = CurveLayout.SimplifyContinuous;
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
    Auto,
    
    /// The grid marks all natural numbers, but the ticks are not labelled.
    SquareGridNoLabels,
    
    /// The ticks and grid mark all natural numbers.
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