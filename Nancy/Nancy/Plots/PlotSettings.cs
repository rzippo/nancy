using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Plots;

/// <summary>
/// Settings for a plot of Nancy curves.
/// </summary>
/// <remarks>
/// The base class contains settings that are meaningful in all contexts,
/// But some may not be supported by all plot implementations.
/// </remarks>
public record PlotSettings
{
    /// <summary>
    /// The plot title.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Range for the x-axis.
    /// </summary>
    public Interval? XLimit { get; set; } = null;

    /// <summary>
    /// Range for the y-axis.
    /// </summary>
    public Interval? YLimit { get; set; } = null;
    
    /// <summary>
    /// Label for the x-axis.
    /// </summary>
    public string XLabel { get; set; } = string.Empty;

    /// <summary>
    /// Label for the y-axis.
    /// </summary>
    public string YLabel { get; set; } = string.Empty;
    
    /// Controls whether the legend is included or not in the plot.
    public LegendStrategy LegendStrategy { get; set; } = LegendStrategy.Auto;

    /// If the legend is included in the plot, controls where it should be placed.
    public LegendPosition LegendPosition { get; set; } = LegendPosition.SouthEast;
}

/// Options for legend position.
public enum LegendPosition
{
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West,
    NorthWest,
}

/// Options to control whether the legend is included or not in the plot.
public enum LegendStrategy
{
    /// Decides automatically if to show the legend or not.
    /// By default, the legend is omitted if a single curve or sequence is being plotted,
    /// and its name was not specified.
    Auto, 
    
    /// Force the legend to be included in the plot.
    ForceEnable, 
    
    /// Force the legend to be omitted from the plot.
    ForceDisable
}