using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

public record RationalModuloExpression : RationalBinaryExpression<Rational, Rational>
{
    public RationalModuloExpression(
        IGenericExpression<Rational> leftExpression,
        IGenericExpression<Rational> rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(leftExpression, rightExpression, expressionName, settings)
    {
    }

    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
