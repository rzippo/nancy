using Unipi.Nancy.Expressions.Visitors;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class describing a placeholder for any curve expression (used for equivalences)
/// </summary>
public class CurvePlaceholderExpression(
    string curveName,
    ExpressionSettings? settings = null) : CurveExpression(curveName, settings)
{
    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
}