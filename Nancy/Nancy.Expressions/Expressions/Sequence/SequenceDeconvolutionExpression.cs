using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the deconvolution.
/// </summary>
public record SequenceDeconvolutionExpression : SequenceBinaryExpression<Sequence, Sequence>
{
    /// <summary>
    /// Creates a deconvolution expression.
    /// </summary>
    public SequenceDeconvolutionExpression(
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
    /// Creates a deconvolution expression.
    /// </summary>
    public SequenceDeconvolutionExpression(
        Sequence sequenceL,
        string nameL,
        SequenceExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequenceL, nameL), rightExpression, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a deconvolution expression.
    /// </summary>
    public SequenceDeconvolutionExpression(
        SequenceExpression LeftExpression,
        SequenceExpression RightExpression,
        string expressionName = "",
        ExpressionSettings? Settings = null)
        : base(LeftExpression, RightExpression, expressionName, Settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(ISequenceExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISequenceExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
