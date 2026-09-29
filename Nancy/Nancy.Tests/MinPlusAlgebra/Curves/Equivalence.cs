using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class Equivalence
{
    // Set of distinct curves
    public static List<Curve> TestCurves = new List<Curve>
    {
        new SigmaRhoArrivalCurve(sigma: 100, rho: 5),
        new RateLatencyServiceCurve(rate: 20, latency: 10),
        new Curve(
            baseSequence: new Sequence(new List<Element>()
            {
                Point.Origin(),
                new Segment(0,1, 0, 1),
                new Point(1, 1),
                Segment.Constant(1, 3, 1),
                new Point(3, 1),
                new Segment(3,4, 1, 1)
            }),
            pseudoPeriodStart: 2,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 1
        ),
        new Curve(
            baseSequence: new Sequence(new List<Element>()
            {
                Point.Origin(),
                new Segment(0,1, 0, 1),
                new Point(1, 1),
                Segment.Constant(1, 3, 1),
                new Point(3, 1),
                new Segment(3,4, 1, 1),
                new Point(4, 2),
                new Segment(4,5, 2, 0)
            }),
            pseudoPeriodStart: 2,
            pseudoPeriodLength: 3,
            pseudoPeriodHeight: 1
        ),
        new Curve(
            baseSequence: new Sequence(new List<Element>()
            {
                Point.Origin(),
                new Segment(0,2, 0, 1),
                new Point(2, 2),
                Segment.Constant(2, 3, 2),
                new Point(3, 2),
                Segment.Constant(3,4, 2),
                new Point(4, 2),
                new Segment(4,5, 2, 1),
            }),
            pseudoPeriodStart: 3,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 1
        ),
        new Curve(
            baseSequence: new Sequence(new List<Element>()
            {
                Point.Origin(),
                new Segment(0,4, 0, new Rational(1, 2)),
                new Point(4, 2),
                new Segment(4, 5, 2, 1),
                new Point(5, 3),
                Segment.Constant(5,6, 3),
                new Point(6, 3),
                new Segment(6, 7, 3, 1),
                new Point(7, 4),
                Segment.Constant(7,8, 4),
            }),
            pseudoPeriodStart: 0,
            pseudoPeriodLength: 8,
            pseudoPeriodHeight: 4
        ),
        new Curve(
            baseSequence: new Sequence(new List<Element>()
            {
                Point.Origin(),
                new Segment(0, 4, 0, new Rational(1, 2)),
                new Point(4, 2),
                new Segment(4, 5, 2, 1),
                new Point(5, 3),
                Segment.Constant(5, 6, 3),
                new Point(6, 3),
                new Segment(6, 7, 3, 1),
                new Point(7, 4),
                Segment.Constant(7, 8, 4),
                new Point(8, 4),
                new Segment(8, 12, 4, new Rational(1, 2)),
                new Point(12, 6),
                new Segment(12, 13, 6, 1),
                new Point(13, 7),
                Segment.Constant(13, 14, 7),
            }),
            pseudoPeriodStart: 12,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 1
        ),
        FakeEquivalenceGenerator(3).f,
        FakeEquivalenceGenerator(3).g,
        FakeEquivalenceGenerator(5).f,
        FakeEquivalenceGenerator(5).g,
        FakeEquivalenceGenerator(20).f,
        FakeEquivalenceGenerator(20).g,
    };

    /// <summary>
    /// Given <paramref name="n"/>, this method will generate a pair of curve which are equivalent in [0, max(Tf + n * df, Tg + n * dg)[,
    /// but are *not* equivalent outside of such interval. 
    /// </summary>
    public static (Curve f, Curve g) FakeEquivalenceGenerator(int n)
    {
        if (n < 2)
            throw new ArgumentException();

        var elements = new List<Element>();
        elements.Add(Point.Origin());
        elements.Add(new Segment(0, 4, 0, new Rational(1, 2)));
        for (int i = 0; i < n; i++)
        {
            elements.Add(new Point(4 + i * 2, 2 + i * 1));
            elements.Add(new Segment(4 + i * 2, 5 + i * 2, 2 + i * 1, 1));
            elements.Add(new Point(5 + i * 2, 3 + i * 1));
            elements.Add(Segment.Constant(5 + i * 2, 6 + i * 2, 3 + i * 1));
        }

        var f = new Curve(
            baseSequence: new Sequence(elements),
            pseudoPeriodStart: 0,
            pseudoPeriodLength: 4 + n * 2,
            pseudoPeriodHeight: 2 + n * 1
        );

        var g = new Curve(
            baseSequence: f.Cut(0, n * f.PseudoPeriodLength  - 2 * (n - 1)),
            pseudoPeriodStart: n * f.PseudoPeriodLength  - 2 * n,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 1
        );

        return (f, g);
    }

    public static IEnumerable<object[]> GetSameCurveTestCases()
    {
        foreach (var curve in TestCurves)
        {
            yield return new object[] {curve, curve};
            yield return new object[] {curve, new Curve(curve)};

            var delayed = new Curve(
                baseSequence: curve.Cut(0, curve.SecondPseudoPeriodEnd + curve.PseudoPeriodLength / 2),
                pseudoPeriodStart: curve.FirstPseudoPeriodEnd + curve.PseudoPeriodLength / 2,
                pseudoPeriodLength: curve.PseudoPeriodLength,
                pseudoPeriodHeight: curve.PseudoPeriodHeight
            );
            yield return new object[] {curve, delayed};
        }
    }

    public static IEnumerable<object[]> GetDifferentCurveTestCases()
    {
        foreach (var f in TestCurves)
        {
            foreach (var g in TestCurves)
            {
                if(g == f)
                    continue;

                yield return new object[] {f, g};
            }
        }
    }

    [Theory]
    [MemberData(nameof(GetSameCurveTestCases))]
    public void SameCurves(Curve f, Curve g)
    {
        Assert.True(g.Equivalent(f));
        Assert.True(g.EquivalentExceptOrigin(f));
    }

    [Theory]
    [MemberData(nameof(GetDifferentCurveTestCases))]
    public void DifferentCurves(Curve f, Curve g)
    {
        Assert.False(f.Equivalent(g));
        Assert.False(f.EquivalentExceptOrigin(g));
    }

    [Theory]
    [MemberData(nameof(GetSameCurveTestCases))]
    public void SameCurvesHaveNoFirstInequivalence(Curve f, Curve g)
    {
        Assert.Null(Curve.FindFirstInequivalence(f, g));
        Assert.Null(Curve.FindFirstInequivalence(g, f));
    }

    public static List<(Curve a, Curve b, Rational time, bool isStartIncluded, Rational? expected)> FirstInequivalenceCases =
    [
        // equal at 1, then 2 (t - 1) against 0 up to 2
        (new RateLatencyServiceCurve(2, 1), new StepCurve(4, 2), 1, true, 1),
        (new RateLatencyServiceCurve(2, 1), new StepCurve(4, 2), 1, false, 1),
        (new RateLatencyServiceCurve(2, 1), new StepCurve(4, 2), 0, true, 1),
        // equal but at 2
        (
            new RateLatencyServiceCurve(1, 0),
            new Curve(
                new Sequence([Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, 5), new Segment(2, 3, 2, 1), new Point(3, 3), new Segment(3, 4, 3, 1)]),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            ),
            0, true, 2
        ),
        // equal after 2
        (
            new RateLatencyServiceCurve(1, 0),
            new Curve(
                new Sequence([Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, 5), new Segment(2, 3, 2, 1), new Point(3, 3), new Segment(3, 4, 3, 1)]),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 1
            ),
            2, false, null
        ),
        // the same staircase, over pseudo-periods of 2 and of 3
        (StairOverPeriod(2), StairOverPeriod(3), 0, true, null),
        // the same staircase, but 7 at 5, in the last period before the least common multiple of the two
        (StairOverPeriod(2), StairOverPeriod(6, raisedPointAt: 5), 0, true, 5),
    ];

    /// <summary>
    /// The staircase with steps of 1 every 1, as a curve whose pseudo-period starts at 0 and spans <paramref name="periods"/> steps,
    /// optionally with the value at <paramref name="raisedPointAt"/> raised by 2.
    /// </summary>
    private static Curve StairOverPeriod(int periods, int? raisedPointAt = null)
    {
        var elements = new StairCurve(1, 1).Cut(0, periods).Elements
            .Select(element => element is Point p && p.Time == raisedPointAt ? new Point(p.Time, p.Value + 2) : element);
        return new Curve(new Sequence(elements), 0, periods, periods);
    }

    public static IEnumerable<object[]> GetFirstInequivalenceTestCases()
        => FirstInequivalenceCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetFirstInequivalenceTestCases))]
    public void FirstInequivalence(Curve a, Curve b, Rational time, bool isStartIncluded, Rational? expected)
    {
        Assert.Equal(expected, Curve.FindFirstInequivalenceAfter(a, b, time, isStartIncluded));
        Assert.Equal(expected, Curve.FindFirstInequivalenceAfter(b, a, time, isStartIncluded));
    }
}
