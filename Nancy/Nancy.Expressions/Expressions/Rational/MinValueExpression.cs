using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression that computes the minimum value attained by a curve expression
/// (<see cref="Curve.MinValue"/>).
/// </summary>
/// <remarks>
/// Computing this expression throws an <see cref="InvalidOperationException"/> if the curve does not attain a
/// minimum, i.e., if its infimum is not attained by any point of the curve. Use <see cref="InfValueExpression"/>
/// if the infimum is sufficient.
/// </remarks>
public record MinValueExpression : RationalUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the "minimum value" expression
    /// </summary>
    public MinValueExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the "minimum value" expression
    /// </summary>
    public MinValueExpression(
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
