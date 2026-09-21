using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the ceiling of a sequence (<see cref="Sequence.Ceil"/>).
/// </summary>
public record SequenceCeilExpression : SequenceUnaryExpression<Sequence>
{
    /// <summary>
    /// Creates the ceiling of a sequence expression.
    /// </summary>
    public SequenceCeilExpression(
        Sequence sequence,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequence, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the ceiling of a sequence expression.
    /// </summary>
    public SequenceCeilExpression(
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
