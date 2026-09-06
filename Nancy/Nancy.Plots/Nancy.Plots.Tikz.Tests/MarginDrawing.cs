using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Unipi.Nancy.Plots.Tikz;
using Xunit;

namespace Nancy.Plots.Tikz.Tests;

/// <summary>
/// What is drawn between the requested x-limit and the frame.
/// </summary>
/// <remarks>
/// A plot frames past the data limit, and the strip between the two has to show the curve rather than a guess at it.
/// These assert the drawn coordinates against the curve itself, which is the check nothing made before:
/// every earlier test pinned the limits, and a line drawn to the wrong place inside them passed them all.
/// The three cases that must not move are as much the point as the two that must: the margin is not to be fixed by dropping it,
/// and a sequence given directly must keep ending where it ends.
/// </remarks>
public class MarginDrawing
{
    /// <summary>
    /// Rate 1 up to $t = 1$, rate 20 after, so a line projected from the first rate cannot land on the second by accident.
    /// </summary>
    private static Curve RateChangeAtOne() => new Curve(
        baseSequence: new Sequence([
            Point.Origin(),
            new Segment(0, 1, 0, 1),
            new Point(1, 1),
            new Segment(1, 2, 1, 20)
        ]),
        pseudoPeriodStart: 1,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: 20
    );

    /// <summary>
    /// Rises at rate 2 over the first half of each period, then holds.
    /// </summary>
    private static Curve RiseThenFlat() => new Curve(
        baseSequence: new Sequence([
            Point.Origin(),
            new Segment(0, new Rational(1, 2), 0, 2),
            new Point(new Rational(1, 2), 1),
            Segment.Constant(new Rational(1, 2), 1, 1)
        ]),
        pseudoPeriodStart: 0,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: 1
    );

    private static TikzPlotSettings Until(Rational upper)
        => new() { XLimit = new Interval(0, upper) };

    #region A curve is drawn from the curve

    /// <summary>
    /// Asserts every drawn coordinate against the function, which is what the margin was getting wrong.
    /// The curves used here take values that a decimal holds exactly, so the printed coordinate is the value and not a rounding of it,
    /// and are continuous, so the drawn value is the one <see cref="Curve.ValueAt"/> reports rather than a left limit.
    /// </summary>
    private static void AssertDrawnFromTheCurve(Curve curve, string code)
    {
        var drawn = FirstRunCoordinates(code);
        Assert.NotEmpty(drawn);
        foreach (var (x, y) in drawn)
            Assert.Equal((decimal)curve.ValueAt((Rational)x), y);
    }

    [Fact]
    public void ACurveIsDrawnFromTheCurveWhereTheLimitLandsOnABreakpoint()
    {
        var curve = RateChangeAtOne();

        var code = TikzPlots.ToTikzPlotCode(curve, "h", Until(1));

        AssertDrawnFromTheCurve(curve, code);
        // the frame is at 1.03, where the curve is 1.6; the old rate of 1 would put it at 1.03
        Assert.Equal((1.03m, 1.6m), FirstRunCoordinates(code)[^1]);
    }

    [Fact]
    public void ACurveIsDrawnFromTheCurveWhereTheLimitStartsAFlat()
    {
        var curve = RiseThenFlat();

        var code = TikzPlots.ToTikzPlotCode(curve, "f", Until(new Rational(1, 2)));

        AssertDrawnFromTheCurve(curve, code);
        Assert.Equal((0.515m, 1m), FirstRunCoordinates(code)[^1]);
    }

    /// <summary>
    /// The limit inside a segment rather than on its start, which was already right.
    /// A margin that stopped being drawn at all would pass the two above and fail this.
    /// </summary>
    [Fact]
    public void ACurveIsDrawnFromTheCurveWhereTheLimitFallsInsideASegment()
    {
        var curve = RiseThenFlat();

        var code = TikzPlots.ToTikzPlotCode(curve, "f", Until(new Rational(3, 5)));

        AssertDrawnFromTheCurve(curve, code);
        Assert.Equal((0.618m, 1m), FirstRunCoordinates(code)[^1]);
    }

    #endregion

    #region The frame is not an end

