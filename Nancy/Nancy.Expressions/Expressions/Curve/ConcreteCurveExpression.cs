using System.Runtime.CompilerServices;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class describing an expression composed of a concrete curve
/// </summary>
public record ConcreteCurveExpression : CurveExpression
{
    /// <summary>
    /// Creates a concrete curve expression with a default curve
    /// </summary>
    public ConcreteCurveExpression() : this(Curve.Zero(), "defaultCurve")
    {
    }

    /// <summary>
    /// Creates a concrete curve expression starting from a <see cref="Curve"/> object
    /// </summary>
    public ConcreteCurveExpression(Curve curve,
        [CallerArgumentExpression("curve")] string name = "",
        ExpressionSettings? settings = null) : base(name, settings)
    {
        _value = curve;
    }

    /// <inheritdoc />
    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);

    /// <summary>
    /// Always <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// A leaf's <c>Value</c> is its only state, set once at construction and never recomputed, so clearing it would lose the curve for good.
    /// </remarks>
    protected internal override bool ValueCacheIsCheap => true;

    /// <summary>
    /// True if <paramref name="other"/> wraps an equal <see cref="Curve"/>.
    /// </summary>
    public virtual bool Equals(ConcreteCurveExpression? other)
        => other is not null && base.Equals(other) && Value.Equals(other.Value);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Value);
}
