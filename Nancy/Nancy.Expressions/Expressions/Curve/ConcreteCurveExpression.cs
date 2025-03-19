using System.Runtime.CompilerServices;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class describing an expression composed of a concrete curve
/// </summary>
public record ConcreteCurveExpression : CurveExpression
{
    /// <summary>
    /// Creates a concrete curve expression with a default curve
    /// </summary>
    public ConcreteCurveExpression() : this(Curve.Zero(), "defaultCurve")
    {
    }

    /// <summary>
    /// Creates a concrete curve expression starting from a <see cref="Curve"/> object
    /// </summary>
    public ConcreteCurveExpression(Curve curve,
        [CallerArgumentExpression("curve")] string name = "",
        ExpressionSettings? settings = null) : base(name, settings)
    {
        _value = curve;
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
}
