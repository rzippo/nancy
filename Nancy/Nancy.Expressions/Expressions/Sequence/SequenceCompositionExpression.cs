using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the composition of two sequences (<see cref="Sequence.Composition(Sequence,Sequence)"/>).
/// </summary>
public record SequenceCompositionExpression : SequenceBinaryExpression<Sequence, Sequence>
{
    /// <summary>
    /// Creates a composition expression.
    /// </summary>
    /// <param name="sequenceL">The left operand sequence.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="sequenceR">The right operand sequence.</param>
    /// <param name="nameR">The name to assign to the right operand.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceCompositionExpression(
        Sequence sequenceL,
        string nameL,
        Sequence sequenceR,
        string nameR,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequenceL, nameL), new ConcreteSequenceExpression(sequenceR, nameR), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a composition expression.
    /// </summary>
    /// <param name="leftOperand">The left operand expression.</param>
    /// <param name="rightOperand">The right operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceCompositionExpression(
        SequenceExpression leftOperand,
        SequenceExpression rightOperand,
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
