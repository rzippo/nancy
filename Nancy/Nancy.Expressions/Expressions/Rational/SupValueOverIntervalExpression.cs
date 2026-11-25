using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression that computes the supremum of a curve over an interval (<see cref="Curve.SupValueOverInterval"/>).
/// </summary>
public record SupValueOverIntervalExpression : RationalUnaryExpression<Curve>
{
    /// <summary>
    /// The interval over which the extremum is taken.
    /// </summary>
    public Interval Interval { get; init; }

    /// <summary>
    /// Creates the expression of the supremum of a curve over an interval.
    /// </summary>
    /// <param name="curve">The operand curve.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="interval">The interval over which the extremum is taken.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    public SupValueOverIntervalExpression(
        Curve curve,
        string name,
        Interval interval,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), interval, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the expression of the supremum of a curve over an interval.
    /// </summary>
    /// <param name="expression">The operand expression.</param>
    /// <param name="interval">The interval over which the extremum is taken.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    public SupValueOverIntervalExpression(
        CurveExpression expression,
        Interval interval,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
        Interval = interval;
    }

    /// <inheritdoc />
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