    /// <summary>
    /// The plot stops at the frame because the plot stops there, not because the curve does,
    /// so the end carries none of the marks that say a function ended.
    /// </summary>
    [Fact]
    public void ACurveIsNotMarkedWhereThePlotEnds()
    {
        var code = TikzPlots.ToTikzPlotCode(RateChangeAtOne(), "h", Until(1));

        // no endpoint dot at the frame
        Assert.DoesNotContain(1.03m, PointMarks(code).Select(p => p.x));
        // and none of the decorations for an open end
        var line = FirstDataPlotLine(code);
        Assert.DoesNotContain("shorten >", line);
        Assert.DoesNotContain("-(", line);
    }

    #endregion

    #region A sequence given directly ends where it ends

    /// <summary>
    /// There is no curve behind a sequence, so nothing is known past its end and the plot must not draw to the frame.
    /// A right-open end keeps saying so, with the bracket and the shortened line.
    /// </summary>
    [Fact]
    public void ARightOpenSequenceIsDrawnOnlyToItsOwnEnd()
    {
        var sequence = RateChangeAtOne().Cut(0, 1);
        Assert.True(sequence.IsRightOpen);

        var code = TikzPlots.ToTikzPlotCode(sequence, "s", Until(1));

        // the frame still reaches 1.03, and the sequence still stops at 1
        Assert.Contains("xmax = 1.03,", code);
        Assert.Equal((1m, 1m), FirstRunCoordinates(code)[^1]);
        Assert.DoesNotContain(1.03m, FirstRunCoordinates(code).Select(p => p.x));

        var line = FirstDataPlotLine(code);
        Assert.Contains("-(", line);
        Assert.Contains("shorten > = 1pt", line);
    }

    /// <summary>
    /// A right-closed end says so with a dot at its own last point, which is not the frame either.
    /// </summary>
    [Fact]
    public void ARightClosedSequenceIsMarkedAtItsOwnEnd()
    {
        var sequence = RiseThenFlat().Cut(0, new Rational(1, 2), isStartIncluded: true, isEndIncluded: true);
        Assert.False(sequence.IsRightOpen);

        var code = TikzPlots.ToTikzPlotCode(sequence, "s", Until(new Rational(1, 2)));

        Assert.Contains("xmax = 0.515,", code);
        Assert.Equal((0.5m, 1m), FirstRunCoordinates(code)[^1]);
        Assert.Contains((0.5m, 1m), PointMarks(code));
        Assert.DoesNotContain(0.515m, FirstRunCoordinates(code).Select(p => p.x));
    }

    #endregion

    #region What the margin can hold

    /// <summary>
    /// A margin is a strip of the function, not a straight line to its far corner:
    /// a staircase stepping inside it is drawn stepping, which no single carried point could have expressed.
    /// </summary>
    [Fact]
    public void TheMarginShowsAStepFallingInsideIt()
    {
        // steps of height 1 every half unit, from t = 1; the frame at 4.12 holds the step at t = 4
        var stair = new Curve(
            baseSequence: new Sequence([
                Point.Origin(),
                Segment.Zero(0, 1),
                new Point(1, 0),
                Segment.Constant(1, new Rational(3, 2), 1)
            ]),
            pseudoPeriodStart: 1,
            pseudoPeriodLength: new Rational(1, 2),
            pseudoPeriodHeight: 1
        );

        var code = TikzPlots.ToTikzPlotCode(stair, "s", Until(4));

        // the run before the step ends at its own last value, and is marked there
        Assert.Contains((4m, 6m), PointMarks(code));
        // and the step itself is drawn, rather than the flat being carried across the margin
        Assert.Contains("coordinates { (4, 7) (4.12, 7) }", code);
    }

    /// <summary>
    /// A finite run that stops before the frame stops because the curve does, here because it goes infinite,
    /// so it keeps the dot that says so.
    /// Only the run reaching the frame loses one.
    /// </summary>
    [Fact]
    public void AFiniteRunEndingBeforeTheFrameKeepsItsMark()
    {
        var code = TikzPlots.ToTikzPlotCode(new DelayServiceCurve(3), "d", Until(8));

        Assert.Contains((3m, 0m), PointMarks(code));
    }

