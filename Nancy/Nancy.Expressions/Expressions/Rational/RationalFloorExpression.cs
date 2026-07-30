using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the floor of a rational number, $\lfloor x \rfloor$.
/// </summary>
public record RationalFloorExpression : RationalUnaryExpression<Rational>
{
    /// <summary>
    /// Creates a "rational floor expression"
    /// </summary>
    public RationalFloorExpression(
        Rational number,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new RationalNumberExpression(number), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the floor of a rational number, $\lfloor x \rfloor$.
    /// </summary>
    public RationalFloorExpression(
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
