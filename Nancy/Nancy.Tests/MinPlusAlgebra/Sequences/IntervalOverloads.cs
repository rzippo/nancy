using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class IntervalOverloads
{
    private static Element[] CutInput() =>
    [
        new Point(0, 10), new Segment(0, 2, 10, 1), new Point(2, 20),
        new Segment(2, 4, 20, 2), new Point(4, 24)
    ];

    public static IEnumerable<(Interval Interval, Element[] Expected)> Cuts()
    {
        foreach (var left in new[] { false, true })
        foreach (var right in new[] { false, true })
        {
            var expected = new List<Element>();
            if (left) expected.Add(new Point(1, 11));
            expected.Add(new Segment(1, 2, 11, 1));
            expected.Add(new Point(2, 20));
            expected.Add(new Segment(2, 3, 20, 2));
            if (right) expected.Add(new Point(3, 22));
            yield return (new Interval(1, 3, left, right), expected.ToArray());
        }
        foreach (var (time, value) in new (Rational, Rational)[] { (0, 10), (1, 11), (2, 20), (3, 22), (4, 24) })
            yield return (Interval.Closed(time, time), [new Point(time, value)]);
    }

    public static IEnumerable<object[]> CutCases() => Cuts().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(CutCases))]
    public void CutUsesTheIntervalBoundsAndEndpointInclusion(Interval interval, Element[] expected)
        => Assert.Equal(expected, ValidationCases.SingleUse(CutInput()).Cut(interval).ToArray());

    public static IEnumerable<Interval> EmptyIntervals =>
    [
        Interval.Open(1, 1), Interval.OpenClosed(1, 1), Interval.ClosedOpen(1, 1)
    ];

    public static IEnumerable<(Element[] Input, Interval Interval)> InvalidCuts()
    {
        foreach (var interval in EmptyIntervals)
            yield return (CutInput(), interval);
        yield return (CutInput(), Interval.Closed(-1, 1));
        yield return (CutInput(), Interval.Closed(3, 5));
        yield return ([], Interval.Closed(0, 1));
        yield return ([Segment.Zero(0, 2)], Interval.Closed(0, 1));
        yield return ([Point.Origin(), Segment.Zero(1, 2)], Interval.ClosedOpen(0, 2));
    }

    public static IEnumerable<object[]> InvalidCutCases() => InvalidCuts().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidCutCases))]
    public void CutReportsInvalidInputDuringEnumeration(Element[] input, Interval interval)
    {
        var result = input.Cut(interval);
        Assert.Throws<ArgumentException>(() => result.ToArray());
    }

    public static IEnumerable<(Interval Interval, Rational? FillWith, Element[] Expected)> Fills()
    {
        foreach (var left in new[] { false, true })
        foreach (var right in new[] { false, true })
        foreach (var fillWith in new Rational?[] { null, -7, Rational.MinusInfinity })
        {
            var value = fillWith ?? Rational.PlusInfinity;
            var expected = new List<Element> { new Point(-1, 5) };
            if (left) expected.Add(new Point(0, value));
            expected.Add(Segment.Constant(0, 1, value));
            expected.Add(new Point(1, 10));
            expected.Add(Segment.Constant(1, 2, value));
            expected.Add(new Point(2, 20));
            expected.Add(Segment.Constant(2, 3, value));
            if (right) expected.Add(new Point(3, value));
            expected.Add(new Point(4, 30));
            yield return (new Interval(0, 3, left, right), fillWith, expected.ToArray());
        }
    }

    public static IEnumerable<object[]> FillCases() => Fills().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(FillCases))]
    public void FillPreservesExistingElementsAndFillsOnlyTheInterval(Interval interval, Rational? fillWith, Element[] expected)
    {
        Element[] input = [new Point(-1, 5), new Point(1, 10), new Point(2, 20), new Point(4, 30)];
        var result = Sequence.Fill(ValidationCases.SingleUse(input), interval, fillWith);
        Assert.Equal(expected, result.ToArray());
    }

    public static IEnumerable<object[]> EmptyFillCases() => EmptyIntervals.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(EmptyFillCases))]
    public void FillKeepsTheInputForAnEmptyInterval(Interval interval)
    {
        Element[] input = [new Point(-1, 5), new Point(4, 30)];
        Assert.Equal(input, Sequence.Fill(input, interval, -7).ToArray());
    }

    public static IEnumerable<(Element[] Input, Element[] Expected)> SingletonFills() =>
    [
        ([], [new Point(1, -7)]),
        ([new Point(1, 10)], [new Point(1, 10)])
    ];

    public static IEnumerable<object[]> SingletonFillCases() => SingletonFills().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(SingletonFillCases))]
    public void FillAddsOnlyAnAbsentSingleton(Element[] input, Element[] expected)
        => Assert.Equal(expected, Sequence.Fill(input, Interval.Closed(1, 1), -7).ToArray());

    [Fact]
    public void CutAcceptsAnUnboundedIntervalWithinTheSourceSupport()
    {
        Element[] input = [Segment.Constant(0, Rational.PlusInfinity, 3)];
        Element[] expected = [new Point(1, 3), Segment.Constant(1, Rational.PlusInfinity, 3)];
        Assert.Equal(expected, input.Cut(Interval.UnboundedAbove(1)).ToArray());
    }

    [Fact]
    public void FillAcceptsAnUnboundedInterval()
    {
        Element[] expected = [new Point(1, -7), Segment.Constant(1, Rational.PlusInfinity, -7)];
        Assert.Equal(expected, Sequence.Fill([], Interval.UnboundedAbove(1), -7).ToArray());
    }

    [Fact]
    public void CutDefersReadingAndDisposesTheSource()
    {
        var read = false;
        var disposed = false;
        var result = Source().Cut(Interval.Closed(0, 0));
        Assert.False(read);
        using (var iterator = result.GetEnumerator())
        {
            Assert.False(read);
            Assert.True(iterator.MoveNext());
            Assert.True(read);
        }
        Assert.True(disposed);

        IEnumerable<Element> Source()
        {
            read = true;
            try { yield return Point.Origin(); }
            finally { disposed = true; }
        }
    }

    [Fact]
    public void FillDefersReadingAndDisposesTheSource()
    {
        var read = false;
        var disposed = false;
        var result = Sequence.Fill(Source(), Interval.Closed(0, 0));
        Assert.False(read);
        using (var iterator = result.GetEnumerator())
        {
            Assert.False(read);
            Assert.True(iterator.MoveNext());
            Assert.True(read);
        }
        Assert.True(disposed);

        IEnumerable<Element> Source()
        {
            read = true;
            try { yield return Point.Origin(); }
            finally { disposed = true; }
        }
    }
}
