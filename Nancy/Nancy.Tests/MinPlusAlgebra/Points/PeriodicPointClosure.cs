using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Points;

public class PeriodicPointClosure
{
    [Fact]
    public void CaseA_1()
    {
        Point point = new Point(
            time: 1,
            value: 2
        );
        Rational c = 1;
        Rational d = 2;

        Curve closure = point.SubAdditiveClosure(
            pseudoPeriodLength: d, 
            pseudoPeriodHeight: c);

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(2, closure.ValueAt(1));
        Assert.Equal(4, closure.ValueAt(2));
        Assert.Equal(3, closure.ValueAt(3));
        Assert.Equal(6, closure.ValueAt(6));
        Assert.Equal(5, closure.ValueAt(7));

        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(0.5m));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(1.5m));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(2.5m));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(5.5m));
    }

    [Fact]
    public void CaseB_1()
    {
        Point point = new Point(
            time: 1,
            value: 1
        );
        Rational c = 3;
        Rational d = 2;

        Curve closure = point.SubAdditiveClosure(
            pseudoPeriodLength: d,
            pseudoPeriodHeight: c);

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(1, closure.ValueAt(1));
        Assert.Equal(2, closure.ValueAt(2));
        Assert.Equal(3, closure.ValueAt(3));
        Assert.Equal(7, closure.ValueAt(7));

        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(0.5m));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(1.2m));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(2.7m));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(5.95m));
    }

    [Fact]
    public void CaseC_1()
    {
        Point point = new Point(
            time: 8,
            value: 16
        );
        Rational c = 12;
        Rational d = 6;

        Curve closure = point.SubAdditiveClosure(
            pseudoPeriodLength: d,
            pseudoPeriodHeight: c);

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(16, closure.ValueAt(8));
        Assert.Equal(28, closure.ValueAt(14));
        Assert.Equal(32, closure.ValueAt(16));
        Assert.Equal(40, closure.ValueAt(20));
        Assert.Equal(40, closure.ValueAt(20));
        Assert.Equal(48, closure.ValueAt(24));
        Assert.Equal(52, closure.ValueAt(26));
        Assert.Equal(60, closure.ValueAt(30));
        Assert.Equal(64, closure.ValueAt(32));

        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(3.5m));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(10));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(12));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(12.2m));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(18));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(18.7m));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(25.95m));

    }

    public static IEnumerable<object[]> GetTestCases()
    {
        var testCases = new (Rational value, Rational length, Rational step)[]
        {
            (0, 5, 3),
            (2, 5, 3),
            (5, 3, 7),
            (Rational.PlusInfinity, 5, 3),
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] { testCase.value, testCase.length, testCase.step };
        }
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Case0(Rational value, Rational length, Rational step)
    {
        var point = new Point(time: 0, value: value);
        var closure = point.SubAdditiveClosure(
            pseudoPeriodLength: length, 
            pseudoPeriodHeight: step);

        Assert.False(closure.IsContinuous);
        Assert.False(closure.IsFinite);

        if (value.IsFinite)
        {
            Assert.False(closure.IsUltimatelyPlain);
            Assert.Equal(0, closure.FirstFiniteTime);
            Assert.Equal(length, closure.FirstFiniteTimeExceptOrigin);
        }
        else
        {
            Assert.True(closure.IsUltimatelyPlain);
            Assert.Equal(0, closure.FirstFiniteTime);
            Assert.Equal(Rational.PlusInfinity, closure.FirstFiniteTimeExceptOrigin);
        }


        Assert.Equal(0, closure.ValueAt(0));

        for (int i = 1; i < 5; i++)
        {
            Assert.Equal(value + i*step, closure.ValueAt(i*length));
        }

        Assert.Equal(length, closure.PseudoPeriodStart);
        Assert.Equal(length, closure.PseudoPeriodLength);
        Assert.Equal(step, closure.PseudoPeriodHeight);
    }
}