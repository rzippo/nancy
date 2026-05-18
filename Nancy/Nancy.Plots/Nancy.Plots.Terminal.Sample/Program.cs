using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Unipi.Nancy.Plots.Terminal;

var app = new CommandApp<SampleCommand>();
app.Configure(config =>
{
    config.SetApplicationName("nancy-terminal-sample");
    config.SetApplicationVersion("1.0");
    config.AddExample(Array.Empty<string>());
    config.AddExample(["--example", "service"]);
    config.AddExample(["--example", "steps", "--plain"]);
    config.AddExample(["--example", "screen-fit"]);
    config.AddExample(["--example", "sequence", "--ticks", "even"]);
    config.AddExample(["--example", "screen-fit", "--tick-labels", "decimal"]);
    config.AddExample(["--width", "100", "--height", "24"]);
});

return app.Run(args);

internal sealed class SampleCommand : Command<SampleSettings>
{
    protected override int Execute(CommandContext context, SampleSettings settings, CancellationToken cancellationToken)
    {
        var examples = GetExamples();

        if (settings.List)
        {
            WriteExamples(examples);
            return 0;
        }

        var selectedExamples = SelectExamples(examples, settings.Example).ToList();
        if (selectedExamples.Count == 0)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Unknown example:[/] {settings.Example}");
            WriteExamples(examples);
            return 1;
        }

        var terminalSize = DetectTerminalSize();
        var ansiMode = GetAnsiMode(settings);
        var renderAll = selectedExamples.Count > 1;

        foreach (var example in selectedExamples)
        {
            if (renderAll)
            {
                AnsiConsole.Write(new Rule(Markup.Escape(example.Name)).LeftJustified());
            }

            var plotSettings = CreatePlotSettings(settings, example, terminalSize, selectedExamples.Count, ansiMode);
            RenderExample(example, plotSettings);

            if (renderAll)
                AnsiConsole.WriteLine();
        }

