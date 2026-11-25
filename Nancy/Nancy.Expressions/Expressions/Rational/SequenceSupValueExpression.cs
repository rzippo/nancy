using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression that computes the supremum of a sequence (<see cref="Sequence.SupValue"/>).
/// </summary>
public record SequenceSupValueExpression : RationalUnaryExpression<Sequence>
{
    /// <summary>
    /// Creates the expression of the supremum of a sequence.
    /// </summary>
    /// <param name="sequence">The operand sequence.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    public SequenceSupValueExpression(
        Sequence sequence,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequence, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the expression of the supremum of a sequence.
    /// </summary>
    /// <param name="expression">The operand expression.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    public SequenceSupValueExpression(
        SequenceExpression expression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
