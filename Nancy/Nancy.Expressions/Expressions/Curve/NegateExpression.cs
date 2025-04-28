using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the negation
/// </summary>
public record NegateExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the negation expression
    /// </summary>
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
    public NegateExpression(
        CurveExpression Expression,
        string ExpressionName = "",
        ExpressionSettings? Settings = null)
        : base(Expression, ExpressionName, Settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
    
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}