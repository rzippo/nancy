using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

public class Fill
{
    public static IEnumerable<(Element[] Input, Interval Interval, Element[] Expected)> BoundedCases()
    {
        yield return ([], Interval.Open(0, 2), [Segment.Constant(0, 2, 7)]);
        yield return ([], Interval.Closed(0, 2), [new Point(0, 7), Segment.Constant(0, 2, 7), new Point(2, 7)]);
        yield return ([], new Interval(0, 2, true, false), [new Point(0, 7), Segment.Constant(0, 2, 7)]);
        yield return ([], new Interval(0, 2, false, true), [Segment.Constant(0, 2, 7), new Point(2, 7)]);
        yield return ([], Interval.Closed(1, 1), [new Point(1, 7)]);
        yield return ([new Point(-2, 4), new Point(4, 5)], Interval.Closed(0, 2),
            [new Point(-2, 4), new Point(0, 7), Segment.Constant(0, 2, 7), new Point(2, 7), new Point(4, 5)]);
        yield return ([new Point(-2, 4), new Point(4, 5)], Interval.Open(0, 2),
            [new Point(-2, 4), Segment.Constant(0, 2, 7), new Point(4, 5)]);
        yield return ([Segment.Constant(-2, 1, 4), Segment.Constant(3, 5, 5)], Interval.Closed(0, 4),
            [Segment.Constant(-2, 1, 4), new Point(1, 7), Segment.Constant(1, 3, 7), new Point(3, 7), Segment.Constant(3, 5, 5)]);
        yield return ([new Point(0, 4), new Point(2, 5)], Interval.Closed(0, 2),
            [new Point(0, 4), Segment.Constant(0, 2, 7), new Point(2, 5)]);
        yield return ([Segment.Constant(-1, 3, 4)], Interval.Closed(0, 2), [Segment.Constant(-1, 3, 4)]);
        yield return ([new Point(4, 5)], Interval.Closed(0, 2),
            [new Point(0, 7), Segment.Constant(0, 2, 7), new Point(2, 7), new Point(4, 5)]);
        yield return ([new Point(-2, 5)], Interval.Closed(0, 2),
            [new Point(-2, 5), new Point(0, 7), Segment.Constant(0, 2, 7), new Point(2, 7)]);
        yield return ([Segment.Constant(0, 1, 4), Segment.Constant(1, 2, 4)], Interval.Closed(1, 1),
            [Segment.Constant(0, 1, 4), new Point(1, 7), Segment.Constant(1, 2, 4)]);
        yield return ([new Point(1, 4)], Interval.Closed(1, 1), [new Point(1, 4)]);
        yield return ([new Point(-2, 4), new Point(4, 5)], Interval.Open(1, 1), [new Point(-2, 4), new Point(4, 5)]);
    }

    public static IEnumerable<object[]> Cases() => BoundedCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void FillingPreservesInputAndInsertsOnlyInsideItsInterval(Element[] input, Interval interval, Element[] expected)
    {
        var actual = ValidationCases.SingleUse(input).Fill(interval, fillWith: 7)
            .ToArray();
        Assert.Equal(expected, actual);
        var looseBounds = ValidationCases.SingleUse(input).Fill(interval.Lower, interval.Upper,
                interval.IsLowerIncluded, interval.IsUpperIncluded, fillWith: 7)
            .ToArray();
        Assert.Equal(expected, looseBounds);
    }

    public static IEnumerable<Element[]> OverlappingInputs()
    {
        yield return [new Point(0, 1), new Point(0, 1)];
        yield return [new Point(0, 1), new Point(0, 2)];
        yield return [Segment.Constant(0, 2, 1), new Point(1, 1)];
        yield return [Segment.Constant(0, 2, 1), Segment.Constant(1, 3, 1)];
        yield return [new Point(2, 1), new Point(1, 1)];
    }

    public static IEnumerable<object[]> InvalidCases() => OverlappingInputs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void FillingRejectsOverlapsAndOutOfOrderElements(Element[] input)
        => Assert.Throws<ArgumentException>(() => input.Fill(0, 3)
            .ToList());

    [Fact]
    public void ReversedBoundsFailBeforeEnumeration()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().Fill(2, 1));

    [Fact]
    public void AnEmptyIntervalReturnsTheElementsUnchanged()
    {
        Element[] elements = [new Point(0, 1), Segment.Constant(0, 2, 1), new Point(2, 1)];
        Assert.Equal(elements, elements.Fill(1, 1).ToArray());
        Assert.Equal(elements, elements.Fill(1, 1, isFromIncluded: false, isToIncluded: true).ToArray());
        Assert.Empty(Array.Empty<Element>().Fill(1, 1).ToList());
    }

    [Fact]
    public void FillingLeavesAnUnreadMalformedSuffixUnexamined()
    {
        Element[] input = [new Point(0, 1), new Point(0, 2)];
        Assert.Same(input[0], ValidationCases.SingleUse(input).Fill(0, 2)
            .First());
    }

    [Fact]
    public void AGapOutsideTheFillingIntervalRemainsAGap()
    {
        Element[] input = [new Point(0, 1), new Point(3, 2), new Point(5, 3)];
        Assert.Throws<ArgumentException>(() => input.ToSequence(0, 3));
    }
}
