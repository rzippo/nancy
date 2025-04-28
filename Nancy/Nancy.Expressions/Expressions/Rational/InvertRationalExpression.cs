using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the inversion of a rational number
/// </summary>
public record InvertRationalExpression : RationalUnaryExpression<Rational>
{
    /// <summary>
    /// Creates a "rational inversion expression"
    /// </summary>
    public InvertRationalExpression(
        Rational number,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new RationalNumberExpression(number), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the inversion of a rational number
    /// </summary>
    public InvertRationalExpression(
        RationalExpression expression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a "rational inversion expression"
    /// </summary>
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}