    #endregion

    #region The margin is not part of the data

    /// <summary>
    /// Ticks are the reader's index of where the breakpoints are, and the sample at the frame is not one of them.
    /// </summary>
    [Fact]
    public void TheMarginAddsNoTicks()
    {
        var code = TikzPlots.ToTikzPlotCode(RiseThenFlat(), "f", new TikzPlotSettings
        {
            XLimit = new Interval(0, new Rational(1, 2)),
            GridTickLayout = GridTickLayout.Breakpoints
        });

        Assert.Contains("extra x ticks = { 0, 0.5 },", code);
        Assert.Contains("extra y ticks = { 0, 1 },", code);
    }

    /// <summary>
    /// An explicit limit below 0 is the window the reader asked for, and the frame is that window plus its margin.
    /// Curves are undefined below 0, so what can be sampled is narrower than what is framed; the frame must not shrink to the sampling.
    /// </summary>
    [Fact]
    public void AnExplicitNegativeLowerLimitKeepsItsFrame()
    {
        var code = TikzPlots.ToTikzPlotCode(
            [new RateLatencyServiceCurve(3, 1), new SigmaRhoArrivalCurve(2, 2)],
            ["sc", "ac"],
            new TikzPlotSettings
            {
                XLimit = new Interval(-1, 10),
                YLimit = new Interval(-2, 30)
            });

        Assert.Contains("xmin = -1.33,", code);
        Assert.Contains("xmax = 10.33,", code);
    }

    #endregion

    #region Infinities

    /// <summary>
    /// A delay curve is $+\infty$ from its delay on, and asking for exactly that limit used to cut the infinity away:
    /// the sampled part held no infinite element, so no area was reserved and a flat line was drawn across the margin,
    /// over a strip where the curve has no finite value at all.
    /// </summary>
    [Fact]
    public void AnInfiniteRegionStartingAtTheLimitIsDrawnAsAnArea()
    {
        var code = TikzPlots.ToTikzPlotCode(new DelayServiceCurve(3), "d", Until(3));

        Assert.Contains("\\fill", code);
        // and the finite line does not run on through the strip where the curve is infinite
        Assert.DoesNotContain(3.09m, FirstRunCoordinates(code).Select(p => p.x));
    }

    #endregion

    #region Reading the emitted code

    private const string CurveColor = "blue!60!black";

    /// <summary>
    /// The line drawing the first run of the first plotted item, which is neither the marks nor the grey annotations.
    /// </summary>
    /// <remarks>
    /// A sequence broken by a discontinuity is emitted as several runs; the tests that need a later one read the code directly.
    /// </remarks>
    private static string FirstDataPlotLine(string code, string color = CurveColor)
        => code.Split('\n')
            .Select(l => l.Trim())
            .First(l => l.StartsWith("\\addplot", StringComparison.Ordinal)
                        && l.Contains($"color = {color}")
                        && !l.Contains("only marks"));

    private static List<(decimal x, decimal y)> FirstRunCoordinates(string code, string color = CurveColor)
        => ParseCoordinates(FirstDataPlotLine(code, color));

    /// <summary>
    /// Every point drawn as a dot, across all the "only marks" plots.
    /// </summary>
    private static List<(decimal x, decimal y)> PointMarks(string code, string color = CurveColor)
        => code.Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.StartsWith("\\addplot", StringComparison.Ordinal)
                        && l.Contains($"color = {color}")
                        && l.Contains("only marks"))
            .SelectMany(ParseCoordinates)
            .ToList();

    private static List<(decimal x, decimal y)> ParseCoordinates(string line)
    {
        var open = line.IndexOf("coordinates {", StringComparison.Ordinal);
        Assert.True(open >= 0, $"no coordinates in: {line}");
        var body = line[(open + "coordinates {".Length)..line.IndexOf('}', open)];

        return Regex.Matches(body, @"\(\s*(-?[\d.]+)\s*,\s*(-?[\d.]+)\s*\)")
            .Select(m => (
                decimal.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
                decimal.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture)))
            .ToList();
    }

    #endregion
}
