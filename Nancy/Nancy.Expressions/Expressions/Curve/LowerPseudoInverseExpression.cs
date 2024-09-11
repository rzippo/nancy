using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the lower pseudo-inverse
/// (<see cref="Curve.LowerPseudoInverse"/>)
/// </summary>
public class LowerPseudoInverseExpression(CurveExpression expression, string expressionName = "", ExpressionSettings? settings = null)
    : CurveUnaryExpression<Curve>(expression, expressionName, settings)
{
    /// <summary>
    /// Statically adds the set of well-known equivalences involving the lower pseudo-inverse operation to the
    /// dictionary of the equivalences (<see cref="Equivalences"/>)
    /// </summary>
    static LowerPseudoInverseExpression()
    {
        AddEquivalence(typeof(LowerPseudoInverseExpression), new IsomorphismConvRight());
        AddEquivalence(typeof(LowerPseudoInverseExpression), new PseudoInverseOfLeftContinuous());
    }
    
    /// <summary>
    /// Creates the lower pseudo-inverse expression
    /// </summary>
    public LowerPseudoInverseExpression(Curve curve, string name, string expressionName = "", ExpressionSettings? settings = null) : this(
        new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
}