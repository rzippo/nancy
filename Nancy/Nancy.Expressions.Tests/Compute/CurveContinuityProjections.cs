using System.Collections.Generic;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CurveContinuityProjections
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CurveContinuityProjections(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<Curve> TestCases =
    [
        new Curve(
            baseSequence: new Sequence([
                Point.Origin(),
                Segment.Constant(0, 1, 2),
                new Point(1, 9),
                Segment.Constant(1, 2, 4)
            ]),
            pseudoPeriodStart: 1,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: 0
        ),
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
        new RateLatencyServiceCurve(rate: 2, latency: 3),
        new SigmaRhoArrivalCurve(sigma: 10, rho: 5),
    ];

    public static IEnumerable<object[]> ContinuityProjectionTestCases
        => TestCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ContinuityProjectionTestCases))]
    public void ToLeftContinuousExpressionComputesConcreteProjection(Curve operand)
    {
        _testOutputHelper.WriteLine($"operand: {operand.ToCodeString()}");

        var nancyResult = operand.ToLeftContinuous();
        var expressionOperand = Expressions.FromCurve(operand);
        var expression = Expressions.ToLeftContinuous(expressionOperand);
        var expressionResult = expression.Compute();

        _testOutputHelper.WriteLine($"Nancy result: {nancyResult.ToCodeString()}");
        _testOutputHelper.WriteLine($"Nancy.Expressions result: {expressionResult.ToCodeString()}");

        Assert.IsType<ToLeftContinuousExpression>(expression);
        Assert.True(expression.IsLeftContinuous);
        Assert.True(expressionResult.IsLeftContinuous);
        Assert.True(Curve.Equivalent(nancyResult, expressionResult),
            $"Results differ: Nancy={nancyResult.ToCodeString()}, Expressions={expressionResult.ToCodeString()}");
    }

    [Theory]
    [MemberData(nameof(ContinuityProjectionTestCases))]
    public void ToLeftContinuousConcreteOverloadComputesConcreteProjection(Curve operand)
    {
        var nancyResult = operand.ToLeftContinuous();
        var expressionResult = Expressions.ToLeftContinuous(operand).Compute();

        Assert.True(expressionResult.IsLeftContinuous);
        Assert.True(Curve.Equivalent(nancyResult, expressionResult),
            $"Results differ: Nancy={nancyResult.ToCodeString()}, Expressions={expressionResult.ToCodeString()}");
    }

    [Theory]
    [MemberData(nameof(ContinuityProjectionTestCases))]
    public void ToLeftContinuousInstanceMethodComputesConcreteProjection(Curve operand)
    {
        var nancyResult = operand.ToLeftContinuous();
        var expressionResult = operand.ToExpression().ToLeftContinuous().Compute();

        Assert.True(expressionResult.IsLeftContinuous);
        Assert.True(Curve.Equivalent(nancyResult, expressionResult),
            $"Results differ: Nancy={nancyResult.ToCodeString()}, Expressions={expressionResult.ToCodeString()}");
    }

    [Theory]
    [MemberData(nameof(ContinuityProjectionTestCases))]
    public void ToRightContinuousExpressionComputesConcreteProjection(Curve operand)
    {
        _testOutputHelper.WriteLine($"operand: {operand.ToCodeString()}");

        var nancyResult = operand.ToRightContinuous();
        var expressionOperand = Expressions.FromCurve(operand);
        var expression = Expressions.ToRightContinuous(expressionOperand);
        var expressionResult = expression.Compute();

        _testOutputHelper.WriteLine($"Nancy result: {nancyResult.ToCodeString()}");
        _testOutputHelper.WriteLine($"Nancy.Expressions result: {expressionResult.ToCodeString()}");

        Assert.IsType<ToRightContinuousExpression>(expression);
        Assert.True(expression.IsRightContinuous);
        Assert.True(expressionResult.IsRightContinuous);
        Assert.True(Curve.Equivalent(nancyResult, expressionResult),
            $"Results differ: Nancy={nancyResult.ToCodeString()}, Expressions={expressionResult.ToCodeString()}");
    }

    [Theory]
    [MemberData(nameof(ContinuityProjectionTestCases))]
    public void ToRightContinuousConcreteOverloadComputesConcreteProjection(Curve operand)
    {
        var nancyResult = operand.ToRightContinuous();
        var expressionResult = Expressions.ToRightContinuous(operand).Compute();

        Assert.True(expressionResult.IsRightContinuous);
        Assert.True(Curve.Equivalent(nancyResult, expressionResult),
            $"Results differ: Nancy={nancyResult.ToCodeString()}, Expressions={expressionResult.ToCodeString()}");
    }

    [Theory]
    [MemberData(nameof(ContinuityProjectionTestCases))]
    public void ToRightContinuousInstanceMethodComputesConcreteProjection(Curve operand)
    {
        var nancyResult = operand.ToRightContinuous();
        var expressionResult = operand.ToExpression().ToRightContinuous().Compute();

        Assert.True(expressionResult.IsRightContinuous);
        Assert.True(Curve.Equivalent(nancyResult, expressionResult),
            $"Results differ: Nancy={nancyResult.ToCodeString()}, Expressions={expressionResult.ToCodeString()}");
    }
}
