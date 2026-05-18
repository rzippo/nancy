namespace Unipi.Nancy.Plots.Terminal;

/// <summary>
/// Style used to format terminal plot axis tick labels.
/// </summary>
public enum TerminalPlotTickLabelStyle
{
    /// <summary>
    /// Format tick labels as rounded decimal values.
    /// </summary>
    Decimal,

    /// <summary>
    /// Prefer exact rational labels when they are compact, otherwise use decimals.
    /// </summary>
    RationalWhenCompact
}
