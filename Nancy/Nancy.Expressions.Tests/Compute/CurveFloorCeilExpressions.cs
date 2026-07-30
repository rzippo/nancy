using System.Collections.Generic;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CurveFloorCeilExpressions
{
    public static List<Curve> FloorCeilCases =
    [
        new RateLatencyServiceCurve(rate: 2, latency: 3),
        new SigmaRhoArrivalCurve(sigma: new Rational(5, 2), rho: new Rational(3, 2)),
    ];

    public static IEnumerable<object[]> FloorCeilTestCases
        => FloorCeilCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(FloorCeilTestCases))]
    public void FloorExpressionComputesConcreteFloor(Curve curve)
    {
        var expression = Expressions.Floor(curve.ToExpression());
        var concreteExpression = Expressions.Floor(curve);
        var instanceExpression = curve.ToExpression().Floor();
        var expected = curve.Floor();

        Assert.IsType<FloorExpression>(expression);
        Assert.IsType<FloorExpression>(concreteExpression);
        Assert.IsType<FloorExpression>(instanceExpression);
        Assert.True(Curve.Equivalent(expected, expression.Compute()));
        Assert.True(Curve.Equivalent(expected, concreteExpression.Compute()));
        Assert.True(Curve.Equivalent(expected, instanceExpression.Compute()));
    }

    [Theory]
    [MemberData(nameof(FloorCeilTestCases))]
    public void CeilExpressionComputesConcreteCeiling(Curve curve)
    {
        var expression = Expressions.Ceil(curve.ToExpression());
        var concreteExpression = Expressions.Ceil(curve);
        var instanceExpression = curve.ToExpression().Ceil();
        var expected = curve.Ceil();

        Assert.IsType<CeilExpression>(expression);
        Assert.IsType<CeilExpression>(concreteExpression);
        Assert.IsType<CeilExpression>(instanceExpression);
        Assert.True(Curve.Equivalent(expected, expression.Compute()));
        Assert.True(Curve.Equivalent(expected, concreteExpression.Compute()));
        Assert.True(Curve.Equivalent(expected, instanceExpression.Compute()));
    }

    [Fact]
    public void FloorAndCeilDifferOnFractionalCurve()
    {
        var curve = new SigmaRhoArrivalCurve(sigma: new Rational(5, 2), rho: new Rational(3, 2));
        var expression = curve.ToExpression();

        var floored = expression.Floor().Compute();
        var ceiled = expression.Ceil().Compute();

        Assert.False(Curve.Equivalent(floored, ceiled));
        Assert.True(Curve.Equivalent(curve.Floor(), floored));
        Assert.True(Curve.Equivalent(curve.Ceil(), ceiled));
    }
}
