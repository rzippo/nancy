using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression that computes the maximum value attained by a curve expression
/// (<see cref="Curve.MaxValue"/>).
/// </summary>
/// <remarks>
/// Computing this expression throws an <see cref="InvalidOperationException"/> if the curve does not attain a
/// maximum, i.e., if its supremum is not attained by any point of the curve. Use <see cref="SupValueExpression"/>
/// if the supremum is sufficient.
/// </remarks>
public record MaxValueExpression : RationalUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the "maximum value" expression
    /// </summary>
    public MaxValueExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the "maximum value" expression
    /// </summary>
    public MaxValueExpression(
        CurveExpression expression,
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
