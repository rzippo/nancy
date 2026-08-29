using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class ToConcrete
{
    [Fact]
    public void CollapsesToAConcreteCurveExpressionWithTheSameValue()
    {
        var a = new RateLatencyServiceCurve(rate: 2, latency: 0).ToExpression("a");
        var b = new SigmaRhoArrivalCurve(sigma: 3, rho: 1).ToExpression("b");
        CurveExpression sum = a.Addition(b);

        var concrete = sum.ToConcrete();

        Assert.IsType<ConcreteCurveExpression>(concrete);
        Assert.True(Curve.Equivalent(sum.Compute(), concrete.Compute()));
    }

    [Fact]
    public void LeavesTheOriginalExpressionUnchanged()
    {
        var a = new RateLatencyServiceCurve(rate: 2, latency: 0).ToExpression("a");
        var b = new SigmaRhoArrivalCurve(sigma: 3, rho: 1).ToExpression("b");
        CurveExpression sum = a.Addition(b).WithName("sum");

        sum.ToConcrete();

        Assert.IsType<AdditionExpression>(sum);
        Assert.Equal("sum", sum.Name);
        Assert.Equal(2, ((AdditionExpression)sum).Operands.Count);
    }

    [Fact]
    public void DefaultsItsNameToTheOriginalExpressionsName()
    {
        var expression = new RateLatencyServiceCurve(rate: 2, latency: 0).ToExpression("a").WithName("named");

        var concrete = expression.ToConcrete();

        Assert.Equal("named", concrete.Name);
    }

    [Fact]
    public void CollapsesToARationalNumberExpressionWithTheSameValue()
    {
        RationalExpression sum = new Rational(1, 2).ToExpression("half").Addition(new Rational(1, 3).ToExpression("third"));

        var concrete = sum.ToConcrete();

        Assert.IsType<RationalNumberExpression>(concrete);
        Assert.Equal(sum.Compute(), concrete.Compute());
    }
}
