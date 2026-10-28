using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class ScalarAndShapeProperties
{
    public record ShapeCase(Curve Curve, bool NonPositive, bool Positive, bool Negative, bool Constant, bool Affine);

    private static Curve Line(Rational startValue, Rational slope)
        => new(new Sequence([new Point(0, startValue), new Segment(0, 1, startValue, slope)]), 0, 1, slope);

    public static List<ShapeCase> Shapes =>
    [
        new(Line(0, 0), true, false, false, true, true),
        new(Line(1, 0), false, true, false, true, true),
        new(Line(-1, 0), true, false, true, true, true),
        new(Line(1, 1), false, true, false, false, true),
        new(Line(1, -1), false, false, false, false, true),
        new(Line(-1, -1), true, false, true, false, true),
        new(Line(-1, 1), false, false, false, false, true),
        new(Curve.PlusInfinite(), false, true, false, true, false),
        new(Curve.MinusInfinite(), true, false, true, true, false),
        new(new Curve(new Sequence([new Point(0, 1), new Segment(0, 1, 1, -1)]), 0, 1, 0), false, true, false, false, false),
        new(new Curve(new Sequence([Point.PlusInfinite(0), Segment.Constant(0, 1, 1), new Point(1, 1), Segment.Constant(1, 2, 1)]), 1, 1, 0), false, true, false, false, false)
    ];

    public static IEnumerable<object[]> ShapeCases => Shapes.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ShapeCases))]
    public void IsNonPositiveAccountsForTheTail(ShapeCase item)
        => Assert.Equal(item.NonPositive, item.Curve.IsNonPositive);

    [Theory]
    [MemberData(nameof(ShapeCases))]
    public void IsPositiveAccountsForTheTail(ShapeCase item)
        => Assert.Equal(item.Positive, item.Curve.IsPositive);

    [Theory]
    [MemberData(nameof(ShapeCases))]
    public void IsNegativeAccountsForTheTail(ShapeCase item)
        => Assert.Equal(item.Negative, item.Curve.IsNegative);

    [Theory]
    [MemberData(nameof(ShapeCases))]
    public void IsConstantAccountsForTheTail(ShapeCase item)
        => Assert.Equal(item.Constant, item.Curve.IsConstant);

    [Theory]
    [MemberData(nameof(ShapeCases))]
    public void IsAffineAccountsForTheTail(ShapeCase item)
        => Assert.Equal(item.Affine, item.Curve.IsAffine);

    [Fact]
    public void UltimateShapePredicatesKeepTheirFiniteTailMeaning()
    {
        Assert.False(Curve.PlusInfinite().IsUltimatelyConstant);
        Assert.False(Curve.PlusInfinite().IsUltimatelyAffine);
        Assert.False(Curve.MinusInfinite().IsUltimatelyConstant);
        Assert.False(Curve.MinusInfinite().IsUltimatelyAffine);
        var finiteTail = new Curve(new Sequence([
            Point.PlusInfinite(0), Segment.Constant(0, 1, 1),
            new Point(1, 1), Segment.Constant(1, 2, 1)]), 1, 1, 0);
        Assert.True(finiteTail.IsUltimatelyConstant);
        Assert.True(finiteTail.IsUltimatelyAffine);
        Assert.False(finiteTail.IsFinite);
    }

    public record Window(Curve Curve, Rational Start, Rational? End, bool Left, bool Right,
        bool Increasing, bool NonIncreasing, bool Decreasing, bool Continuous);

    public static List<Window> Windows =>
    [
        new(Line(0, 1), 0, null, true, false, true, false, false, true),
        new(Line(0, -1), 0, null, true, false, false, true, true, true),
        new(Line(1, 0), 0, null, true, false, false, true, false, true),
        new(Line(0, -1), 5, null, false, false, false, true, true, true),
        new(Line(0, 1), 5, 5, true, true, true, true, true, true),
        new(Curve.PlusInfinite(), 0, null, true, false, false, true, false, true),
        new(Curve.MinusInfinite(), 0, null, true, false, false, true, false, true),
        new(new Curve(new Sequence([Point.Origin(), Segment.Constant(0, 1, 1)]), 0, 1, 1), 0, 1, false, false, false, true, false, true),
        new(new Curve(new Sequence([Point.Origin(), Segment.Constant(0, 1, 1)]), 0, 1, 1), 0, 1, true, false, false, false, false, false),
        new(new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1)]), 0, 1, 0), 0, 1, true, false, true, false, false, true),
        new(new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1)]), 0, 1, 0), 0, 1, true, true, false, false, false, false),
        new(new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1)]), 0, 1, 0), 0, null, false, false, false, false, false, false),
        new(new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, -1)]), 0, 1, 0), 0, 1, true, false, false, true, true, true),
        new(new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, -1)]), 0, 1, 0), 0, null, false, false, false, false, false, false)
    ];

    public static IEnumerable<object[]> WindowCases => Windows.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(WindowCases))]
    public void IsIncreasingUsesTheRequestedInterval(Window item)
        => Assert.Equal(item.Increasing, item.Curve.IsIncreasingOverInterval(item.Start, item.End, item.Left, item.Right));

    [Theory]
    [MemberData(nameof(WindowCases))]
    public void IsIncreasingUsesTheRequestedIntervalAfterGlobalEvaluation(Window item)
    {
        _ = item.Curve.IsIncreasing;
        Assert.Equal(item.Increasing, item.Curve.IsIncreasingOverInterval(item.Start, item.End, item.Left, item.Right));
    }

    [Theory]
    [MemberData(nameof(WindowCases))]
    public void IsNonIncreasingUsesTheRequestedInterval(Window item)
        => Assert.Equal(item.NonIncreasing, item.Curve.IsNonIncreasingOverInterval(item.Start, item.End, item.Left, item.Right));

    [Theory]
    [MemberData(nameof(WindowCases))]
    public void IsNonIncreasingUsesTheRequestedIntervalAfterGlobalEvaluation(Window item)
    {
        _ = item.Curve.IsNonIncreasing;
        Assert.Equal(item.NonIncreasing, item.Curve.IsNonIncreasingOverInterval(item.Start, item.End, item.Left, item.Right));
    }

    [Theory]
    [MemberData(nameof(WindowCases))]
    public void IsDecreasingUsesTheRequestedInterval(Window item)
        => Assert.Equal(item.Decreasing, item.Curve.IsDecreasingOverInterval(item.Start, item.End, item.Left, item.Right));

    [Theory]
    [MemberData(nameof(WindowCases))]
    public void IsDecreasingUsesTheRequestedIntervalAfterGlobalEvaluation(Window item)
    {
        _ = item.Curve.IsDecreasing;
        Assert.Equal(item.Decreasing, item.Curve.IsDecreasingOverInterval(item.Start, item.End, item.Left, item.Right));
    }

    [Theory]
    [MemberData(nameof(WindowCases))]
    public void IsContinuousUsesTheRequestedInterval(Window item)
        => Assert.Equal(item.Continuous, item.Curve.IsContinuousOverInterval(item.Start, item.End, item.Left, item.Right));

    [Theory]
    [MemberData(nameof(WindowCases))]
    public void IsContinuousUsesTheRequestedIntervalAfterGlobalEvaluation(Window item)
    {
        _ = item.Curve.IsContinuous;
        Assert.Equal(item.Continuous, item.Curve.IsContinuousOverInterval(item.Start, item.End, item.Left, item.Right));
    }

    public static List<(Rational Start, Rational End, bool Left, bool Right)> InvalidWindows =>
    [
        (-1, 1, true, true), (2, 1, true, true),
        (1, 1, false, false), (1, 1, true, false), (1, 1, false, true),
        (Rational.PlusInfinity, Rational.PlusInfinity, true, true)
    ];

    public static IEnumerable<object[]> InvalidWindowCases => InvalidWindows.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidWindowCases))]
    public void IsIncreasingRejectsInvalidIntervals(Rational start, Rational end, bool left, bool right)
    {
        var curve = Line(0, 0);
        _ = curve.IsIncreasing;
        Assert.Throws<ArgumentException>(() => curve.IsIncreasingOverInterval(start, end, left, right));
    }

    [Theory]
    [MemberData(nameof(InvalidWindowCases))]
    public void IsNonIncreasingRejectsInvalidIntervals(Rational start, Rational end, bool left, bool right)
    {
        var curve = Line(0, 0);
        _ = curve.IsNonIncreasing;
        Assert.Throws<ArgumentException>(() => curve.IsNonIncreasingOverInterval(start, end, left, right));
    }

    [Theory]
    [MemberData(nameof(InvalidWindowCases))]
    public void IsDecreasingRejectsInvalidIntervals(Rational start, Rational end, bool left, bool right)
    {
        var curve = Line(0, 0);
        _ = curve.IsDecreasing;
        Assert.Throws<ArgumentException>(() => curve.IsDecreasingOverInterval(start, end, left, right));
    }

    [Theory]
    [MemberData(nameof(InvalidWindowCases))]
    public void IsContinuousRejectsInvalidIntervals(Rational start, Rational end, bool left, bool right)
    {
        var curve = Line(0, 0);
        _ = curve.IsContinuous;
        Assert.Throws<ArgumentException>(() => curve.IsContinuousOverInterval(start, end, left, right));
    }

    [Fact]
    public void PairedExtremaSeparateUnboundedValuesFromTheirTime()
    {
        Assert.Equal((Rational.PlusInfinity, Rational.PlusInfinity), Line(0, 1).SupValueAndArg());
        Assert.Equal((Rational.MinusInfinity, Rational.PlusInfinity), Line(0, -1).InfValueAndArg());
        Assert.Equal(((Rational) 1, (Rational) 0), Line(1, -1).SupValueAndArg());
        Assert.Equal(((Rational) (-1), (Rational) 0), Line(-1, 1).InfValueAndArg());
    }

    [Fact]
    public void InfiniteConstantExtremaHaveAFiniteLocation()
    {
        Assert.Equal((Rational.PlusInfinity, (Rational) 0), Curve.PlusInfinite().SupValueAndArg());
        Assert.Equal((Rational.PlusInfinity, (Rational) 0), Curve.PlusInfinite().InfValueAndArg());
        Assert.Equal((Rational.MinusInfinity, (Rational) 0), Curve.MinusInfinite().SupValueAndArg());
        Assert.Equal((Rational.MinusInfinity, (Rational) 0), Curve.MinusInfinite().InfValueAndArg());
    }
}
