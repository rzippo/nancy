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
