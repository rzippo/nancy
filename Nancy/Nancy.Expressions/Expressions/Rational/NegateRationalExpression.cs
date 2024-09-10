using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the negation of a rational number
/// </summary>
public class NegateRationalExpression(RationalExpression expression, string expressionName = "", ExpressionSettings? settings = null)
    : RationalUnaryExpression<Rational>(expression, expressionName, settings)
{
    /// <summary>
    /// Creates a "rational negation expression"
    /// </summary>
    public NegateRationalExpression(Rational number, string expressionName = "", ExpressionSettings? settings = null) : this(
        new RationalNumberExpression(number), expressionName, settings)
    {
    }
    
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);
}