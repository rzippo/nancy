using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the division between rational numbers
/// </summary>
public class RationalDivisionExpression(IGenericExpression<Rational> leftExpression, IGenericExpression<Rational> rightExpression, string expressionName = "", ExpressionSettings? settings = null) 
    : RationalBinaryExpression<Rational, Rational>(leftExpression, rightExpression, expressionName, settings)
{
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);
}