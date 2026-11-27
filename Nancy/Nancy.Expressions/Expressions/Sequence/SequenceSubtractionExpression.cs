using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the subtraction.
/// </summary>
public record SequenceSubtractionExpression : SequenceBinaryExpression<Sequence, Sequence>
{
    /// <summary>
    /// Creates a subtraction expression.
    /// </summary>
    /// <param name="sequenceL">The left operand sequence.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="sequenceR">The right operand sequence.</param>
    /// <param name="nameR">The name to assign to the right operand.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceSubtractionExpression(
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
    /// Creates a subtraction expression.
    /// </summary>
    /// <param name="sequenceL">The left operand sequence.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="rightExpression">The right operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceSubtractionExpression(
        Sequence sequenceL,
        string nameL,
        SequenceExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequenceL, nameL), rightExpression, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a subtraction expression.
    /// </summary>
    /// <param name="leftExpression">The left operand expression.</param>
    /// <param name="rightExpression">The right operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceSubtractionExpression(
        SequenceExpression leftExpression,
        SequenceExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(leftExpression, rightExpression, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(ISequenceExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISequenceExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
