using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the sub-additive closure
/// (<see cref="Curve.SuperAdditiveClosure(ComputationSettings)"/>)
/// </summary>
public record SuperAdditiveClosureExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Creates the super-additive closure expression
    /// </summary>
    public SuperAdditiveClosureExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the sub-additive closure
    /// (<see cref="Curve.SuperAdditiveClosure(ComputationSettings)"/>)
    /// </summary>
    public SuperAdditiveClosureExpression(
        CurveExpression expression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
}