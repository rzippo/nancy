using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CurveToUpperNonDecreasing
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CurveToUpperNonDecreasing(ITestOutputHelper testOutputHelper)
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

    public static IEnumerable<object[]> ToUpperNonDecreasingTestCases
        => TestCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ToUpperNonDecreasingTestCases))]
    public void ToUpperNonDecreasingEquivalence(Curve operand)
    {
        _testOutputHelper.WriteLine($"operand: {operand.ToCodeString()}");

        // Compute through Nancy
        var nancyResult = operand.ToUpperNonDecreasing();
        _testOutputHelper.WriteLine($"Nancy result: {nancyResult.ToCodeString()}");

        // Compute through Nancy.Expressions
        var expressionOperand = Expressions.FromCurve(operand);
        var toUpperNonDecExp = Expressions.ToUpperNonDecreasing(expressionOperand);
        var expressionResult = toUpperNonDecExp.Compute();
        _testOutputHelper.WriteLine($"Nancy.Expressions result: {expressionResult.ToCodeString()}");

        // Verify equivalence
        Assert.True(Curve.Equivalent(nancyResult, expressionResult),
            $"Results differ: Nancy={nancyResult.ToCodeString()}, Expressions={expressionResult.ToCodeString()}");
        
        // Verify result is non-decreasing
        Assert.True(nancyResult.IsNonDecreasing,
            $"Result is not non-decreasing: {nancyResult.ToCodeString()}");
    }
}
