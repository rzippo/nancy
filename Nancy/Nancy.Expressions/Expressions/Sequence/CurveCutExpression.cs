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
    /// <param name="curve">The operand curve.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="interval">The interval to cut.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
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
    /// <param name="Expression">The operand expression.</param>
    /// <param name="interval">The interval to cut.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="Settings">Settings for the expression definition and evaluation.</param>
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
