using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the negation
/// </summary>
public record NegateExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the negation expression
    /// </summary>
    /// <param name="curve">The operand curve.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public NegateExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the negation
    /// </summary>
    /// <param name="Expression">The operand expression.</param>
    /// <param name="ExpressionName">The name of the expression.</param>
    /// <param name="Settings">Settings for the expression definition and evaluation.</param>
    public NegateExpression(
        CurveExpression Expression,
        string ExpressionName = "",
        ExpressionSettings? Settings = null)
        : base(Expression, ExpressionName, Settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}