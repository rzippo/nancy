using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CurveShifts
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CurveShifts(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Curve a, Rational b)> Shifts = [
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,60,35,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 60, pseudoPeriodHeight: 35),
            new Rational(3)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,60,35,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 60, pseudoPeriodHeight: 35),
            new Rational(-3)
        )
    ];

    public static IEnumerable<object[]> ShiftTestCases
        => Shifts.ToXUnitTestCases();

    public static List<(Curve a, Rational b)> PositiveHorizontalShifts = [
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,60,35,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 60, pseudoPeriodHeight: 35),
            new Rational(3)
        ),
        (
            new RateLatencyServiceCurve(rate: 4, latency: 2),
            new Rational(1, 2)
        )
    ];

    public static IEnumerable<object[]> PositiveHorizontalShiftTestCases
        => PositiveHorizontalShifts.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ShiftTestCases))]
    public void ShiftTests(Curve a, Rational b)
    {
        var aExp = a.ToExpression();
        var bExp = b.ToExpression();
        var shiftExp = Expressions.VerticalShift(aExp, bExp);
        _testOutputHelper.WriteLine(shiftExp.ToUnicodeString());
        var subCurve = a.VerticalShift(b, false);
        var shiftExpResult = shiftExp.Compute();
        _testOutputHelper.WriteLine(subCurve.ToCodeString());
        _testOutputHelper.WriteLine(shiftExpResult.ToCodeString());
        Assert.True(Curve.Equivalent(subCurve, shiftExpResult));
    }

    [Theory]
    [MemberData(nameof(PositiveHorizontalShiftTestCases))]
    public void ForwardByTests(Curve a, Rational b)
    {
        var aExp = a.ToExpression();
        var bExp = b.ToExpression();
        var forwardExp = Expressions.ForwardBy(aExp, bExp);
        _testOutputHelper.WriteLine(forwardExp.ToUnicodeString());
        var forwardCurve = a.ForwardBy(b);
        var forwardExpResult = forwardExp.Compute();
        _testOutputHelper.WriteLine(forwardCurve.ToCodeString());
        _testOutputHelper.WriteLine(forwardExpResult.ToCodeString());
        Assert.IsType<ForwardByExpression>(forwardExp);
        Assert.True(Curve.Equivalent(forwardCurve, forwardExpResult));
    }

    [Theory]
    [MemberData(nameof(PositiveHorizontalShiftTestCases))]
    public void ForwardByConcreteOverloadTests(Curve a, Rational b)
    {
        var forwardExp = Expressions.ForwardBy(a, b);
        var forwardCurve = a.ForwardBy(b);
        var forwardExpResult = forwardExp.Compute();

        Assert.IsType<ForwardByExpression>(forwardExp);
        Assert.True(Curve.Equivalent(forwardCurve, forwardExpResult));
    }

    [Theory]
    [MemberData(nameof(PositiveHorizontalShiftTestCases))]
    public void ForwardByInstanceMethodTests(Curve a, Rational b)
    {
        var forwardExp = a.ToExpression().ForwardBy(b);
        var forwardCurve = a.ForwardBy(b);
        var forwardExpResult = forwardExp.Compute();

        Assert.IsType<ForwardByExpression>(forwardExp);
        Assert.True(Curve.Equivalent(forwardCurve, forwardExpResult));
    }

    [Theory]
    [MemberData(nameof(PositiveHorizontalShiftTestCases))]
    public void DelayByExpressionComputesConcreteDelay(Curve a, Rational b)
    {
        var delayExp = Expressions.DelayBy(a.ToExpression(), b.ToExpression());
        var concreteExp = Expressions.DelayBy(a, b);
        var mixedExp = Expressions.DelayBy(a, b.ToExpression());
        var instanceExp = a.ToExpression().DelayBy(b);
        var expected = a.DelayBy(b);

        Assert.IsType<DelayByExpression>(delayExp);
        Assert.IsType<DelayByExpression>(concreteExp);
        Assert.IsType<DelayByExpression>(mixedExp);
        Assert.IsType<DelayByExpression>(instanceExp);
        Assert.True(Curve.Equivalent(expected, delayExp.Compute()));
        Assert.True(Curve.Equivalent(expected, concreteExp.Compute()));
        Assert.True(Curve.Equivalent(expected, mixedExp.Compute()));
        Assert.True(Curve.Equivalent(expected, instanceExp.Compute()));
    }

    [Theory]
    [MemberData(nameof(ShiftTestCases))]
    public void HorizontalShiftTests(Curve a, Rational b)
    {
        var aExp = a.ToExpression();
        var bExp = b.ToExpression();
        var shiftExp = Expressions.HorizontalShift(aExp, bExp);
        _testOutputHelper.WriteLine(shiftExp.ToUnicodeString());
        var shiftCurve = a.HorizontalShift(b);
        var shiftExpResult = shiftExp.Compute();
        _testOutputHelper.WriteLine(shiftCurve.ToCodeString());
        _testOutputHelper.WriteLine(shiftExpResult.ToCodeString());
        Assert.IsType<HorizontalShiftExpression>(shiftExp);
        Assert.True(Curve.Equivalent(shiftCurve, shiftExpResult));
    }

    [Theory]
    [MemberData(nameof(ShiftTestCases))]
    public void HorizontalShiftConcreteOverloadTests(Curve a, Rational b)
    {
        var shiftExp = Expressions.HorizontalShift(a, b);
        var shiftCurve = a.HorizontalShift(b);
        var shiftExpResult = shiftExp.Compute();

        Assert.IsType<HorizontalShiftExpression>(shiftExp);
        Assert.True(Curve.Equivalent(shiftCurve, shiftExpResult));
    }

    [Theory]
    [MemberData(nameof(ShiftTestCases))]
    public void HorizontalShiftInstanceMethodTests(Curve a, Rational b)
    {
        var shiftExp = a.ToExpression().HorizontalShift(b);
        var shiftCurve = a.HorizontalShift(b);
        var shiftExpResult = shiftExp.Compute();

        Assert.IsType<HorizontalShiftExpression>(shiftExp);
        Assert.True(Curve.Equivalent(shiftCurve, shiftExpResult));
    }
    
    [Theory]
    [MemberData(nameof(ShiftTestCases))]
    public void ShiftViaOperatorTests(Curve a, Rational b)
    {
        var aExp = a.ToExpression();
        var bExp = b.ToExpression();
        var shiftExp = aExp + bExp;
        _testOutputHelper.WriteLine(shiftExp.ToUnicodeString());
        var subCurve = a.VerticalShift(b, false);
        var shiftExpResult = shiftExp.Compute();
        _testOutputHelper.WriteLine(subCurve.ToCodeString());
        _testOutputHelper.WriteLine(shiftExpResult.ToCodeString());
        Assert.True(Curve.Equivalent(subCurve, shiftExpResult));
    }

    [Theory]
    [MemberData(nameof(ShiftTestCases))]
    public void VerticalShiftConcreteAndInstanceOverloadsComputeShift(Curve a, Rational b)
    {
        var concreteExp = Expressions.VerticalShift(a, b);
        var mixedExp = Expressions.VerticalShift(a, b.ToExpression());
        var instanceExp = a.ToExpression().VerticalShift(b);
        var expected = a.VerticalShift(b, false);

        Assert.IsType<VerticalShiftExpression>(concreteExp);
        Assert.IsType<VerticalShiftExpression>(mixedExp);
        Assert.IsType<VerticalShiftExpression>(instanceExp);
        Assert.True(Curve.Equivalent(expected, concreteExp.Compute()));
        Assert.True(Curve.Equivalent(expected, mixedExp.Compute()));
        Assert.True(Curve.Equivalent(expected, instanceExp.Compute()));
    }
}
