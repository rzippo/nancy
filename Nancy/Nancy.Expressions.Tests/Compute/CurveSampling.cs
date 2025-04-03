using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

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
        Assert.Equivalent(valueAtValue, valueAtExpResult);
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
        Assert.Equivalent(leftLimitAtValue, leftLimitAtExpResult);
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
        Assert.Equivalent(rightLimitAtValue, rightLimitAtExpResult);
    }

}