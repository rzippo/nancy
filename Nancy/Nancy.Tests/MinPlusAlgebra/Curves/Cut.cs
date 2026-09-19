using System;
using System.Linq;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class Cut
{
    public static List<(Curve curve, Rational cutStart, Rational cutEnd)> CutSelfEquivalenceTuples =
    [
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
    ];

    public static IEnumerable<object[]> CutSelfEquivalenceTestCases()
        => CutSelfEquivalenceTuples.ToXUnitTestCases();

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

    [Theory]
    [MemberData(nameof(CutSelfEquivalenceTestCases))]
    public void CutIntervalSelfEquivalence(Curve curve, Rational cutStart, Rational cutEnd)
    {
        var interval = new Interval(cutStart, cutEnd, true, false);
        var cut = curve.Cut(interval);
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

    [Theory]
    [MemberData(nameof(CutSelfEquivalenceTestCases))]
    public void CutAsEnumerableSelfEquivalence(Curve curve, Rational cutStart, Rational cutEnd)
    {
        var cutEnumerable = curve.CutAsEnumerable(cutStart, cutEnd);
        foreach (var element in cutEnumerable)
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

    [Theory]
    [MemberData(nameof(CutSelfEquivalenceTestCases))]
    public void CutAsEnumerableIntervalSelfEquivalence(Curve curve, Rational cutStart, Rational cutEnd)
    {
        var interval = new Interval(cutStart, cutEnd, true, false);
        var cutEnumerable = curve.CutAsEnumerable(interval);
        foreach (var element in cutEnumerable)
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

    public static 
        List<(Curve curve, Rational cutStart, Rational cutEnd, bool isStartIncluded, bool isEndIncluded, Sequence expected)> 
        CutKnownTuples =
    [
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
        ),
        // [t, t] past the end of the base sequence, where the cut must extend the curve.
        // For an ultimately affine curve the extension is built as a single segment, which a zero-width window cannot be.
        (
            curve: new SigmaRhoArrivalCurve(5, 3),
            cutStart: 7,
            cutEnd: 7,
            isStartIncluded: true,
            isEndIncluded: true,
            expected: new Sequence(new Element[]
            {
                new Point(7, 26)
            })
        ),
        (
            curve: new RateLatencyServiceCurve(5, 2),
            cutStart: 10,
            cutEnd: 10,
            isStartIncluded: true,
            isEndIncluded: true,
            expected: new Sequence(new Element[]
            {
                new Point(10, 40)
            })
        ),
        (
            // the lower pseudo-inverse of a rate-latency curve, cut over the image of a constant arrival curve
            curve: new RateLatencyServiceCurve(1, 0).LowerPseudoInverse(),
            cutStart: 30,
            cutEnd: 30,
            isStartIncluded: true,
            isEndIncluded: true,
            expected: new Sequence(new Element[]
            {
                new Point(30, 30)
            })
        ),
        (
            // not ultimately affine: the extension is built by periods, and already worked
            curve: new StairCurve(30, 100).DelayBy(0).ToRightContinuous(),
            cutStart: 250,
            cutEnd: 250,
            isStartIncluded: true,
            isEndIncluded: true,
            expected: new Sequence(new Element[]
            {
                new Point(250, 90)
            })
        )
    ];

    public static List<(Curve curve, Rational time)> DegenerateCutTuples =
    [
        (new SigmaRhoArrivalCurve(5, 3), 0),
        (new SigmaRhoArrivalCurve(5, 3), 1),
        (new SigmaRhoArrivalCurve(5, 3), new Rational(15, 2)),
        (new SigmaRhoArrivalCurve(5, 3), 250),
        (new RateLatencyServiceCurve(5, 2), 0),
        (new RateLatencyServiceCurve(5, 2), 3),
        (new RateLatencyServiceCurve(5, 2), 100),
        (new RateLatencyServiceCurve(1, 0).LowerPseudoInverse(), 0),
        (new RateLatencyServiceCurve(1, 0).LowerPseudoInverse(), 30),
        (new DelayServiceCurve(3), 0),
        (new DelayServiceCurve(3), 3),
        (new DelayServiceCurve(3), new Rational(15, 2)),
        (new FlowControlCurve(3, 5, 2), 1),
        (new FlowControlCurve(3, 5, 2), 100),
        (new StairCurve(30, 100).DelayBy(0), 250),
        (new StairCurve(30, 100).DelayBy(0).ToRightContinuous(), 250),
    ];

    public static IEnumerable<object[]> DegenerateCutTestCases()
        => DegenerateCutTuples.ToXUnitTestCases();

    /// <summary>
    /// A zero-width, closed window is the single point of the curve at that time, wherever it lies:
    /// inside the base sequence or in the extension.
    /// </summary>
    [Theory]
    [MemberData(nameof(DegenerateCutTestCases))]
    public void DegenerateCutIsThePoint(Curve curve, Rational time)
    {
        var expected = new Sequence([new Point(time, curve.ValueAt(time))]);

        Assert.Equal(expected, curve.Cut(time, time, true, true));
        Assert.Equal(expected, curve.Cut(new Interval(time, time, true, true)));
        Assert.Equal(expected, curve.CutAsEnumerable(time, time, true, true).ToSequence());
        Assert.Equal(expected, curve.CutAsEnumerable(new Interval(time, time, true, true)).ToSequence());
        Assert.Equal(1, curve.Count(time, time, true, true));
    }

    /// <summary>
    /// A zero-width window that does not include both of its endpoints describes nothing, and is rejected.
    /// </summary>
    [Theory]
    [MemberData(nameof(DegenerateCutTestCases))]
    public void DegenerateCutRequiresBothEndpoints(Curve curve, Rational time)
    {
        foreach (var (isStartIncluded, isEndIncluded) in new[] { (true, false), (false, true), (false, false) })
        {
            Assert.Throws<ArgumentException>(() => curve.Cut(time, time, isStartIncluded, isEndIncluded));
            Assert.Throws<ArgumentException>(() =>
                curve.CutAsEnumerable(time, time, isStartIncluded, isEndIncluded).ToList());
        }
    }
    
    public static IEnumerable<object[]> CutTestCases()
        => CutKnownTuples.ToXUnitTestCases();

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
    public void CutIntervalEquivalence(Curve curve, Rational cutStart, Rational cutEnd, bool isStartIncluded,
        bool isEndIncluded, Sequence expected)
    {
        var interval = new Interval(cutStart, cutEnd, isStartIncluded, isEndIncluded);
        var sequence = curve.Cut(interval);
        Assert.True(Sequence.Equivalent(expected, sequence));
    }
    
    [Theory]
    [MemberData(nameof(CutTestCases))]
    public void CutAsEnumerableEquivalence(Curve curve, Rational cutStart, Rational cutEnd, bool isStartIncluded,
        bool isEndIncluded, Sequence expected)
    {
        var cutEnumerable = curve.CutAsEnumerable(cutStart, cutEnd, isStartIncluded, isEndIncluded);
        var sequence = cutEnumerable.ToSequence();
        Assert.True(Sequence.Equivalent(expected, sequence));
    }
    
    [Theory]
    [MemberData(nameof(CutTestCases))]
    public void CutAsEnumerableIntervalEquivalence(Curve curve, Rational cutStart, Rational cutEnd, bool isStartIncluded,
        bool isEndIncluded, Sequence expected)
    {
        var interval = new Interval(cutStart, cutEnd, isStartIncluded, isEndIncluded);
        var cutEnumerable = curve.CutAsEnumerable(interval);
        var sequence = cutEnumerable.ToSequence();
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
    
    [Theory]
    [MemberData(nameof(CutTestCases))]
    public void CutCountInterval(Curve curve, Rational cutStart, Rational cutEnd, bool isStartIncluded,
        bool isEndIncluded, Sequence expected)
    {
        var interval =  new Interval(cutStart, cutEnd, isStartIncluded, isEndIncluded);
        var curveCount = curve.Count(interval);
        Assert.Equal(expected.Count, curveCount);
    }
}