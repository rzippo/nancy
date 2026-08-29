using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the subtraction between rational numbers
/// </summary>
public record RationalSubtractionExpression : RationalBinaryExpression<Rational, Rational>
{
    /// <summary>
    /// Class representing an expression whose root operation is the subtraction between rational numbers
    /// </summary>
    public RationalSubtractionExpression(
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