using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveEquivalence
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
}