using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the upper pseudo-inverse
/// (<see cref="Curve.UpperPseudoInverse"/>)
/// </summary>
public record UpperPseudoInverseExpression : CurveUnaryExpression<Curve>
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
    /// <param name="curve">The operand curve.</param>
    /// <param name="name">The name to assign to the operand.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public UpperPseudoInverseExpression(
        Curve curve,
        string name,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), expressionName, settings)
    {
    }

    /// <summary>
    /// Class representing an expression whose root operation is the upper pseudo-inverse
    /// (<see cref="Curve.UpperPseudoInverse"/>)
    /// </summary>
    /// <param name="expression">The operand expression.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public UpperPseudoInverseExpression(
        CurveExpression expression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}