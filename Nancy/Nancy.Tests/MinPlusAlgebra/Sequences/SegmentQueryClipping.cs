using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SegmentQueryClipping
{
    public static IEnumerable<(Sequence Sequence, Rational Time, Segment Before, Segment After)> InteriorCases()
    {
        foreach (var slope in new Rational[] { -2, 0, 3 })
        foreach (var closed in new[] { false, true })
        {
            var elements = new List<Element>();
            if (closed) elements.Add(new Point(2, 99));
            elements.Add(new Segment(2, 6, 7, slope));
            if (closed) elements.Add(new Point(6, -99));
            yield return (new Sequence(elements), 4, new Segment(2, 4, 7, slope), new Segment(4, 6, 7 + 2 * slope, slope));
        }
        foreach (var value in new[] { Rational.PlusInfinity, Rational.MinusInfinity })
            yield return (new Sequence([new Segment(2, 6, value, 0)]), 4,
                new Segment(2, 4, value, 0), new Segment(4, 6, value, 0));
    }

    public static IEnumerable<object[]> BeforeInteriors => InteriorCases().Select(c => (c.Sequence, c.Time, c.Before)).ToXUnitTestCases();
    public static IEnumerable<object[]> AfterInteriors => InteriorCases().Select(c => (c.Sequence, c.Time, c.After)).ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(BeforeInteriors))]
    public void BeforeClipsEverySequenceLookup(Sequence sequence, Rational time, Segment before)
    {
        Assert.Equal(before, sequence.GetSegmentBefore(time));
        Assert.Equal(before, sequence.Elements.GetSegmentBefore(time));
        var (segment, index) = sequence.GetSegmentBefore_Linear(time);
        Assert.Equal(before, segment);
        Assert.IsType<Segment>(sequence.Elements[index]);
        Assert.Equal(before, sequence.GetSegmentBefore(time, cutToTime: true));
        Assert.Same(sequence.Elements[index], sequence.GetSegmentBefore(time, cutToTime: false));
        Assert.Same(sequence.Elements[index], sequence.Elements.GetSegmentBefore(time, cutToTime: false));
        Assert.Same(sequence.Elements[index], sequence.GetSegmentBefore_Linear(time, index, cutToTime: false).segment);
    }

    [Theory]
    [MemberData(nameof(AfterInteriors))]
    public void AfterClipsEverySequenceLookup(Sequence sequence, Rational time, Segment after)
    {
        Assert.Equal(after, sequence.GetSegmentAfter(time));
        Assert.Equal(after, sequence.Elements.GetSegmentAfter(time));
        var (segment, index) = sequence.GetSegmentAfter_Linear(time);
        Assert.Equal(after, segment);
        Assert.IsType<Segment>(sequence.Elements[index]);
        Assert.Equal(after, sequence.GetSegmentAfter(time, cutToTime: true));
        Assert.Same(sequence.Elements[index], sequence.GetSegmentAfter(time, cutToTime: false));
        Assert.Same(sequence.Elements[index], sequence.Elements.GetSegmentAfter(time, cutToTime: false));
        Assert.Same(sequence.Elements[index], sequence.GetSegmentAfter_Linear(time, index, cutToTime: false).segment);
    }

    private static Sequence BrokenSlope() => new([
        new Point(0, 0), new Segment(0, 2, 0, 1), new Point(2, 2),
        new Segment(2, 4, 2, -1)]);

    [Fact]
    public void BreakpointSelectsTheRequestedSideWithoutCopying()
    {
        var sequence = BrokenSlope();
        var left = sequence.Elements[1];
        var right = sequence.Elements[3];
        Assert.Same(left, sequence.GetSegmentBefore(2));
        Assert.Same(left, sequence.Elements.GetSegmentBefore(2));
        Assert.Same(left, sequence.GetSegmentBefore_Linear(2, 1).segment);
        Assert.Same(right, sequence.GetSegmentAfter(2));
        Assert.Same(right, sequence.Elements.GetSegmentAfter(2));
        Assert.Same(right, sequence.GetSegmentAfter_Linear(2, 1).segment);
        Assert.Same(left, sequence.GetSegmentAfter(0));
        Assert.Same(right, sequence.GetSegmentBefore(4));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingSidesRemainErrors(bool cutToTime)
    {
        var sequence = BrokenSlope();
        Assert.Throws<ArgumentException>(() => sequence.GetSegmentBefore(0, cutToTime));
        Assert.Throws<ArgumentException>(() => sequence.Elements.GetSegmentBefore(0, cutToTime));
        Assert.Throws<ArgumentException>(() => sequence.GetSegmentBefore_Linear(0, cutToTime: cutToTime));
        Assert.Throws<ArgumentException>(() => sequence.GetSegmentAfter(4, cutToTime));
        Assert.Throws<ArgumentException>(() => sequence.Elements.GetSegmentAfter(4, cutToTime));
        Assert.Throws<ArgumentException>(() => sequence.GetSegmentAfter_Linear(4, cutToTime: cutToTime));
    }

    public static IEnumerable<(Rational Time, Segment Before, Segment After)> PeriodCases => [
        (1, new Segment(0, 1, 0, 1), new Segment(1, 2, 1, 1)),
        (3, new Segment(2, 3, 2, -1), new Segment(3, 4, 1, -1)),
        (5, new Segment(4, 5, 0, 1), new Segment(5, 6, 1, 1)),
        (7, new Segment(6, 7, 2, -1), new Segment(7, 8, 1, -1))];
    public static IEnumerable<object[]> BeforePeriods => PeriodCases.Select(c => (c.Time, c.Before)).ToXUnitTestCases();
    public static IEnumerable<object[]> AfterPeriods => PeriodCases.Select(c => (c.Time, c.After)).ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(BeforePeriods))]
    public void CurveBeforeClipsInBaseAndLaterPeriods(Rational time, Segment before)
    {
        var curve = new Curve(BrokenSlope(), 0, 4, 0);
        Assert.Equal(before, curve.GetSegmentBefore(time));
        Assert.True(curve.GetSegmentBefore(time, cutToTime: false).EndTime > time);
    }

    [Theory]
    [MemberData(nameof(AfterPeriods))]
    public void CurveAfterClipsInBaseAndLaterPeriods(Rational time, Segment after)
    {
        var curve = new Curve(BrokenSlope(), 0, 4, 0);
        Assert.Equal(after, curve.GetSegmentAfter(time));
        Assert.Equal(after, curve.GetSegmentAfter(time, false));
        Assert.True(curve.GetSegmentAfter(time, cutToTime: false).StartTime < time);
        Assert.True(curve.GetSegmentAfter(time, autoMerge: false, cutToTime: false).StartTime < time);
    }

    [Fact]
    public void CurveAfterClipsAfterMergingAcrossThePeriodBoundary()
    {
        var curve = new Curve(new Sequence([
            new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 1),
            new Segment(1, 2, 1, -1), new Point(2, 0), new Segment(2, 3, 0, 1)]), 0, 3, 1);
        var time = new Rational(5, 2);
        Assert.Equal(new Segment(time, 4, new Rational(1, 2), 1), curve.GetSegmentAfter(time));
        Assert.Equal(new Segment(time, 3, new Rational(1, 2), 1), curve.GetSegmentAfter(time, false));
        Assert.Equal(new Segment(2, 4, 0, 1), curve.GetSegmentAfter(time, cutToTime: false));
        Assert.Equal(new Segment(2, 3, 0, 1), curve.GetSegmentAfter(time, false, false));
    }

    [Fact]
    public void NeighbourhoodRetainsOriginalBounds()
    {
        var sequence = BrokenSlope();
        Assert.Equal(0, sequence.CutToNeighbourhood(1, 3).DefinedFrom);
        Assert.Equal(4, sequence.CutToNeighbourhood(1, 3).DefinedUntil);
        var curve = new Curve(sequence, 0, 4, 0);
        Assert.Equal(0, curve.CutToNeighbourhood(1, 3).DefinedFrom);
        Assert.Equal(4, curve.CutToNeighbourhood(1, 3).DefinedUntil);
    }

    [Fact]
    public void UnboundedSegmentsRetainTheirUncutEndpoint()
    {
        var segment = new Segment(0, Rational.PlusInfinity, 1, 2);
        var sequence = new Sequence([segment]);
        Assert.Equal(new Segment(0, 3, 1, 2), sequence.GetSegmentBefore(3));
        Assert.Equal(new Segment(3, Rational.PlusInfinity, 7, 2), sequence.GetSegmentAfter(3));
        Assert.Equal(new Segment(0, 3, 1, 2), sequence.Elements.GetSegmentBefore(3));
        Assert.Equal(new Segment(3, Rational.PlusInfinity, 7, 2), sequence.Elements.GetSegmentAfter(3));
        Assert.Same(segment, sequence.GetSegmentBefore(Rational.PlusInfinity));
    }

    [Fact]
    public void UltimatelyAffineCurveAfterClipsItsEarlyReturn()
    {
        var curve = new Curve(new Sequence([new Point(0, 1), new Segment(0, 4, 1, 2)]), 0, 4, 8);
        Assert.Equal(new Segment(1, 4, 3, 2), curve.GetSegmentAfter(1));
        Assert.Equal(new Segment(0, 4, 1, 2), curve.GetSegmentAfter(1, cutToTime: false));
    }

    [Fact]
    public void StreamBeforeStopsAndDisposesAtTheSelectedSegment()
    {
        var disposed = false;
        IEnumerable<Element> Elements()
        {
            try
            {
                yield return new Segment(2, 6, 7, 3);
                throw new InvalidOperationException("Unread tail");
            }
            finally { disposed = true; }
        }
        Assert.Equal(new Segment(2, 4, 7, 3), Elements().GetSegmentBefore(4));
        Assert.True(disposed);
    }

    [Fact]
    public void StreamAfterStopsAndDisposesAtTheSelectedSegment()
    {
        var disposed = false;
        IEnumerable<Element> Elements()
        {
            try
            {
                yield return new Segment(2, 6, 7, 3);
                throw new InvalidOperationException("Unread tail");
            }
            finally { disposed = true; }
        }
        Assert.Equal(new Segment(4, 6, 13, 3), Elements().GetSegmentAfter(4));
        Assert.True(disposed);
    }
}
