using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the left limit of a sequence at a time (<see cref="Sequence.LeftLimitAt"/>).
/// </summary>
public record SequenceLeftLimitAtExpression : RationalBinaryExpression<Sequence, Rational>
{
    /// <summary>
    /// Creates the left limit of a sequence at a time expression.
    /// </summary>
    /// <param name="sequence">The operand sequence.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="value">The target time of the limit.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceLeftLimitAtExpression(
        Sequence sequence,
        string name,
        Rational value,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequence, name), new RationalNumberExpression(value), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the left limit of a sequence at a time expression.
    /// </summary>
    /// <param name="leftOperand">The left operand expression.</param>
    /// <param name="rightOperand">The target time of the limit.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceLeftLimitAtExpression(
        SequenceExpression leftOperand,
        RationalExpression rightOperand,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(leftOperand, rightOperand, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
