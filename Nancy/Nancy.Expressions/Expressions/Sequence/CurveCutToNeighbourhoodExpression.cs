using Unipi.Nancy.Expressions;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression that computes a cut of a curve over a neighbourhood of an interval.
/// </summary>
public record CurveCutToNeighbourhoodExpression : SequenceUnaryExpression<Curve>
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
    /// Creates the cut expression.
    /// </summary>
    public CurveCutToNeighbourhoodExpression(
        Curve curve,
        string name,
        Rational cutStart,
        Rational cutEnd,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), cutStart, cutEnd, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the cut expression.
    /// </summary>
    public CurveCutToNeighbourhoodExpression(
        CurveExpression Expression,
        Rational cutStart,
        Rational cutEnd,
        string expressionName = "",
        ExpressionSettings? Settings = null)
        : base(Expression, expressionName, Settings)
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
