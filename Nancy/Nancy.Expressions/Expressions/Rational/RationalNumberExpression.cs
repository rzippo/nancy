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

    /// <summary>
    /// Always <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// A leaf's <c>Value</c> is its only state, set once at construction and never recomputed, so clearing it would lose the value for good.
    /// </remarks>
    protected internal override bool ValueCacheIsCheap => true;

    /// <summary>
    /// True if <paramref name="other"/> wraps an equal <see cref="Rational"/>.
    /// </summary>
    public virtual bool Equals(RationalNumberExpression? other)
        => other is not null && base.Equals(other) && Value.Equals(other.Value);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Value);
}