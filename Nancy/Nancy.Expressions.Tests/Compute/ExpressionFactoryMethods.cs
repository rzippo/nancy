using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class ExpressionFactoryMethods
{
    [Fact]
    public void DefaultConcreteCurveExpressionWrapsZeroCurve()
    {
        var expression = new ConcreteCurveExpression();

        Assert.Equal("defaultCurve", expression.Name);
        Assert.True(expression.IsComputed);
        Assert.True(Curve.Equivalent(Curve.Zero(), expression.Compute()));
    }

    [Fact]
    public void CurveExpressionEnumerableSumComputesAddition()
    {
        var curves = new List<Curve>
        {
            new RateLatencyServiceCurve(rate: 2, latency: 0),
            new SigmaRhoArrivalCurve(sigma: 3, rho: 1),
        };
        IEnumerable<CurveExpression> expressions = curves.Select(curve => curve.ToExpression());
        var sumExpression = expressions.Sum();

        Assert.IsType<AdditionExpression>(sumExpression);
        Assert.True(Curve.Equivalent(Curve.Addition(curves), sumExpression.Compute()));
    }

    [Fact]
    public void CurveExpressionReadOnlyCollectionSumComputesAddition()
    {
        var curves = new List<Curve>
        {
            new RateLatencyServiceCurve(rate: 4, latency: 1),
            Curve.Zero(),
        };
        IReadOnlyCollection<CurveExpression> expressions = curves
            .Select(curve => curve.ToExpression())
            .ToList();
        var sumExpression = expressions.Sum();

        Assert.IsType<AdditionExpression>(sumExpression);
        Assert.True(Curve.Equivalent(Curve.Addition(curves), sumExpression.Compute()));
    }

    [Fact]
    public void RationalExpressionEnumerableSumComputesAddition()
    {
        var values = new List<Rational> { 1, new Rational(2, 3), new Rational(-5, 4) };
        IEnumerable<RationalExpression> expressions = values.Select(value => value.ToExpression());
        var sumExpression = expressions.Sum();

        Assert.IsType<RationalAdditionExpression>(sumExpression);
        Assert.Equal(values.Aggregate(Rational.Zero, (sum, value) => sum + value), sumExpression.Compute());
    }

    [Fact]
    public void RationalExpressionReadOnlyCollectionSumComputesAddition()
    {
        var values = new List<Rational> { new Rational(7, 3), new Rational(-1, 2), 5 };
        IReadOnlyCollection<RationalExpression> expressions = values
            .Select(value => value.ToExpression())
            .ToList();
        var sumExpression = expressions.Sum();

        Assert.IsType<RationalAdditionExpression>(sumExpression);
        Assert.Equal(values.Aggregate(Rational.Zero, (sum, value) => sum + value), sumExpression.Compute());
    }
}
