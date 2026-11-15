using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class InfiniteTailCut
{
    public static IEnumerable<(Curve Curve, Sequence Expected)> InfiniteTails()
    {
        foreach (var value in new[] { Rational.PlusInfinity, Rational.MinusInfinity })
        foreach (var height in new[] { Rational.Zero, value })
        {
            var curve = new Curve(new Sequence([
                new Point(0, 7), new Segment(0, 1, 2, 3),
                new Point(1, 9), Segment.Constant(1, 2, value),
                new Point(2, value), Segment.Constant(2, 3, value)
            ]), 2, 1, height);
            var expected = new Sequence([
                new Point(new Rational(1, 2), new Rational(7, 2)),
                new Segment(new Rational(1, 2), 1, new Rational(7, 2), 3),
                new Point(1, 9), Segment.Constant(1, new Rational(15, 2), value),
                new Point(new Rational(15, 2), value)
            ]);
            yield return (curve, expected);
        }
    }

    public static IEnumerable<object[]> Cases() => InfiniteTails().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void CutPreservesTheTransientAndInfiniteTail(Curve curve, Sequence expected)
    {
        var actual = curve.Cut(new Rational(1, 2), new Rational(15, 2), true, true);
        Assert.True(Sequence.Equivalent(expected, actual));
        Assert.Equal(expected.Count, actual.Count);
        var tail = curve.Cut(new Rational(7, 2), new Rational(15, 2), false, false);
        Assert.Equal(Segment.Constant(new Rational(7, 2), new Rational(15, 2), expected.ValueAt(2)), Assert.Single(tail.Elements));
        Assert.True(curve.IsUltimatelyInfinite);
        Assert.Equal(expected.ValueAt(2).IsPlusInfinite, curve.IsUltimatelyPlusInfinite);
        Assert.Equal(expected.ValueAt(2).IsMinusInfinite, curve.IsUltimatelyMinusInfinite);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void StreamedCutPreservesTheTransientAndInfiniteTail(Curve curve, Sequence expected)
    {
        var actual = curve.CutAsEnumerable(new Rational(1, 2), new Rational(15, 2), true, true)
            .ToSequence();
        Assert.True(Sequence.Equivalent(expected, actual));
        Assert.Equal(expected.Count, actual.Count);
        var tail = curve.CutAsEnumerable(new Rational(7, 2), new Rational(15, 2), false, false);
        Assert.Equal(Segment.Constant(new Rational(7, 2), new Rational(15, 2), expected.ValueAt(2)), Assert.Single(tail));
        Assert.True(curve.IsUltimatelyInfinite);
        Assert.Equal(expected.ValueAt(2).IsPlusInfinite, curve.IsUltimatelyPlusInfinite);
        Assert.Equal(expected.ValueAt(2).IsMinusInfinite, curve.IsUltimatelyMinusInfinite);
    }

    private static (Curve Curve, Sequence Expected) RepeatedFinitePoints()
    {
        var curve = new Curve(new Sequence([new Point(0, 3), Segment.PlusInfinite(0, 1)]), 0, 1, 0);
        var expected = new Sequence([
            new Point(0, 3), Segment.PlusInfinite(0, 1),
            new Point(1, 3), Segment.PlusInfinite(1, 2),
            new Point(2, 3), Segment.PlusInfinite(2, 3), new Point(3, 3)
        ]);
        return (curve, expected);
    }

    [Fact]
    public void CutPreservesRepeatedFinitePoints()
    {
        var (curve, expected) = RepeatedFinitePoints();
        Assert.False(curve.IsUltimatelyPlusInfinite);
        Assert.False(curve.IsUltimatelyMinusInfinite);
        Assert.False(curve.IsUltimatelyInfinite);
        Assert.True(Sequence.Equivalent(expected, curve.Cut(0, 3, true, true)));
    }

    [Fact]
    public void StreamedCutPreservesRepeatedFinitePoints()
    {
        var (curve, expected) = RepeatedFinitePoints();
        Assert.False(curve.IsUltimatelyPlusInfinite);
        Assert.False(curve.IsUltimatelyMinusInfinite);
        Assert.False(curve.IsUltimatelyInfinite);
        Assert.True(Sequence.Equivalent(expected, curve.CutAsEnumerable(0, 3, true, true).ToSequence()));
    }
}
