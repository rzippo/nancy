using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CurveSubtractions
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CurveSubtractions(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Curve a, Curve b, bool nonNegative)> Subtractions = [
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,60,35,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 60, pseudoPeriodHeight: 35),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,1) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            true
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,60,35,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 60, pseudoPeriodHeight: 35),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,1) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            false
        )
    ];

    public static IEnumerable<object[]> SubtractionTestCases
        => Subtractions.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(SubtractionTestCases))]
    public void SubtractionTests(Curve a, Curve b, bool nonNegative)
    {
        var aExp = Expressions.FromCurve(a);
        var bExp = Expressions.FromCurve(b);
        var subExp = Expressions.Subtraction(aExp, bExp, nonNegative);
        _testOutputHelper.WriteLine(subExp.ToUnicodeString());
        var subCurve = Curve.Subtraction(a, b, nonNegative);
        var subExpResult = subExp.Compute();
        _testOutputHelper.WriteLine(subCurve.ToCodeString());
        _testOutputHelper.WriteLine(subExpResult.ToCodeString());
        Assert.True(Curve.Equivalent(subCurve, subExpResult));
    }
}