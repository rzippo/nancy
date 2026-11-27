using Unipi.Nancy.Expressions;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the horizontal deviation between two sequence expressions.
/// </summary>
public record SequenceVerticalDeviationExpression : RationalBinaryExpression<Sequence, Sequence>
{
    /// <summary>
    /// Creates a horizontal deviation expression.
    /// </summary>
    /// <param name="sequenceL">The left operand sequence.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="sequenceR">The right operand sequence.</param>
    /// <param name="nameR">The name to assign to the right operand.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceVerticalDeviationExpression(
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
    /// Creates a horizontal deviation expression.
    /// </summary>
    /// <param name="sequenceL">The left operand sequence.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="rightExpression">The right operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceVerticalDeviationExpression(
        Sequence sequenceL,
        string nameL,
        SequenceExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequenceL, nameL), rightExpression, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a horizontal deviation expression.
    /// </summary>
    /// <param name="LeftExpression">The left operand expression.</param>
    /// <param name="RightExpression">The right operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="Settings">Settings for the expression definition and evaluation.</param>
    public SequenceVerticalDeviationExpression(
        SequenceExpression LeftExpression,
        SequenceExpression RightExpression,
        string expressionName = "",
        ExpressionSettings? Settings = null)
        : base(LeftExpression, RightExpression, expressionName, Settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
