namespace Unipi.Nancy.Plots.XPlot.Plotly;

public record XPlotPlotSettings : PlotSettings
{
    /// If true, the plot is forced to have the x and y axes to have the same scale.
    /// If false, they are adapted to the render size.
    public bool SameScaleAxes { get; set; } = false;

    public int Width { get; set; } = 1200;
    public int Height { get; set; } = 800;
    public double ScaleFactor { get; set; } = 1.5;
}