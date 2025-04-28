using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// An expression that computes the addition between rational numbers.
/// N-ary operator.
/// </summary>
public record RationalAdditionExpression : RationalNAryExpression
{
    /// <summary>
    /// Creates an expression for addition of rational numbers.
    /// </summary>
    public RationalAdditionExpression(
        IReadOnlyCollection<IGenericExpression<Rational>> expressions,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expressions, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates an expression for addition of rational numbers.
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

    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}