using Unipi.Nancy.Expressions;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression that computes the cut of a sequence over an interval.
/// </summary>
public record SequenceCutToNeighbourhoodExpression : SequenceUnaryExpression<Sequence>
{
    /// <summary>
    /// Left endpoint of the interval the result is a neighbourhood of.
    /// </summary>
    public Rational CutStart { get; init; }

    /// <summary>
    /// Right endpoint of the interval the result is a neighbourhood of.
    /// </summary>
    public Rational CutEnd { get; init; }

    /// <summary>
    /// Creates a cut expression from a concrete sequence.
    /// </summary>
    public SequenceCutToNeighbourhoodExpression(
        Sequence sequence,
        string name,
        Rational cutStart,
        Rational cutEnd,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequence, name), cutStart, cutEnd, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a cut expression from an existing sequence expression.
    /// </summary>
    public SequenceCutToNeighbourhoodExpression(
        SequenceExpression expression,
        Rational cutStart,
        Rational cutEnd,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
        CutStart = cutStart;
        CutEnd = cutEnd;
    }

    /// <inheritdoc />
    public override void Accept(ISequenceExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISequenceExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
