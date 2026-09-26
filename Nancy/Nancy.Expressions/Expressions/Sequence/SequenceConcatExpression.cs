using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the concatenation of two sequences (<see cref="Sequence.Concat(Sequence,Sequence,bool,bool)"/>).
/// </summary>
/// <remarks>
/// The operands are joined in order, the right one following the left, so this node is binary rather than n-ary:
/// concatenation is associative but not commutative, and an n-ary node's operands are matched in any order.
/// </remarks>
public record SequenceConcatExpression : SequenceBinaryExpression<Sequence, Sequence>
{
    /// <summary>
    /// If true, the delay of the right operand is preserved as a gap between the two.
    /// </summary>
    public bool PreserveDelay { get; init; }

    /// <summary>
    /// If true, the value the right operand starts from is preserved as a jump at the join.
    /// </summary>
    public bool PreserveShift { get; init; }

    /// <summary>
    /// Creates a concatenation expression.
    /// </summary>
    public SequenceConcatExpression(
        Sequence sequenceL,
        string nameL,
        Sequence sequenceR,
        string nameR,
        bool preserveDelay = false,
        bool preserveShift = false,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequenceL, nameL), new ConcreteSequenceExpression(sequenceR, nameR),
            preserveDelay, preserveShift, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a concatenation expression.
    /// </summary>
    public SequenceConcatExpression(
        Sequence sequenceL,
        string nameL,
        SequenceExpression rightExpression,
        bool preserveDelay = false,
        bool preserveShift = false,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequenceL, nameL), rightExpression,
            preserveDelay, preserveShift, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a concatenation expression.
    /// </summary>
    public SequenceConcatExpression(
        SequenceExpression leftExpression,
        SequenceExpression rightExpression,
        bool preserveDelay = false,
        bool preserveShift = false,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(leftExpression, rightExpression, expressionName, settings)
    {
        PreserveDelay = preserveDelay;
        PreserveShift = preserveShift;
    }

    /// <inheritdoc />
    public override void Accept(ISequenceExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISequenceExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
