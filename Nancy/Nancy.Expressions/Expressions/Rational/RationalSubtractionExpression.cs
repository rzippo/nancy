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
        IGenericExpression<Rational> leftExpression,
        IGenericExpression<Rational> rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(leftExpression, rightExpression, expressionName, settings)
    {
    }

    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}