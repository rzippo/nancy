using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

public record WithOriginAtExpression : CurveUnaryExpression<Curve>
{
    public WithOriginAtExpression(
        Curve curve,
        string name,
        Rational value,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), value, expressionName, settings)
    {
    }

    public WithOriginAtExpression(
        CurveExpression expression,
        Rational value,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
        OriginValue = value;
    }

    public Rational OriginValue { get; }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);

    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
