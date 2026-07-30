using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the ceiling of a rational number, $\lceil x \rceil$.
/// </summary>
public record RationalCeilExpression : RationalUnaryExpression<Rational>
{
    /// <summary>
    /// Creates a "rational ceiling expression"
    /// </summary>
    public RationalCeilExpression(
        Rational number,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new RationalNumberExpression(number), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the ceiling of a rational number, $\lceil x \rceil$.
    /// </summary>
    public RationalCeilExpression(
        RationalExpression expression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
