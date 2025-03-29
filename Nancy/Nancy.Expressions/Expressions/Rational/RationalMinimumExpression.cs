using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// An expression that computes the minimum between rational numbers.
/// N-ary operator.
/// </summary>
public record RationalMinimumExpression : RationalNAryExpression
{
    /// <summary>
    /// Creates an expression for minimum of rational numbers.
    /// </summary>
    public RationalMinimumExpression(
        IReadOnlyCollection<IGenericExpression<Rational>> expressions,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expressions, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates an expression for minimum of rational numbers.
    /// </summary>
    public RationalMinimumExpression(
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