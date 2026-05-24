using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus.CurveCtorTests;

public class StairCurve
{
    public static List<(Rational a, Rational b)> StairCurveCtorCases =
    [
        (0, 5),
        (3, 7),
        (14.5m, new Rational(20, 3)),
        (5, 1),
        (1, 10),
    ];

    public static IEnumerable<object[]> GetStairCurveCtorCases()
        => StairCurveCtorCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetStairCurveCtorCases))]
    public void StairCurveCtor(Rational a, Rational b)
    {
        var curve = new Unipi.Nancy.NetworkCalculus.StairCurve(a, b);

        Assert.True(curve.IsFinite);
        Assert.Equal(a == 0, curve.IsZero);
        Assert.True(curve.IsLeftContinuous);
        Assert.True(curve.IsUltimatelyPlain);

        Assert.Equal(a, curve.A);
        Assert.Equal(b, curve.B);

        Assert.Equal(0, curve.ValueAt(0));

        if (a > 0)
        {
            Assert.Equal(a, curve.RightLimitAt(0));

            Rational midB = b / 2;
            Assert.Equal(a, curve.ValueAt(midB));
            Assert.Equal(a, curve.ValueAt(b));
            Assert.Equal(a, curve.LeftLimitAt(b));
            Assert.Equal(2 * a, curve.RightLimitAt(b));

            Assert.Equal(2 * a, curve.ValueAt(b + midB));
            Assert.Equal(2 * a, curve.ValueAt(2 * b));
            Assert.Equal(2 * a, curve.LeftLimitAt(2 * b));
            Assert.Equal(3 * a, curve.RightLimitAt(2 * b));

            Assert.Equal(110 * a, curve.ValueAt(110 * b));
        }
    }

    [Fact]
    public void ZeroStep()
    {
        var curve = new Unipi.Nancy.NetworkCalculus.StairCurve(0, 5);

        Assert.True(curve.IsZero);
        Assert.True(curve.IsContinuous);
        Assert.True(curve.IsLeftContinuous);

        Assert.Equal(Rational.PlusInfinity, curve.FirstNonZeroTime);

        Assert.Equal(0, curve.ValueAt(0));
        Assert.Equal(0, curve.ValueAt(3));
        Assert.Equal(0, curve.ValueAt(5));
        Assert.Equal(0, curve.ValueAt(10));
        Assert.Equal(0, curve.ValueAt(110));
    }

    public static List<(Rational a, Rational b)> InvalidArgumentsCases =
    [
        (-1, 5),
        (3, 0),
        (3, -5),
    ];

    public static IEnumerable<object[]> GetInvalidArgumentsCases()
        => InvalidArgumentsCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetInvalidArgumentsCases))]
    public void InvalidArguments(Rational a, Rational b)
    {
        Assert.Throws<ArgumentException>(() => new Unipi.Nancy.NetworkCalculus.StairCurve(a, b));
    }
}
