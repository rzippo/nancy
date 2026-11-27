using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

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
    /// <param name="curve">The operand curve.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
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
    /// <param name="Expression">The operand expression.</param>
    /// <param name="ExpressionName">The name of the expression.</param>
    /// <param name="Settings">Settings for the expression definition and evaluation.</param>
    public LowerPseudoInverseExpression(
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