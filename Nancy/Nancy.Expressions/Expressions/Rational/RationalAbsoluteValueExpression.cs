using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression which computes the absolute value of a rational number.
/// </summary>
public record RationalAbsoluteValueExpression : RationalUnaryExpression<Rational>
{
    /// <summary>
    /// Creates a rational absolute value expression.
    /// </summary>
    /// <param name="number">The rational number.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public RationalAbsoluteValueExpression(
        Rational number,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new RationalNumberExpression(number), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a rational absolute value expression.
    /// </summary>
    /// <param name="expression">The operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public RationalAbsoluteValueExpression(
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
