namespace Unipi.Nancy.Plots;

/// <summary>
/// Specifies the position of a legend on a plot.
/// </summary>
public enum LegendPosition
{
    /// <summary>
    /// Legend positioned at the top center of the plot.
    /// </summary>
    North,
    
    /// <summary>
    /// Legend positioned at the top right corner of the plot.
    /// </summary>
    NorthEast,
    
    /// <summary>
    /// Legend positioned on the right side of the plot.
    /// </summary>
    East,
    
    /// <summary>
    /// Legend positioned at the bottom right corner of the plot.
    /// </summary>
    SouthEast,
    
    /// <summary>
    /// Legend positioned at the bottom center of the plot.
    /// </summary>
    South,
    
    /// <summary>
    /// Legend positioned at the bottom left corner of the plot.
    /// </summary>
    SouthWest,
    
    /// <summary>
    /// Legend positioned on the left side of the plot.
    /// </summary>
    West,
    
    /// <summary>
    /// Legend positioned at the top left corner of the plot.
    /// </summary>
    NorthWest,
}