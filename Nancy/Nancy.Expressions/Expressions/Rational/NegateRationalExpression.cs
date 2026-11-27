using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the negation of a rational number
/// </summary>
public record NegateRationalExpression : RationalUnaryExpression<Rational>
{
    /// <summary>
    /// Creates a "rational negation expression"
    /// </summary>
    /// <param name="number">The rational number.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
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
    /// <param name="expression">The operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public NegateRationalExpression(
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