using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the left-continuous projection of a sequence (<see cref="Sequence.ToLeftContinuous"/>).
/// </summary>
public record SequenceToLeftContinuousExpression : SequenceUnaryExpression<Sequence>
{
    /// <summary>
    /// Creates the left-continuous projection of a sequence expression.
    /// </summary>
    /// <param name="sequence">The operand sequence.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceToLeftContinuousExpression(
        Sequence sequence,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequence, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the left-continuous projection of a sequence expression.
    /// </summary>
    /// <param name="expression">The operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceToLeftContinuousExpression(
        SequenceExpression expression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(ISequenceExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISequenceExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
