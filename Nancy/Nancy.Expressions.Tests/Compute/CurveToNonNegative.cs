using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CurveToNonNegative
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CurveToNonNegative(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<Curve> TestCases = 
    [
        // Simple curve that goes negative
        new Curve(
            baseSequence: new Sequence([
                new Point(0, -2),
                new Segment(0, 2, -2, 1),
                new Point(2, 0),
                new Segment(2, 4, 0, 1)
            ]),
            pseudoPeriodStart: 2,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 1
        ),
        // Curve that starts positive
        new Curve(
            baseSequence: new Sequence([
                Point.Origin(),
                new Segment(0, 2, 0, 1),
                new Point(2, 2),
                new Segment(2, 3, 2, -1),
                new Point(3, 1),
                new Segment(3, 4, 1, 1)
            ]),
            pseudoPeriodStart: 3,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: 1
        ),
        // Curve with constant negative segment
        new Curve(
            baseSequence: new Sequence([
                new Point(0, -5),
                Segment.Constant(0, 3, -5),
                new Point(3, 0),
                new Segment(3, 5, 0, 1)
            ]),
            pseudoPeriodStart: 3,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 1
        ),
        // RateLatency service curve (already non-negative)
        new RateLatencyServiceCurve(rate: 2, latency: 3),
        // SigmaRho arrival curve
        new SigmaRhoArrivalCurve(sigma: 10, rho: 5),
    ];

    public static IEnumerable<object[]> ToNonNegativeTestCases
        => TestCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ToNonNegativeTestCases))]
    public void ToNonNegativeEquivalence(Curve operand)
    {
        _testOutputHelper.WriteLine($"operand: {operand.ToCodeString()}");

        // Compute through Nancy
        var nancyResult = operand.ToNonNegative();
        _testOutputHelper.WriteLine($"Nancy result: {nancyResult.ToCodeString()}");

        // Compute through Nancy.Expressions
        var expressionOperand = Expressions.FromCurve(operand);
        var toNonNegExp = Expressions.ToNonNegative(expressionOperand);
        var expressionResult = toNonNegExp.Compute();
        _testOutputHelper.WriteLine($"Nancy.Expressions result: {expressionResult.ToCodeString()}");

        // Verify equivalence
        Assert.True(Curve.Equivalent(nancyResult, expressionResult),
            $"Results differ: Nancy={nancyResult.ToCodeString()}, Expressions={expressionResult.ToCodeString()}");
        
        // Verify result is non-negative everywhere
        Assert.True(nancyResult.IsNonNegative,
            $"Result is not non-negative: {nancyResult.ToCodeString()}");
    }
}
