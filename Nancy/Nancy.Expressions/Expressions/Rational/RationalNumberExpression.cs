using System.Runtime.CompilerServices;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class describing an expression composed of a rational (<see cref="Rational"/>) number
/// </summary>
public record RationalNumberExpression : RationalExpression
{
    /// <summary>
    /// Creates a rational number expression starting from a <see cref="Rational"/> object
    /// </summary>
    public RationalNumberExpression(
        Rational number,
        [CallerArgumentExpression("number")] string expressionName = "", 
        ExpressionSettings? settings = null)
        : base(expressionName, settings)
    {
        _value = number;
    }

    /// <inheritdoc />
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}