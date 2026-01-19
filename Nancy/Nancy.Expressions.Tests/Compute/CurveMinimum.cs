using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CurveMinimum
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CurveMinimum(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Curve a, Curve b)> TestCases = 
    [
        // Two rate-latency service curves
        (
            new RateLatencyServiceCurve(rate: 2, latency: 3),
            new RateLatencyServiceCurve(rate: 3, latency: 2)
        ),
        // Two sigma-rho arrival curves
        (
            new SigmaRhoArrivalCurve(sigma: 10, rho: 5),
            new SigmaRhoArrivalCurve(sigma: 15, rho: 8)
        ),
        // Mixed curve types
        (
            new RateLatencyServiceCurve(rate: 1, latency: 10),
            new SigmaRhoArrivalCurve(sigma: 5, rho: 2)
        ),
        // Custom curves with different behaviors
        (
            new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 10, 0, 1),
                    new Point(10, 10),
                    new Segment(10, 20, 10, 0)
                ]),
                pseudoPeriodStart: 10,
                pseudoPeriodLength: 10,
                pseudoPeriodHeight: 0
            ),
            new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 5, 0, 2),
                    new Point(5, 10),
                    new Segment(5, 15, 10, 0)
                ]),
                pseudoPeriodStart: 5,
                pseudoPeriodLength: 10,
                pseudoPeriodHeight: 0
            )
        ),
        // Service curves with different rates
        (
            new RateLatencyServiceCurve(rate: 5, latency: 1),
            new RateLatencyServiceCurve(rate: 2, latency: 5)
        ),
    ];

    public static IEnumerable<object[]> MinimumTestCases
        => TestCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(MinimumTestCases))]
    public void MinimumEquivalence(Curve a, Curve b)
    {
        _testOutputHelper.WriteLine($"curve a: {a.ToCodeString()}");
        _testOutputHelper.WriteLine($"curve b: {b.ToCodeString()}");

        // Compute through Nancy
        var nancyResult = Curve.Minimum(a, b);
        _testOutputHelper.WriteLine($"Nancy result: {nancyResult.ToCodeString()}");

        // Compute through Nancy.Expressions
        var aExp = Expressions.FromCurve(a);
        var bExp = Expressions.FromCurve(b);
        var minExp = Expressions.Minimum(aExp, bExp);
        var expressionResult = minExp.Compute();
        _testOutputHelper.WriteLine($"Nancy.Expressions result: {expressionResult.ToCodeString()}");

        // Verify equivalence
        Assert.True(Curve.Equivalent(nancyResult, expressionResult),
            $"Results differ: Nancy={nancyResult.ToCodeString()}, Expressions={expressionResult.ToCodeString()}");
    }

    [Theory]
    [MemberData(nameof(MinimumTestCases))]
    public void MinimumMultipleEquivalence(Curve a, Curve b)
    {
        _testOutputHelper.WriteLine($"Testing Minimum with multiple curves");
        _testOutputHelper.WriteLine($"curve a: {a.ToCodeString()}");
        _testOutputHelper.WriteLine($"curve b: {b.ToCodeString()}");

        // Compute through Nancy with list
        var curves = new List<Curve> { a, b };
        var nancyResult = Curve.Minimum(curves);
        _testOutputHelper.WriteLine($"Nancy result: {nancyResult.ToCodeString()}");

        // Compute through Nancy.Expressions with list
        var curveExpressions = new List<CurveExpression> 
        { 
            Expressions.FromCurve(a), 
            Expressions.FromCurve(b) 
        };
        var minExp = Expressions.Minimum(curveExpressions);
        var expressionResult = minExp.Compute();
        _testOutputHelper.WriteLine($"Nancy.Expressions result: {expressionResult.ToCodeString()}");

        // Verify equivalence
        Assert.True(Curve.Equivalent(nancyResult, expressionResult),
            $"Results differ: Nancy={nancyResult.ToCodeString()}, Expressions={expressionResult.ToCodeString()}");
    }
}
