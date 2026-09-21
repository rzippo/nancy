using Unipi.Nancy.Expressions;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression that computes the cut of a curve over an interval.
/// </summary>
public record CurveCutExpression : SequenceUnaryExpression<Curve>
{
    /// <summary>
    /// The cut interval.
    /// </summary>
    public Interval Interval { get; init; }

    /// <summary>
    /// Creates the cut expression.
    /// </summary>
    public CurveCutExpression(
        Curve curve,
        string name,
        Interval interval,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), interval, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the cut expression.
    /// </summary>
    public CurveCutExpression(
        CurveExpression Expression,
        Interval interval,
        string expressionName = "",
        ExpressionSettings? Settings = null)
        : base(Expression, expressionName, Settings)
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
