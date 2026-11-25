using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression that computes a time attaining the minimum of a curve (<see cref="Curve.MinArg"/>).
/// </summary>
/// <remarks>
/// Computing this expression throws an <see cref="InvalidOperationException"/> if the curve does not attain a minimum.
/// Use <see cref="InfArgExpression"/> if the location of the infimum is sufficient.
/// </remarks>
public record MinArgExpression : RationalUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the expression of a time attaining the minimum of a curve.
    /// </summary>
    /// <param name="curve">The operand curve.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    public MinArgExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the expression of a time attaining the minimum of a curve.
    /// </summary>
    /// <param name="expression">The operand expression.</param>
    /// <param name="expressionName">The name to assign to the expression.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    public MinArgExpression(
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
