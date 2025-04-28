using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the sub-additive closure
/// (<see cref="Curve.SubAdditiveClosure(ComputationSettings)"/>)
/// </summary>
public record SubAdditiveClosureExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Statically adds the set of well-known equivalences involving the sub-additive closure operation to the
    /// dictionary of the equivalences (<see cref="Equivalences"/>)
    /// </summary>
    static SubAdditiveClosureExpression()
    {
        AddEquivalence(typeof(SubAdditiveClosureExpression), new SubAdditiveClosureOfMin());
        AddEquivalence(typeof(SubAdditiveClosureExpression), new SubAdditiveClosureOfSubAdd());
    }
    
    /// <summary>
    /// Creates the sub-additive closure expression
    /// </summary>
    public SubAdditiveClosureExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the sub-additive closure
    /// (<see cref="Curve.SubAdditiveClosure(ComputationSettings)"/>)
    /// </summary>
    public SubAdditiveClosureExpression(
        CurveExpression expression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
    
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}