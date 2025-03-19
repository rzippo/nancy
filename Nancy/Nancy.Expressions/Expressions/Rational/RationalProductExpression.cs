using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the product between rational numbers (n-ary operation)
/// </summary>
public record RationalProductExpression : RationalNAryExpression
{
    /// <summary>
    /// Creates a (rational) product expression
    /// </summary>
    public RationalProductExpression(
        IReadOnlyCollection<IGenericExpression<Rational>> expressions,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expressions, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a (rational) product expression
    /// </summary>
    public RationalProductExpression(
        IReadOnlyCollection<Rational> rationals,
        IReadOnlyCollection<string> names,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(rationals, names, expressionName, settings)
    {
    }

    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);
}