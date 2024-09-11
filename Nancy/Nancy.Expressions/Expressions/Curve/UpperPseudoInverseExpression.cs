using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the upper pseudo-inverse
/// (<see cref="Curve.UpperPseudoInverse"/>)
/// </summary>
public class UpperPseudoInverseExpression(CurveExpression expression, string expressionName = "", ExpressionSettings? settings = null)
    : CurveUnaryExpression<Curve>(expression, expressionName, settings)
{
    /// <summary>
    /// Statically adds the set of well-known equivalences involving the upper pseudo-inverse operation to the
    /// dictionary of the equivalences (<see cref="Equivalences"/>)
    /// </summary>
    static UpperPseudoInverseExpression()
    {
        AddEquivalence(typeof(UpperPseudoInverseExpression), new IsomorphismConvLeft());
    }
    
    /// <summary>
    /// Creates the upper pseudo-inverse expression
    /// </summary>
    public UpperPseudoInverseExpression(Curve curve, string name, string expressionName = "", ExpressionSettings? settings = null) : this(
        new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
}