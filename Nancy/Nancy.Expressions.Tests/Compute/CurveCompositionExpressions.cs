using System.Collections.Generic;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CurveCompositionExpressions
{
    public static List<(Curve outer, Curve inner)> CompositionCases =
    [
        (
            new Curve(
                new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, 2)
                ]),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 2
            ),
            new Curve(
                new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, 3)
                ]),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 3
            )
        ),
        (
            new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 2, 0),
                    new Point(2, 0),
                    new Segment(2, 3, 0, 4)
                ]),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 4
            ),
            new Curve(
                new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    Segment.Constant(1, 2, 1)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            )
        ),
    ];

    public static IEnumerable<object[]> CompositionTestCases
        => CompositionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(CompositionTestCases))]
    public void CompositionExpressionComputesConcreteOperation(Curve outer, Curve inner)
    {
        var expression = Expressions.Composition(outer.ToExpression(), inner.ToExpression());
        var concreteExpression = Expressions.Composition(outer, inner);
        var leftConcreteExpression = Expressions.Composition(outer, inner.ToExpression());
        var rightConcreteExpression = Expressions.Composition(outer.ToExpression(), inner);
        var instanceExpression = outer.ToExpression().Composition(inner);
        var expected = Curve.Composition(outer, inner);

        Assert.IsType<CompositionExpression>(expression);
        Assert.IsType<CompositionExpression>(concreteExpression);
        Assert.IsType<CompositionExpression>(leftConcreteExpression);
        Assert.IsType<CompositionExpression>(rightConcreteExpression);
        Assert.IsType<CompositionExpression>(instanceExpression);
        Assert.True(Curve.Equivalent(expected, expression.Compute()));
        Assert.True(Curve.Equivalent(expected, concreteExpression.Compute()));
        Assert.True(Curve.Equivalent(expected, leftConcreteExpression.Compute()));
        Assert.True(Curve.Equivalent(expected, rightConcreteExpression.Compute()));
        Assert.True(Curve.Equivalent(expected, instanceExpression.Compute()));
    }
}
