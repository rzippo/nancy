using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the lower pseudo-inverse
/// (<see cref="Curve.LowerPseudoInverse"/>)
/// </summary>
public record LowerPseudoInverseExpression : CurveUnaryExpression<Curve>
{
    /// <summary>
    /// Statically adds the set of well-known equivalences involving the lower pseudo-inverse operation to the
    /// dictionary of the equivalences (<see cref="Equivalences"/>)
    /// </summary>
    static LowerPseudoInverseExpression()
    {
        AddEquivalence(typeof(LowerPseudoInverseExpression), new IsomorphismConvRight());
        AddEquivalence(typeof(LowerPseudoInverseExpression), new PseudoInversesOfLeftContinuous());
    }
    
    /// <summary>
    /// Creates the lower pseudo-inverse expression
    /// </summary>
    public LowerPseudoInverseExpression(
        Curve curve, 
        string name, 
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the lower pseudo-inverse expression
    /// (<see cref="Curve.LowerPseudoInverse"/>)
    /// </summary>
    public LowerPseudoInverseExpression(
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