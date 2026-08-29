using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

public class LeftRightOperandDeprecationShim
{
    [Fact]
    public void ObsoleteLeftRightExpressionReturnSameOperandsAsLeftRightOperandOnCurveBinaryExpression()
    {
        var expression = (CurveBinaryExpression<Curve, Curve>) Unipi.Nancy.Expressions.Expressions.Subtraction(
            Curve.Zero(), Curve.Zero(), "a", "b");

#pragma warning disable CS0618
        Assert.Same(expression.LeftOperand, expression.LeftExpression);
        Assert.Same(expression.RightOperand, expression.RightExpression);
#pragma warning restore CS0618
    }

    [Fact]
    public void ObsoleteLeftRightExpressionReturnSameOperandsAsLeftRightOperandOnRationalBinaryExpression()
    {
        var expression = (RationalBinaryExpression<Rational, Rational>) Unipi.Nancy.Expressions.Expressions.RationalSubtraction(
            Rational.Zero, Rational.Zero, "a", "b");

#pragma warning disable CS0618
        Assert.Same(expression.LeftOperand, expression.LeftExpression);
        Assert.Same(expression.RightOperand, expression.RightExpression);
#pragma warning restore CS0618
    }
}
