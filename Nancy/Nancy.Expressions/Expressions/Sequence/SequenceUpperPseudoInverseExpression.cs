using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the upper pseudo-inverse of a sequence (<see cref="Sequence.UpperPseudoInverse"/>).
/// </summary>
public record SequenceUpperPseudoInverseExpression : SequenceUnaryExpression<Sequence>
{
    /// <summary>
    /// Creates the upper pseudo-inverse of a sequence expression.
    /// </summary>
    public SequenceUpperPseudoInverseExpression(
        Sequence sequence,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequence, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the upper pseudo-inverse of a sequence expression.
    /// </summary>
    public SequenceUpperPseudoInverseExpression(
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
