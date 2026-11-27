using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the division between rational numbers
/// </summary>
public record RationalDivisionExpression : RationalBinaryExpression<Rational, Rational>
{
    /// <summary>
    /// Class representing an expression whose root operation is the division between rational numbers
    /// </summary>
    /// <param name="leftOperand">The left operand expression.</param>
    /// <param name="rightOperand">The right operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public RationalDivisionExpression(
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