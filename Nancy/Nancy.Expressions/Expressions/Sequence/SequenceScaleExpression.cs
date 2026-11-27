using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the scaling of a sequence by a rational (<see cref="Sequence.Scale"/>).
/// </summary>
public record SequenceScaleExpression : SequenceBinaryExpression<Sequence, Rational>
{
    /// <summary>
    /// Creates the scaling of a sequence by a rational expression.
    /// </summary>
    /// <param name="sequence">The operand sequence.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="value">The scaling factor.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceScaleExpression(
        Sequence sequence,
        string name,
        Rational value,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequence, name), new RationalNumberExpression(value), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the scaling of a sequence by a rational expression.
    /// </summary>
    /// <param name="leftOperand">The left operand expression.</param>
    /// <param name="rightOperand">The scaling factor.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceScaleExpression(
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
