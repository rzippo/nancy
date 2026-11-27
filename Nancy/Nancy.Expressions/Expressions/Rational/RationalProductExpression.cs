using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the product between rational numbers (n-ary operation)
/// </summary>
public record RationalProductExpression : RationalNAryExpression
{
    /// <summary>
    /// Creates a (rational) product expression
    /// </summary>
    /// <param name="operands">The operand expressions.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public RationalProductExpression(
        IReadOnlyCollection<IGenericExpression<Rational>> operands,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(operands, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a (rational) product expression
    /// </summary>
    /// <param name="rationals">The operand rational numbers.</param>
    /// <param name="names">The names to assign to the operands, one per operand and in the same order.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    /// <exception cref="ArgumentException">The number of names differs from the number of operands.</exception>
    public RationalProductExpression(
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