        return 0;
    }

    private static IReadOnlyList<PlotExample> GetExamples()
    {
        return
        [
            new PlotExample(
                Name: "service",
                Description: "Single rate-latency service curve.",
                Curves: [new RateLatencyServiceCurve(rate: 1, latency: 3)],
                Sequences: null,
                Names: ["service"],
                Configure: settings =>
                {
                    settings.XLimit = new Interval(0, 10);
                    settings.YLimit = new Interval(0, 8);
                }),

            new PlotExample(
                Name: "arrival-service",
                Description: "Arrival and service curves on shared axes.",
                Curves: [
                    new SigmaRhoArrivalCurve(sigma: 2, rho: 2),
                    new RateLatencyServiceCurve(rate: 3, latency: 1)
                ],
                Sequences: null,
                Names: ["arrival", "service"],
                Configure: settings =>
                {
                    settings.XLimit = new Interval(0, 10);
                    settings.YLimit = new Interval(0, 30);
                }),

            new PlotExample(
                Name: "steps",
                Description: "Step curves with visible discontinuity markers.",
                Curves: [
                    new StepCurve(value: 5, stepTime: 2),
                    new StepCurve(value: 9, stepTime: 5)
                ],
                Sequences: null,
                Names: ["step-1", "step-2"],
                Configure: settings =>
                {
                    settings.XLimit = new Interval(0, 10);
                    settings.YLimit = new Interval(0, 10);
                }),

            new PlotExample(
                Name: "sequence",
                Description: "A manually built piecewise sequence.",
                Curves: null,
                Sequences: [
                    new Sequence([
                        Point.Origin(),
                        new Segment(startTime: 0, endTime: 2, rightLimitAtStartTime: 1, slope: 1),
                        new Point(time: 2, value: 5),
                        new Segment(startTime: 2, endTime: 5, rightLimitAtStartTime: 3, slope: 0),
                        new Point(time: 5, value: 3),
                        new Segment(startTime: 5, endTime: 8, rightLimitAtStartTime: 3, slope: 2)
                    ])
                ],
                Names: ["sample"],
                Configure: settings =>
                {
                    settings.XLimit = new Interval(0, 8);
                    settings.YLimit = new Interval(0, 10);
                }),

            new PlotExample(
                Name: "screen-fit",
                Description: "Arrival and service curves sized to the available terminal space.",
                Curves: [
                    new SigmaRhoArrivalCurve(sigma: 2, rho: 2),
                    new RateLatencyServiceCurve(rate: 3, latency: 1)
                ],
                Sequences: null,
                Names: ["arrival", "service"],
                Configure: settings =>
                {
                    settings.XLimit = new Interval(0, 10);
                    settings.YLimit = new Interval(0, 30);
                    settings.XAxisTickCount = 7;
                    settings.YAxisTickCount = 7;
                },
                FitAvailableTerminalSpace: true,
                IncludeInAll: false)
        ];
    }

    private static IEnumerable<PlotExample> SelectExamples(IEnumerable<PlotExample> examples, string exampleName)
    {
        if (string.Equals(exampleName, "all", StringComparison.OrdinalIgnoreCase))
            return examples.Where(example => example.IncludeInAll);

        return examples.Where(example =>
            string.Equals(example.Name, exampleName, StringComparison.OrdinalIgnoreCase));
    }

    private static void WriteExamples(IEnumerable<PlotExample> examples)
    {
        var table = new Table()
            .AddColumn("Example")
            .AddColumn("Description");

        foreach (var example in examples)
            table.AddRow(Markup.Escape(example.Name), Markup.Escape(example.Description));

        AnsiConsole.Write(table);
    }

    private static TerminalPlotSettings CreatePlotSettings(
        SampleSettings settings,
        PlotExample example,
        TerminalSize terminalSize,
        int exampleCount,
        TerminalPlotAnsiMode ansiMode)
    {
        const int yAxisLabelWidth = 9;

        var detectedPlotWidth = terminalSize.Width - yAxisLabelWidth - 2;
        var detectedPlotHeight = exampleCount == 1
            ? terminalSize.Height - 8
            : (terminalSize.Height - 8) / exampleCount;

        var plotSettings = new TerminalPlotSettings
        {
            AnsiMode = ansiMode,
            DrawAxisLabels = true,
            Height = settings.Height ?? GetPlotHeight(example, detectedPlotHeight),
            LegendStrategy = LegendStrategy.ForceEnable,
            Title = example.Description,
            Width = settings.Width ?? GetPlotWidth(example, detectedPlotWidth),
            XLabel = "time",
            YAxisLabelWidth = yAxisLabelWidth,
            YLabel = "data"
        };

        example.Configure(plotSettings);
        plotSettings.XAxisTickStrategy = settings.TickStrategy;
        plotSettings.YAxisTickStrategy = settings.TickStrategy;
        plotSettings.TickLabelStyle = settings.TickLabelStyle;
        return plotSettings;
    }

    private static int GetPlotWidth(PlotExample example, int detectedPlotWidth)
    {
        return example.FitAvailableTerminalSpace
            ? Math.Max(32, detectedPlotWidth)
            : Math.Clamp(detectedPlotWidth, 32, 140);
    }

    private static int GetPlotHeight(PlotExample example, int detectedPlotHeight)
    {
        return example.FitAvailableTerminalSpace
            ? Math.Max(8, detectedPlotHeight)
            : Math.Clamp(detectedPlotHeight, 8, 26);
    }

    private static void RenderExample(PlotExample example, TerminalPlotSettings settings)
    {
        var renderer = new TerminalNancyPlotRenderer { PlotSettings = settings };

        if (settings.AnsiMode == TerminalPlotAnsiMode.Auto)
        {
            var plot = example.Curves is not null
                ? renderer.GetDefaultModeler().GetPlot(example.Curves, example.Names)
                : renderer.GetDefaultModeler().GetPlot(example.Sequences!, example.Names);
            renderer.WriteToConsole(plot, AnsiConsole.Console);
        }
        else
        {
            var rendered = example.Curves is not null
                ? renderer.Plot(example.Curves, example.Names)
                : renderer.Plot(example.Sequences!, example.Names);
            Console.Write(rendered);
        }
    }

    private static TerminalSize DetectTerminalSize()
    {
        var width = AnsiConsole.Console.Profile.Width;
        var height = AnsiConsole.Console.Profile.Height;

        try
        {
            if (width <= 0 && !Console.IsOutputRedirected)
                width = Console.WindowWidth;
            if (height <= 0 && !Console.IsOutputRedirected)
                height = Console.WindowHeight;
        }
        catch
        {
            // Some redirected or hosted consoles throw when probing WindowWidth/WindowHeight.
        }

        return new TerminalSize(
            Width: width > 0 ? width : 100,
            Height: height > 0 ? height : 30);
    }

    private static TerminalPlotAnsiMode GetAnsiMode(SampleSettings settings)
    {
        if (settings.Ansi)
            return TerminalPlotAnsiMode.Ansi;
        if (settings.Plain)
            return TerminalPlotAnsiMode.PlainText;

        return TerminalPlotAnsiMode.Auto;
    }

}

