using Unipi.Nancy.Expressions.Visitors;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class describing a placeholder for any rational expression (used for equivalences)
/// </summary>
public class RationalPlaceholderExpression(
    string rationalName,
    ExpressionSettings? settings = null) : RationalExpression(rationalName, settings)
{
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);
}