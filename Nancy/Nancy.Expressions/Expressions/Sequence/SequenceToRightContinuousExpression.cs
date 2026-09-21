using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the right-continuous projection of a sequence (<see cref="Sequence.ToRightContinuous"/>).
/// </summary>
public record SequenceToRightContinuousExpression : SequenceUnaryExpression<Sequence>
{
    /// <summary>
    /// Creates the right-continuous projection of a sequence expression.
    /// </summary>
    public SequenceToRightContinuousExpression(
        Sequence sequence,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequence, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the right-continuous projection of a sequence expression.
    /// </summary>
    public SequenceToRightContinuousExpression(
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
