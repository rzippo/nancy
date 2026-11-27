using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the horizontal shift of a sequence (<see cref="Sequence.HorizontalShift"/>).
/// </summary>
public record SequenceHorizontalShiftExpression : SequenceBinaryExpression<Sequence, Rational>
{
    /// <summary>
    /// Creates the horizontal shift of a sequence expression.
    /// </summary>
    /// <param name="sequence">The operand sequence.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="value">The amount of the shift.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceHorizontalShiftExpression(
        Sequence sequence,
        string name,
        Rational value,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequence, name), new RationalNumberExpression(value), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the horizontal shift of a sequence expression.
    /// </summary>
    /// <param name="leftOperand">The left operand expression.</param>
    /// <param name="rightOperand">The amount of the shift.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceHorizontalShiftExpression(
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
