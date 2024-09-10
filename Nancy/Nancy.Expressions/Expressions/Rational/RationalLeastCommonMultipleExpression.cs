using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the l.c.m. between rational numbers (or expressions)
/// </summary>
public class RationalLeastCommonMultipleExpression : RationalNAryExpression
{
    /// <summary>
    /// Creates a l.c.m. expression
    /// </summary>
    public RationalLeastCommonMultipleExpression(IReadOnlyCollection<IGenericExpression<Rational>> expressions,
        string expressionName = "", ExpressionSettings? settings = null) : base(expressions, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a l.c.m. expression
    /// </summary>
    public RationalLeastCommonMultipleExpression(IReadOnlyCollection<Rational> rationals,
        IReadOnlyCollection<string> names, string expressionName = "", ExpressionSettings? settings = null) : base(rationals, names, expressionName, settings)
    {
    }

    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);
}