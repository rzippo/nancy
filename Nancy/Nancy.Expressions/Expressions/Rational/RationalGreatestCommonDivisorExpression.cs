using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the g.c.d. between rational numbers (or expressions)
/// </summary>
public record RationalGreatestCommonDivisorExpression : RationalNAryExpression
{
    /// <summary>
    /// Creates a g.c.d. expression
    /// </summary>
    /// <param name="operands">The operand expressions.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public RationalGreatestCommonDivisorExpression(
        IReadOnlyCollection<IGenericExpression<Rational>> operands,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(operands, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a g.c.d. expression
    /// </summary>
    /// <param name="rationals">The operand rational numbers.</param>
    /// <param name="names">The names to assign to the operands, in the same order.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public RationalGreatestCommonDivisorExpression(
        IReadOnlyCollection<Rational> rationals,
        IReadOnlyCollection<string> names,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(rationals, names, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}