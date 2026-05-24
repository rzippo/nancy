using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CurveSampling
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CurveSampling(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Curve a, Rational b)> CurveTimePairs = [
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,60,35,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 60, pseudoPeriodHeight: 35),
            new Rational(0)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,60,35,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 60, pseudoPeriodHeight: 35),
            new Rational(3)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,60,35,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 60, pseudoPeriodHeight: 35),
            new Rational(5)
        )
    ];

    public static IEnumerable<object[]> ValueAtTestCases
        => CurveTimePairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ValueAtTestCases))]
    public void ValueAt(Curve a, Rational b)
    {
        var aExp = a.ToExpression();
        var bExp = b.ToExpression();
        var valueAtExp = aExp.ValueAt(bExp);
        _testOutputHelper.WriteLine(valueAtExp.ToUnicodeString());
        var valueAtValue = a.ValueAt(b);
        var valueAtExpResult = valueAtExp.Compute();
        _testOutputHelper.WriteLine(valueAtValue.ToCodeString());
        _testOutputHelper.WriteLine(valueAtExpResult.ToCodeString());
        Assert.IsType<ValueAtExpression>(valueAtExp);
        Assert.Equivalent(valueAtValue, valueAtExpResult);
    }

    [Theory]
    [MemberData(nameof(ValueAtTestCases))]
    public void ValueAtConcreteOverloads(Curve a, Rational b)
    {
        var expressionWithRational = new ValueAtExpression(a, "a", b);
        var expressionWithRationalExpression = new ValueAtExpression(a, "a", b.ToExpression());
        var instanceExpressionWithRational = a.ToExpression().ValueAt(b);
        var expected = a.ValueAt(b);

        Assert.IsType<ValueAtExpression>(expressionWithRational);
        Assert.IsType<ValueAtExpression>(expressionWithRationalExpression);
        Assert.IsType<ValueAtExpression>(instanceExpressionWithRational);
        Assert.Equal(expected, expressionWithRational.Compute());
        Assert.Equal(expected, expressionWithRationalExpression.Compute());
        Assert.Equal(expected, instanceExpressionWithRational.Compute());
    }
    
    public static IEnumerable<object[]> LeftLimitAtTestCases
        => CurveTimePairs
            .Where(p => p.b > 0)
            .ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(LeftLimitAtTestCases))]
    public void LeftLimitAt(Curve a, Rational b)
    {
        var aExp = a.ToExpression();
        var bExp = b.ToExpression();
        var leftLimitAtExp = aExp.LeftLimitAt(bExp);
        _testOutputHelper.WriteLine(leftLimitAtExp.ToUnicodeString());
        var leftLimitAtValue = a.LeftLimitAt(b);
        var leftLimitAtExpResult = leftLimitAtExp.Compute();
        _testOutputHelper.WriteLine(leftLimitAtValue.ToCodeString());
        _testOutputHelper.WriteLine(leftLimitAtExpResult.ToCodeString());
        Assert.IsType<LeftLimitAtExpression>(leftLimitAtExp);
        Assert.Equivalent(leftLimitAtValue, leftLimitAtExpResult);
    }

    [Theory]
    [MemberData(nameof(LeftLimitAtTestCases))]
    public void LeftLimitAtConcreteOverloads(Curve a, Rational b)
    {
        var expressionWithRational = new LeftLimitAtExpression(a, "a", b);
        var expressionWithRationalExpression = new LeftLimitAtExpression(a, "a", b.ToExpression());
        var instanceExpressionWithRational = a.ToExpression().LeftLimitAt(b);
        var expected = a.LeftLimitAt(b);

        Assert.IsType<LeftLimitAtExpression>(expressionWithRational);
        Assert.IsType<LeftLimitAtExpression>(expressionWithRationalExpression);
        Assert.IsType<LeftLimitAtExpression>(instanceExpressionWithRational);
        Assert.Equal(expected, expressionWithRational.Compute());
        Assert.Equal(expected, expressionWithRationalExpression.Compute());
        Assert.Equal(expected, instanceExpressionWithRational.Compute());
    }
    
    public static IEnumerable<object[]> RightLimitAtTestCases
        => CurveTimePairs.ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(RightLimitAtTestCases))]
    public void RightLimitAt(Curve a, Rational b)
    {
        var aExp = a.ToExpression();
        var bExp = b.ToExpression();
        var rightLimitAtExp = aExp.RightLimitAt(bExp);
        _testOutputHelper.WriteLine(rightLimitAtExp.ToUnicodeString());
        var rightLimitAtValue = a.RightLimitAt(b);
        var rightLimitAtExpResult = rightLimitAtExp.Compute();
        _testOutputHelper.WriteLine(rightLimitAtValue.ToCodeString());
        _testOutputHelper.WriteLine(rightLimitAtExpResult.ToCodeString());
        Assert.IsType<RightLimitAtExpression>(rightLimitAtExp);
        Assert.Equivalent(rightLimitAtValue, rightLimitAtExpResult);
    }

    [Theory]
    [MemberData(nameof(RightLimitAtTestCases))]
    public void RightLimitAtConcreteOverloads(Curve a, Rational b)
    {
        var expressionWithRational = new RightLimitAtExpression(a, "a", b);
        var expressionWithRationalExpression = new RightLimitAtExpression(a, "a", b.ToExpression());
        var instanceExpressionWithRational = a.ToExpression().RightLimitAt(b);
        var expected = a.RightLimitAt(b);

        Assert.IsType<RightLimitAtExpression>(expressionWithRational);
        Assert.IsType<RightLimitAtExpression>(expressionWithRationalExpression);
        Assert.IsType<RightLimitAtExpression>(instanceExpressionWithRational);
        Assert.Equal(expected, expressionWithRational.Compute());
        Assert.Equal(expected, expressionWithRationalExpression.Compute());
        Assert.Equal(expected, instanceExpressionWithRational.Compute());
    }

}
