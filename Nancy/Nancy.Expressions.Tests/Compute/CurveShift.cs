using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

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
}