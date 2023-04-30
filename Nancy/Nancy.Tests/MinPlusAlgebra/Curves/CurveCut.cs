using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveCut
{
    public static IEnumerable<object[]> CutSelfEquivalenceTestCases()
    {
        var testCases = new (Curve curve, Rational cutStart, Rational cutEnd)[]
        {
            (
                new FlowControlCurve(new Rational(2*5*11), 4000, new Rational(2*5*11)),
                0,
                630
            ),
            (
                new FlowControlCurve(new Rational(3*7*13), 5000, new Rational(3*7*13)),
                0,
                630
            ),
            (
                Curve.Minimum(
                    new FlowControlCurve(new Rational(2*5*11), 4000, new Rational(2*5*11)),
                    new FlowControlCurve(new Rational(3*7*13), 5000, new Rational(3*7*13))
                ),
                0,
                630
            ),
            (
                new FlowControlCurve(height: 363, latency: 149, rate: 2), 
                new Rational(314467,922),
                new Rational(1299, 2)
            ),
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] {testCase.curve, testCase.cutStart, testCase.cutEnd};
        }
    }

    [Theory]
    [MemberData(nameof(CutSelfEquivalenceTestCases))]
    public void CutSelfEquivalence(Curve curve, Rational cutStart, Rational cutEnd)
    {
        var cut = curve.Cut(cutStart, cutEnd);
        foreach (var element in cut.Elements)
        {
            if (element is Segment s)
            {
                Assert.Equal(curve.RightLimitAt(s.StartTime), s.RightLimitAtStartTime);
                Assert.Equal(curve.LeftLimitAt(s.EndTime), s.LeftLimitAtEndTime);
            }
            else if(element is Point p)
                Assert.Equal(curve.ValueAt(p.Time), p.Value);
        }
    }

    public static IEnumerable<object[]> CutTestCases()
    {
        var testCases = new (Curve curve, Rational cutStart, Rational cutEnd, bool isStartIncluded, bool isEndIncluded, Sequence expected)[]
        {
            (
                curve: new RateLatencyServiceCurve(5, 2),
                cutStart: 0,
                cutEnd: 4,
                isStartIncluded: true,
                isEndIncluded: true,
                expected: new Sequence(new Element[]
                {
                    Point.Origin(),
                    Segment.Zero(0, 2),
                    Point.Zero(2),
                    new Segment(2, 4, 0, 5),
                    new Point(4, 10)
                })
            ),
            (
                curve: new RateLatencyServiceCurve(5, 2),
                cutStart: 0,
                cutEnd: 4,
                isStartIncluded: false,
                isEndIncluded: false,
                expected: new Sequence(new Element[]
                {
                    Segment.Zero(0, 2),
                    Point.Zero(2),
                    new Segment(2, 4, 0, 5)
                })
            ),
            (
                curve: new RateLatencyServiceCurve(5, 2),
                cutStart: 4,
                cutEnd: 6,
                isStartIncluded: true,
                isEndIncluded: true,
                expected: new Sequence(new Element[]
                {
                    new Point(4, 10),
                    new Segment(4, 6, 10, 5),
                    new Point(6, 20)
                })
            ),
            (
                curve: new RateLatencyServiceCurve(5, 2),
                cutStart: 4,
                cutEnd: 6,
                isStartIncluded: false,
                isEndIncluded: false,
                expected: new Sequence(new Element[]
                {
                    new Segment(4, 6, 10, 5),
                })
            ),
            (
                curve: new SigmaRhoArrivalCurve(5, 3),
                cutStart: 0,
                cutEnd: 5,
                isStartIncluded: true,
                isEndIncluded: false,
                expected: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 5, 5, 3)
                })
            ),
            // [t, t] => should yield the point in t
            (
                curve: new SigmaRhoArrivalCurve(5, 3),
                cutStart: 0,
                cutEnd: 0,
                isStartIncluded: true,
                isEndIncluded: true,
                expected: new Sequence(new Element[]
                {
                    Point.Origin()
                })
            )
        };

        foreach (var (curve, cutStart, cutEnd, isStartIncluded, isEndIncluded, expected) in testCases)
        {
            yield return new object[] {curve, cutStart, cutEnd, isStartIncluded, isEndIncluded, expected};
        }
    }

    [Theory]
    [MemberData(nameof(CutTestCases))]
    public void CutEquivalence(Curve curve, Rational cutStart, Rational cutEnd, bool isStartIncluded,
        bool isEndIncluded, Sequence expected)
    {
        var sequence = curve.Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded);
        Assert.True(Sequence.Equivalent(expected, sequence));
    }

    [Theory]
    [MemberData(nameof(CutTestCases))]
    public void CutCount(Curve curve, Rational cutStart, Rational cutEnd, bool isStartIncluded,
        bool isEndIncluded, Sequence expected)
    {
        var curveCount = curve.Count(cutStart, cutEnd, isStartIncluded, isEndIncluded);
        Assert.Equal(expected.Count, curveCount);
    }
}