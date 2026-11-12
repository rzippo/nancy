using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

#if BIG_RATIONAL
using System.Numerics;
#endif

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Element and segment lookups past the first pseudo-period, against the materialized extension.
/// </summary>
public class PeriodLookups
{
    /// <summary>
    /// Transient $[0, 1[$ and period $[1, 3[$; consecutive elements never merge, so the extension is the normalized cut.
    /// </summary>
    private static Curve JumpingPeriod() => new Curve(
        baseSequence: new Sequence(new Element[]
        {
            Point.Origin(),
            new Segment(startTime: 0, endTime: 1, rightLimitAtStartTime: 0, slope: 2),
            new Point(time: 1, value: 3),
            new Segment(startTime: 1, endTime: 2, rightLimitAtStartTime: 3, slope: 1),
            new Point(time: 2, value: 4),
            new Segment(startTime: 2, endTime: 3, rightLimitAtStartTime: 4, slope: 0)
        }),
        pseudoPeriodStart: 1,
        pseudoPeriodLength: 2,
        pseudoPeriodHeight: 2
    );

    /// <summary>
    /// Times on and between the period boundaries of the second to fourth pseudo-periods.
    /// </summary>
    public static IEnumerable<object[]> TimesPastFirstPeriod()
        => Enumerable.Range(1, 3)
            .SelectMany(k => new[] { 0, new Rational(1, 3), 1, new Rational(3, 2) }
                .Select(offset => new object[] { 1 + 2 * k + offset }));

    [Theory]
    [MemberData(nameof(TimesPastFirstPeriod))]
    public void ElementAtMatchesExtension(Rational time)
    {
        var curve = JumpingPeriod();
        var extension = curve.Extend(12);
        Assert.Equal(extension.GetElementAt(time), curve.GetElementAt(time));
        Assert.Equal(extension.ValueAt(time), curve.ValueAt(time));
    }

    [Theory]
    [MemberData(nameof(TimesPastFirstPeriod))]
    public void SegmentBeforeMatchesExtension(Rational time)
    {
        var curve = JumpingPeriod();
        var extension = curve.Extend(12);
        Assert.Equal(extension.GetSegmentBefore(time), curve.GetSegmentBefore(time));
        Assert.Equal(extension.GetSegmentBefore(time, cutToTime: false), curve.GetSegmentBefore(time, cutToTime: false));
    }

    [Theory]
    [MemberData(nameof(TimesPastFirstPeriod))]
    public void SegmentAfterMatchesExtension(Rational time)
    {
        var curve = JumpingPeriod();
        var extension = curve.Extend(12);
        Assert.Equal(extension.GetSegmentAfter(time), curve.GetSegmentAfter(time, autoMerge: false));
        Assert.Equal(extension.GetSegmentAfter(time, cutToTime: false), curve.GetSegmentAfter(time, autoMerge: false, cutToTime: false));
        Assert.Equal(extension.GetSegmentAfter(time), curve.GetSegmentAfter(time));
    }

    /// <summary>
    /// Positions in the transient, at the end of the stored period, at the joins of later periods and deep in the tail.
    /// </summary>
    public static IEnumerable<object[]> PositionsInExtension()
        => new[] { 0, 1, 2, 5, 6, 9, 10, 130, 257 }
            .Select(index => new object[] { index });

    [Theory]
    [MemberData(nameof(PositionsInExtension))]
    public void ElementByIndexMatchesExtension(int index)
    {
        var curve = JumpingPeriod();
        var extension = curve.Extend(300);
        Assert.Equal(extension.Elements[index], curve.GetElementByIndex(index));
    }

    /// <summary>
    /// The staircase $\lceil t \rceil$ has one period per unit of time, so at time $3 \cdot 10^9$ the period index is past the int range.
    /// </summary>
    private static readonly Rational PastAnIntPeriod = 3_000_000_000L;

    [Fact]
    public void ElementByIndexReachesPastAnIntPeriod()
    {
        // A point and a segment per period, so the segment that starts at time k is at position 2k + 1.
        var stair = new StairCurve(1, 1);
        Assert.Equal(new Segment(PastAnIntPeriod, PastAnIntPeriod + 1, PastAnIntPeriod + 1, 0), stair.GetElementByIndex(2 * 3_000_000_000L + 1));
    }

    [Fact]
    public void LookupsByTimeReachPastAnIntPeriod()
    {
        var stair = new StairCurve(1, 1);
        var segment = new Segment(PastAnIntPeriod, PastAnIntPeriod + 1, PastAnIntPeriod + 1, 0);
        Assert.Equal(segment, stair.GetElementAt(PastAnIntPeriod + new Rational(1, 2)));
        Assert.Equal(segment, stair.GetSegmentAfter(PastAnIntPeriod, autoMerge: false));
        Assert.Equal(segment, stair.GetSegmentBefore(PastAnIntPeriod + 1));
    }

    [Fact]
    public void CutsReachPastAnIntPeriod()
    {
        var stair = new StairCurve(1, 1);
        Element[] expected =
        [
            new Point(PastAnIntPeriod, PastAnIntPeriod), new Segment(PastAnIntPeriod, PastAnIntPeriod + 1, PastAnIntPeriod + 1, 0),
            new Point(PastAnIntPeriod + 1, PastAnIntPeriod + 1), new Segment(PastAnIntPeriod + 1, PastAnIntPeriod + 2, PastAnIntPeriod + 2, 0),
        ];
        Assert.Equal(expected, stair.Cut(PastAnIntPeriod, PastAnIntPeriod + 2).Elements);
        Assert.Equal(expected, stair.CutAsEnumerable(PastAnIntPeriod, PastAnIntPeriod + 2).ToArray());
    }

    #if BIG_RATIONAL
    // The period index of a time is a quotient of rationals; next to a boundary it differs from an integer
    // by less than a decimal can represent, here $1 / (3 \cdot 10^{28})$ of a period.
    private static readonly Rational NearBoundary = new Rational(BigInteger.One, BigInteger.Parse("30000000000000000000000000000"));

    [Fact]
    public void ElementAtJustBeforeBoundary()
    {
        var curve = JumpingPeriod();
        var time = 5 - 2 * NearBoundary;
        var segment = Assert.IsType<Segment>(curve.GetElementAt(time));
        Assert.Equal(new Segment(startTime: 4, endTime: 5, rightLimitAtStartTime: 6, slope: 0), segment);
        Assert.Equal(6, curve.ValueAt(time));
    }

    [Fact]
    public void SegmentAfterJustBeforeBoundary()
    {
        var curve = JumpingPeriod();
        var time = 5 - 2 * NearBoundary;
        var expected = new Segment(startTime: 4, endTime: 5, rightLimitAtStartTime: 6, slope: 0);
        Assert.Equal(expected, curve.GetSegmentAfter(time, autoMerge: false, cutToTime: false));
        Assert.Equal(expected, curve.GetSegmentAfter(time, cutToTime: false));
    }

    [Fact]
    public void SegmentBeforeJustAfterBoundary()
    {
        var curve = JumpingPeriod();
        var time = 5 + 2 * NearBoundary;
        var expected = new Segment(startTime: 5, endTime: 6, rightLimitAtStartTime: 7, slope: 1);
        Assert.Equal(expected, curve.GetSegmentBefore(time, cutToTime: false));
    }
    #endif
}
