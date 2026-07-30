using System;
using System.Collections.Generic;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CurveExtremaExpressions
{
    public static List<Curve> ExtremaCases =
    [
        new RateLatencyServiceCurve(rate: 2, latency: 3),
        new SigmaRhoArrivalCurve(sigma: 2, rho: 1),
        new ConstantCurve(7),
    ];

    public static IEnumerable<object[]> ExtremaTestCases
        => ExtremaCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ExtremaTestCases))]
    public void SupValueExpressionMatchesConcreteSupValue(Curve curve)
    {
        var expression = Expressions.SupValue(curve.ToExpression());
        var concreteExpression = Expressions.SupValue(curve);
        var instanceExpression = curve.ToExpression().SupValue();
        var expected = curve.SupValue();

        Assert.IsType<SupValueExpression>(expression);
        Assert.IsType<SupValueExpression>(concreteExpression);
        Assert.IsType<SupValueExpression>(instanceExpression);
        Assert.Equal(expected, expression.Compute());
        Assert.Equal(expected, concreteExpression.Compute());
        Assert.Equal(expected, instanceExpression.Compute());
    }

    [Theory]
    [MemberData(nameof(ExtremaTestCases))]
    public void InfValueExpressionMatchesConcreteInfValue(Curve curve)
    {
        var expression = Expressions.InfValue(curve.ToExpression());
        var concreteExpression = Expressions.InfValue(curve);
        var instanceExpression = curve.ToExpression().InfValue();
        var expected = curve.InfValue();

        Assert.IsType<InfValueExpression>(expression);
        Assert.IsType<InfValueExpression>(concreteExpression);
        Assert.IsType<InfValueExpression>(instanceExpression);
        Assert.Equal(expected, expression.Compute());
        Assert.Equal(expected, concreteExpression.Compute());
        Assert.Equal(expected, instanceExpression.Compute());
    }

    [Fact]
    public void MaxValueExpressionComputesAttainedMaximum()
    {
        var curve = new ConstantCurve(7);
        var expression = curve.ToExpression();

        Assert.Equal(new Rational(7), expression.MaxValue().Compute());
        Assert.Equal(curve.MaxValue(), expression.MaxValue().Compute());
    }

    [Fact]
    public void MinValueExpressionComputesAttainedMinimum()
    {
        var curve = new RateLatencyServiceCurve(rate: 2, latency: 3);
        var expression = curve.ToExpression();

        Assert.Equal(Rational.Zero, expression.MinValue().Compute());
        Assert.Equal(curve.MinValue(), expression.MinValue().Compute());
    }

    [Fact]
    public void MaxValueExpressionThrowsWhenSupremumNotAttained()
    {
        var curve = new RateLatencyServiceCurve(rate: 2, latency: 3);
        var expression = curve.ToExpression();

        Assert.Null(curve.MaxValue());
        Assert.Throws<InvalidOperationException>(() => expression.MaxValue().Compute());
    }

    [Fact]
    public void MinValueExpressionThrowsWhenInfimumNotAttained()
    {
        var curve = new RateLatencyServiceCurve(rate: 2, latency: 3).Negate();
        var expression = curve.ToExpression();

        Assert.Null(curve.MinValue());
        Assert.Throws<InvalidOperationException>(() => expression.MinValue().Compute());
    }
}
