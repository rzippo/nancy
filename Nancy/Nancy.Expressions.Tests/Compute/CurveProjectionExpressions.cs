using System.Collections.Generic;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CurveProjectionExpressions
{
    public static List<Curve> ProjectionCases =
    [
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
        new Curve(
            baseSequence: new Sequence([
                new Point(0, 5),
                Segment.Constant(0, 2, 5),
                new Point(2, 3),
                new Segment(2, 4, 3, 1)
            ]),
            pseudoPeriodStart: 2,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 1
        ),
        new RateLatencyServiceCurve(rate: 2, latency: 3),
    ];

    public static IEnumerable<object[]> ProjectionTestCases
        => ProjectionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ProjectionTestCases))]
    public void ToLowerNonDecreasingExpressionComputesConcreteProjection(Curve curve)
    {
        var expression = Expressions.ToLowerNonDecreasing(curve.ToExpression());
        var concreteExpression = Expressions.ToLowerNonDecreasing(curve);
        var instanceExpression = curve.ToExpression().ToLowerNonDecreasing();
        var expected = curve.ToLowerNonDecreasing();

        Assert.IsType<ToLowerNonDecreasingExpression>(expression);
        Assert.IsType<ToLowerNonDecreasingExpression>(concreteExpression);
        Assert.IsType<ToLowerNonDecreasingExpression>(instanceExpression);
        Assert.True(expression.IsNonDecreasing);
        Assert.True(Curve.Equivalent(expected, expression.Compute()));
        Assert.True(Curve.Equivalent(expected, concreteExpression.Compute()));
        Assert.True(Curve.Equivalent(expected, instanceExpression.Compute()));
    }

    [Theory]
    [MemberData(nameof(ProjectionTestCases))]
    public void WithZeroOriginExpressionComputesConcreteProjection(Curve curve)
    {
        var expression = Expressions.WithZeroOrigin(curve.ToExpression());
        var concreteExpression = Expressions.WithZeroOrigin(curve);
        var instanceExpression = curve.ToExpression().WithZeroOrigin();
        var expected = curve.WithZeroOrigin();

        Assert.IsType<WithZeroOriginExpression>(expression);
        Assert.IsType<WithZeroOriginExpression>(concreteExpression);
        Assert.IsType<WithZeroOriginExpression>(instanceExpression);
        Assert.True(expression.IsZeroAtZero);
        Assert.True(Curve.Equivalent(expected, expression.Compute()));
        Assert.True(Curve.Equivalent(expected, concreteExpression.Compute()));
        Assert.True(Curve.Equivalent(expected, instanceExpression.Compute()));
    }
}
