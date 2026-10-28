using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

public class DecreasingProperties
{
    public static IEnumerable<object[]> Inputs() => Properties.ElementCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Inputs))]
    public void NonIncreasingChecksSlopesAndBreakpointLimits(Element[] elements)
    {
        var expected = elements.OfType<Segment>()
            .All(s => s.Slope <= 0)
            && elements.EnumerateBreakpoints()
                .All(bp =>
                    (bp.left is null || bp.left.LeftLimitAtEndTime >= bp.center.Value)
                    && (bp.right is null || bp.center.Value >= bp.right.RightLimitAtStartTime));
        Assert.Equal(expected, ValidationCases.SingleUse(elements).IsNonIncreasing());
    }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void DecreasingChecksStrictSlopesAndBreakpointLimits(Element[] elements)
    {
        var expected = elements.OfType<Segment>()
            .All(s => s.Slope < 0)
            && elements.EnumerateBreakpoints()
                .All(bp =>
                    (bp.left is null || bp.left.LeftLimitAtEndTime >= bp.center.Value)
                    && (bp.right is null || bp.center.Value >= bp.right.RightLimitAtStartTime));
        Assert.Equal(expected, ValidationCases.SingleUse(elements).IsDecreasing());
    }

    [Fact]
    public void NonIncreasingRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsNonIncreasing());

    [Fact]
    public void DecreasingRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsDecreasing());

    public static IEnumerable<Element[]> MalformedInputs =>
    [
        [new Segment(0, 1, 0, -1), new Point(2, -1)],
        [new Point(1, 0), new Segment(0, 1, 0, -1)],
        [new Point(0, 0), new Point(0, 0)],
        [new Segment(0, 1, 0, -1), new Segment(1, 2, -1, -1)]
    ];
    public static IEnumerable<object[]> MalformedCases() => MalformedInputs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(MalformedCases))]
    public void NonIncreasingValidatesConsumedOrder(Element[] elements)
        => Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsNonIncreasing());

    [Theory]
    [MemberData(nameof(MalformedCases))]
    public void DecreasingValidatesConsumedOrder(Element[] elements)
        => Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsDecreasing());

    [Fact]
    public void NonIncreasingStopsAtAnIncreasingSegment()
        => Assert.False(IncreasingThenThrow().IsNonIncreasing());

    [Fact]
    public void DecreasingStopsAtAnIncreasingSegment()
        => Assert.False(IncreasingThenThrow().IsDecreasing());

    private static IEnumerable<Element> IncreasingThenThrow()
    {
        yield return new Segment(0, 1, 0, 1);
        throw new InvalidOperationException("The decisive segment was already consumed.");
    }

    [Fact]
    public void LeftContinuityYieldsAPointBeforeReadingItsRightNeighbor()
    {
        static IEnumerable<Element> Source()
        {
            yield return new Point(0, 0);
            yield return new Segment(0, 1, 0, 1);
            yield return new Point(1, 7);
            throw new InvalidOperationException("The right neighbor was requested.");
        }
        var prefix = Source().ToLeftContinuous()
            .Take(3)
            .ToArray();
        Assert.Equal(new Point(1, 1), prefix.Last());
    }

    [Fact]
    public void RightContinuityReadsTheRightNeighborBeforeYieldingAPoint()
    {
        static IEnumerable<Element> Source()
        {
            yield return new Point(0, 7);
            yield return new Segment(0, 1, 2, 1);
            throw new InvalidOperationException("The next breakpoint was requested.");
        }
        Assert.Equal(new Point(0, 2), Source().ToRightContinuous()
            .First());
    }
}
