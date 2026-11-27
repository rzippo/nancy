using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression which computes the modulo of two rational numbers.
/// </summary>
public record RationalModuloExpression : RationalBinaryExpression<Rational, Rational>
{
    /// <summary>
    /// Creates a rational modulo expression.
    /// </summary>
    /// <param name="leftOperand">The left operand expression.</param>
    /// <param name="rightOperand">The right operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public RationalModuloExpression(
        IGenericExpression<Rational> leftOperand,
        IGenericExpression<Rational> rightOperand,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(leftOperand, rightOperand, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
