using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves.Streams;

public class Extension
{
    private static readonly ComputationSettings Serial = new() { UseParallelism = false };

    public static List<Curve> Curves =>
    [
        new StairCurve(3, 2),
        -new StairCurve(3, 2),
        new Curve(new Sequence([
            new Point(0, 0), Segment.Constant(0, 1, 1),
            new Point(1, 2), Segment.Constant(1, 2, 2),
            new Point(2, 3), Segment.Constant(2, 3, 3)]), 1, 2, 1),
        new Curve(new Sequence([
            new Point(0, 0), Segment.Constant(0, 1, 1),
            Point.PlusInfinite(1), Segment.PlusInfinite(1, 2),
            new Point(2, 2), Segment.Constant(2, 3, 2)]), 1, 2, 0),
        new RateLatencyServiceCurve(3, 2)
    ];

    public static IEnumerable<(Curve Curve, Interval Window)> WindowCases()
    {
        foreach (var curve in Curves)
        foreach (var start in new[]
                 {
                     Rational.Zero, curve.PseudoPeriodStart + curve.PseudoPeriodLength / 2,
                     curve.FirstPseudoPeriodEnd, curve.SecondPseudoPeriodEnd + curve.PseudoPeriodLength / 2
                 })
        foreach (var includeStart in new[] { false, true })
        foreach (var includeEnd in new[] { false, true })
            yield return (curve, new Interval(start, start + 3 * curve.PseudoPeriodLength, includeStart, includeEnd));
    }

    public static IEnumerable<object[]> Windows => WindowCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Windows))]
    public void RepeatedPeriodsMatchTheMaterializedExtension(Curve curve, Interval window)
    {
        var expected = curve.Extend(window.Upper + curve.PseudoPeriodLength, Serial)
            .Cut(window);
        var actual = curve.CutAsEnumerable(window, Serial)
            .MergeAsEnumerable()
            .ToSequence();
        Assert.True(Sequence.Equivalent(expected, actual));
        foreach (var element in actual.Elements)
        {
            if (element is Point point)
                Assert.Equal(curve.ValueAt(point.Time, Serial), point.Value);
            else if (element is Segment segment)
            {
                Assert.Equal(curve.RightLimitAt(segment.StartTime), segment.RightLimitAtStartTime);
                Assert.Equal(curve.LeftLimitAt(segment.EndTime), segment.LeftLimitAtEndTime);
            }
        }
    }

    [Theory]
    [MemberData(nameof(Windows))]
    public void InterleavedEnumerationsHaveIndependentPositions(Curve curve, Interval window)
    {
        var stream = curve.CutAsEnumerable(window, Serial);
        using var first = stream.GetEnumerator();
        using var second = stream.GetEnumerator();
        Assert.True(first.MoveNext());
        var expectedFirst = first.Current;
        var hasSecondElement = first.MoveNext();
        Assert.True(second.MoveNext());
        Assert.Equal(expectedFirst, second.Current);
        Assert.Equal(hasSecondElement, second.MoveNext());
        if (hasSecondElement)
            Assert.Equal(first.Current, second.Current);
        while (first.MoveNext())
        {
            Assert.True(second.MoveNext());
            Assert.Equal(first.Current, second.Current);
        }
        Assert.False(second.MoveNext());
    }

    [Fact]
    public void ALongExtensionCanStopAtItsFirstPeriodBoundary()
    {
        var curve = new StairCurve(3, 2);
        var first = curve.CutAsEnumerable(0, 1_000_000, settings: Serial)
            .Take(3)
            .ToArray();
        Assert.Equal<Element>([new Point(0, 0), Segment.Constant(0, 2, 3), new Point(2, 3)], first);
    }
}
