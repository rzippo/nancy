using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

public class OperandsDeprecationShim
{
    [Fact]
    public void ObsoleteExpressionsReturnsTheSameCollectionAsOperandsOnCurveNAryExpression()
    {
        var expression = (CurveNAryExpression) Unipi.Nancy.Expressions.Expressions.Addition(
            Curve.Zero(), Curve.Zero(), "a", "b");

#pragma warning disable CS0618
        Assert.Same(expression.Operands, expression.Expressions);
#pragma warning restore CS0618
    }

    [Fact]
    public void ObsoleteExpressionsReturnsTheSameCollectionAsOperandsOnRationalNAryExpression()
    {
        var expression = (RationalNAryExpression) Unipi.Nancy.Expressions.Expressions.RationalAddition(
            Rational.Zero, Rational.Zero, "a", "b");

#pragma warning disable CS0618
        Assert.Same(expression.Operands, expression.Expressions);
#pragma warning restore CS0618
    }
}
