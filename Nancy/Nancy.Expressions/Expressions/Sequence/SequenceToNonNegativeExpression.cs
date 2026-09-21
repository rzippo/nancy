using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the non-negative closure of a sequence (<see cref="Sequence.ToNonNegative"/>).
/// </summary>
public record SequenceToNonNegativeExpression : SequenceUnaryExpression<Sequence>
{
    /// <summary>
    /// Creates the non-negative closure expression.
    /// </summary>
    public SequenceToNonNegativeExpression(
        Sequence sequence,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequence, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the non-negative closure of a sequence, see <see cref="Sequence.ToNonNegative"/>.
    /// </summary>
    public SequenceToNonNegativeExpression(
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
