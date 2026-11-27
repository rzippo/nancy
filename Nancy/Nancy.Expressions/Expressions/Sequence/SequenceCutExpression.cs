using Unipi.Nancy.Expressions;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression that computes the cut of a sequence over an interval.
/// </summary>
public record SequenceCutExpression : SequenceUnaryExpression<Sequence>
{
    /// <summary>
    /// The cut interval.
    /// </summary>
    public Interval Interval { get; init; }

    /// <summary>
    /// Creates a cut expression from a concrete sequence.
    /// </summary>
    /// <param name="sequence">The operand sequence.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="interval">The interval to cut.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceCutExpression(
        Sequence sequence,
        string name,
        Interval interval,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteSequenceExpression(sequence, name), interval, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a cut expression from an existing sequence expression.
    /// </summary>
    /// <param name="expression">The operand expression.</param>
    /// <param name="interval">The interval to cut.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public SequenceCutExpression(
        SequenceExpression expression,
        Interval interval,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
        Interval = interval;
    }

    /// <inheritdoc />
    public override void Accept(ISequenceExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISequenceExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
