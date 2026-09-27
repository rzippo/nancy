using System;
using System.Runtime.CompilerServices;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class describing an expression composed of a concrete curve.
/// </summary>
public record ConcreteSequenceExpression : SequenceExpression, IExpressionLeaf
{
    /// <summary>
    /// Creates a concrete sequence expression with a default sequence.
    /// </summary>
    public ConcreteSequenceExpression() : this(Sequence.Zero(0, 1), "defaultSequence")
    {
    }

    /// <summary>
    /// Creates a concrete sequence expression starting from a <see cref="Sequence"/> object.
    /// </summary>
    public ConcreteSequenceExpression(Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "",
        ExpressionSettings? settings = null) : base(name, settings)
    {
        _value = sequence;
    }

    /// <inheritdoc />
    /// <remarks>
    /// The cached value is the sequence this leaf wraps, not a computed result, so there is nothing to reclaim by clearing it.
    /// </remarks>
    protected internal override bool ValueCacheIsCheap => true;

    /// <inheritdoc />
    public override void Accept(ISequenceExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISequenceExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);

    /// <summary>
    /// True if <paramref name="other"/> wraps the same sequence.
    /// </summary>
    /// <remarks>
    /// The sequence lives in the base's value cache, which equality otherwise ignores, so a leaf has to compare it itself.
    /// </remarks>
    public virtual bool Equals(ConcreteSequenceExpression? other)
        => other is not null && base.Equals(other) && Value.Equals(other.Value);

    /// <inheritdoc />
    bool IExpressionLeaf.ValueMatches(IExpression other)
        => other is ConcreteSequenceExpression sequence && Value.Equals(sequence.Value);

    /// <inheritdoc />
    void IExpressionLeaf.ResetValueCaches()
        => Value.ResetCachedProperties();

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Value);
}
