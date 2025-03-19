using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the addition between rational numbers (n-ary operation)
/// </summary>
public record RationalAdditionExpression : RationalNAryExpression
{
    /// <summary>
    /// Creates a (rational) addition expression
    /// </summary>
    public RationalAdditionExpression(
        IReadOnlyCollection<IGenericExpression<Rational>> expressions,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expressions, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a (rational) addition expression
    /// </summary>
    public RationalAdditionExpression(
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