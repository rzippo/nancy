using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CurveToLowerNonIncreasing
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CurveToLowerNonIncreasing(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<Curve> TestCases = 
    [
        // Simple curve with a dip
        new Curve(
            baseSequence: new Sequence([
                Point.Origin(),
                new Segment(0, 2, 0, 1),
                new Point(2, 2),
                new Segment(2, 3, 2, -1),
                new Point(3, 1),
                new Segment(3, 6, 1, 1),
                new Point(6, 4),
                new Segment(6, 7, 4, 1)
            ]),
            pseudoPeriodStart: 6,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: 1
        ),
        // Curve with multiple dips
        new Curve(
            baseSequence: new Sequence([
                Point.Origin(),
                new Segment(0, 2, 0, 1),
                new Point(2, 2),
                new Segment(2, 4, 1, 1),
                new Point(4, 3),
                new Segment(4, 5, 3, 1)
            ]),
            pseudoPeriodStart: 4,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: 1
        ),
        // RateLatency service curve (already non-decreasing)
        new RateLatencyServiceCurve(rate: 2, latency: 3),
        // SigmaRho arrival curve (already non-decreasing)
        new SigmaRhoArrivalCurve(sigma: 10, rho: 5),
        // Periodic curve with negative slope in period
        new Curve(
            baseSequence: new Sequence([
                Point.Origin(),
                Segment.Constant(0, 1, 300),
                new Point(1, 100),
                Segment.Constant(1, 2, 400),
            ]),
            pseudoPeriodStart: 1,
            pseudoPeriodHeight: 100,
            pseudoPeriodLength: 1
        ),
    ];

    public static IEnumerable<object[]> ToLowerNonIncreasingTestCases
        => TestCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ToLowerNonIncreasingTestCases))]
    public void ToLowerNonIncreasingEquivalence(Curve operand)
    {
        _testOutputHelper.WriteLine($"operand: {operand.ToCodeString()}");

        // Compute through Nancy
        var nancyResult = operand.ToLowerNonIncreasing();
        _testOutputHelper.WriteLine($"Nancy result: {nancyResult.ToCodeString()}");

        // Compute through Nancy.Expressions
        var expressionOperand = Expressions.FromCurve(operand);
        var toLowerNonIncExp = Expressions.ToLowerNonIncreasing(expressionOperand);
        var expressionResult = toLowerNonIncExp.Compute();
        _testOutputHelper.WriteLine($"Nancy.Expressions result: {expressionResult.ToCodeString()}");

        // Verify equivalence
        Assert.True(Curve.Equivalent(nancyResult, expressionResult),
            $"Results differ: Nancy={nancyResult.ToCodeString()}, Expressions={expressionResult.ToCodeString()}");
        
        // Verify result is non-increasing
        Assert.True(nancyResult.Negate().IsNonDecreasing,
            $"Result is not non-increasing: {nancyResult.ToCodeString()}");
    }

    [Theory]
    [MemberData(nameof(ToLowerNonIncreasingTestCases))]
    public void ToLowerNonIncreasingConcreteAndInstanceOverloadsComputeProjection(Curve operand)
    {
        var expected = operand.ToLowerNonIncreasing();
        var concreteExpression = Expressions.ToLowerNonIncreasing(operand);
        var instanceExpression = operand.ToExpression().ToLowerNonIncreasing();

        Assert.IsType<ToLowerNonIncreasingExpression>(concreteExpression);
        Assert.IsType<ToLowerNonIncreasingExpression>(instanceExpression);
        Assert.True(Curve.Equivalent(expected, concreteExpression.Compute()));
        Assert.True(Curve.Equivalent(expected, instanceExpression.Compute()));
    }
}
