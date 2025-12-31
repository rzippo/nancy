namespace Unipi.Nancy.Plots;

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