internal sealed class SampleSettings : CommandSettings
{
    [Description("Example to render: all, service, arrival-service, steps, sequence, screen-fit.")]
    [CommandOption("-e|--example <NAME>")]
    public string Example { get; init; } = "all";

    [Description("List available examples.")]
    [CommandOption("-l|--list")]
    public bool List { get; init; }

    [Description("Force ANSI color escape sequences in the rendered plot string.")]
    [CommandOption("--ansi")]
    public bool Ansi { get; init; }

    [Description("Force plain text output.")]
    [CommandOption("--plain")]
    public bool Plain { get; init; }

    [Description("Override detected plot width in terminal cells.")]
    [CommandOption("-w|--width <WIDTH>")]
    public int? Width { get; init; }

    [Description("Override detected plot height in terminal rows.")]
    [CommandOption("--height <HEIGHT>")]
    public int? Height { get; init; }

    [Description("Tick strategy: breakpoints or even.")]
    [CommandOption("--ticks <STRATEGY>")]
    public string Ticks { get; init; } = "breakpoints";

    [Description("Tick label style: rational or decimal.")]
    [CommandOption("--tick-labels <STYLE>")]
    public string TickLabels { get; init; } = "rational";

    public TerminalPlotTickStrategy TickStrategy => ParseTickStrategy(Ticks);

    public TerminalPlotTickLabelStyle TickLabelStyle => ParseTickLabelStyle(TickLabels);

    private static TerminalPlotTickStrategy ParseTickStrategy(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "breakpoints" => TerminalPlotTickStrategy.PreferBreakpoints,
            "even" => TerminalPlotTickStrategy.EvenlySpaced,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported tick strategy.")
        };
    }

    private static TerminalPlotTickLabelStyle ParseTickLabelStyle(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "rational" => TerminalPlotTickLabelStyle.RationalWhenCompact,
            "decimal" => TerminalPlotTickLabelStyle.Decimal,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported tick label style.")
        };
    }

    public override ValidationResult Validate()
    {
        if (Ansi && Plain)
            return ValidationResult.Error("Use only one of --ansi or --plain.");
        if (Width is < 10)
            return ValidationResult.Error("--width must be at least 10.");
        if (Height is < 5)
            return ValidationResult.Error("--height must be at least 5.");
        if (!string.Equals(Ticks, "breakpoints", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(Ticks, "even", StringComparison.OrdinalIgnoreCase))
            return ValidationResult.Error("--ticks must be either 'breakpoints' or 'even'.");
        if (!string.Equals(TickLabels, "rational", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(TickLabels, "decimal", StringComparison.OrdinalIgnoreCase))
            return ValidationResult.Error("--tick-labels must be either 'rational' or 'decimal'.");

        return ValidationResult.Success();
    }
}

internal sealed record PlotExample(
    string Name,
    string Description,
    IReadOnlyCollection<Curve>? Curves,
    IReadOnlyCollection<Sequence>? Sequences,
    IReadOnlyCollection<string> Names,
    Action<TerminalPlotSettings> Configure,
    bool FitAvailableTerminalSpace = false,
    bool IncludeInAll = true);

internal readonly record struct TerminalSize(int Width, int Height);
