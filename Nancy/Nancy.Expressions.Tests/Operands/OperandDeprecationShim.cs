using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

public class OperandDeprecationShim
{
    [Fact]
    public void ObsoleteExpressionReturnsSameOperandAsOperandOnCurveUnaryExpression()
    {
        var expression = (CurveUnaryExpression<Curve>) Unipi.Nancy.Expressions.Expressions.Negate(
            Curve.Zero(), "a");

#pragma warning disable CS0618
        Assert.Same(expression.Operand, expression.Expression);
#pragma warning restore CS0618
    }

    [Fact]
    public void ObsoleteExpressionReturnsSameOperandAsOperandOnRationalUnaryExpression()
    {
        var expression = (RationalUnaryExpression<Rational>) Unipi.Nancy.Expressions.Expressions.Negate(
            Rational.Zero);

#pragma warning disable CS0618
        Assert.Same(expression.Operand, expression.Expression);
#pragma warning restore CS0618
    }
}
