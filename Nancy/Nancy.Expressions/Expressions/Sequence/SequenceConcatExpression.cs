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
    /// <param name="sequenceL">The left operand sequence.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="sequenceR">The right operand sequence.</param>
    /// <param name="nameR">The name to assign to the right operand.</param>
    /// <param name="preserveDelay">
    /// If true, preserve the delay of the second sequence with an infinite-valued segment.
    /// Otherwise, remove the delay as if the second sequence started at $t = 0$.
    /// </param>
    /// <param name="preserveShift">
    /// If true, preserve the starting value of the second sequence.
    /// Otherwise, remove the shift as if the second sequence started at value zero.
    /// </param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
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
    /// <param name="sequenceL">The left operand sequence.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="rightExpression">The right operand expression.</param>
    /// <param name="preserveDelay">
    /// If true, preserve the delay of the second sequence with an infinite-valued segment.
    /// Otherwise, remove the delay as if the second sequence started at $t = 0$.
    /// </param>
    /// <param name="preserveShift">
    /// If true, preserve the starting value of the second sequence.
    /// Otherwise, remove the shift as if the second sequence started at value zero.
    /// </param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
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
    /// <param name="leftExpression">The left operand expression.</param>
    /// <param name="rightExpression">The right operand expression.</param>
    /// <param name="preserveDelay">
    /// If true, preserve the delay of the second sequence with an infinite-valued segment.
    /// Otherwise, remove the delay as if the second sequence started at $t = 0$.
    /// </param>
    /// <param name="preserveShift">
    /// If true, preserve the starting value of the second sequence.
    /// Otherwise, remove the shift as if the second sequence started at value zero.
    /// </param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
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
