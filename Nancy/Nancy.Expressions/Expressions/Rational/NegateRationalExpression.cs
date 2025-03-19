using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the negation of a rational number
/// </summary>
public record NegateRationalExpression : RationalUnaryExpression<Rational>
{
    /// <summary>
    /// Creates a "rational negation expression"
    /// </summary>
    public NegateRationalExpression(
        Rational number,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new RationalNumberExpression(number), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the negation of a rational number
    /// </summary>
    public NegateRationalExpression(
        RationalExpression expression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
    }

    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);
}