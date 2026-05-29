using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

public record RationalAbsoluteValueExpression : RationalUnaryExpression<Rational>
{
    public RationalAbsoluteValueExpression(
        Rational number,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new RationalNumberExpression(number), expressionName, settings)
    {
    }

    public RationalAbsoluteValueExpression(
        RationalExpression expression,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(expression, expressionName, settings)
    {
    }

    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
