using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the delay of a sequence (<see cref="Sequence.Delay"/>).
/// </summary>
public record SequenceDelayExpression : SequenceBinaryExpression<Sequence, Rational>
{
    /// <summary>
    /// Creates the delay of a sequence expression.
    /// </summary>
    /// <param name="sequence">The operand sequence.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="value">The amount of the delay.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceDelayExpression(
        Sequence sequence,
        string name,
        Rational value,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequence, name), new RationalNumberExpression(value), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the delay of a sequence expression.
    /// </summary>
    /// <param name="leftOperand">The left operand expression.</param>
    /// <param name="rightOperand">The amount of the delay.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceDelayExpression(
        SequenceExpression leftOperand,
        RationalExpression rightOperand,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(leftOperand, rightOperand, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(ISequenceExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISequenceExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
