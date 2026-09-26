using System;
using Unipi.Nancy.Expressions.Visitors;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class describing a placeholder for any sequence expression (used for equivalences).
/// </summary>
public record SequencePlaceholderExpression : SequenceExpression, IPlaceholderExpression
{
    /// <summary>
    /// Class describing a placeholder for any sequence expression (used for equivalences).
    /// </summary>
    public SequencePlaceholderExpression(
        string sequenceName,
        ExpressionSettings? settings = null)
        : base(sequenceName, settings)
    {
        SequenceName = sequenceName;
    }

    /// <inheritdoc />
    public bool Accepts(IExpression candidate) => candidate is SequenceExpression;

    /// <inheritdoc />
    public override void Accept(ISequenceExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISequenceExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);

    /// <summary>
    /// Name of the sequence placeholder.
    /// </summary>
    public string SequenceName { get; init; }

    /// <summary>
    /// True if <paramref name="other"/> is a placeholder with the same <see cref="SequenceName"/>.
    /// </summary>
    /// <remarks>
    /// The one exception to "name does not matter".
    /// A placeholder has no operands and no computable value, so its name is its entire identity.
    /// </remarks>
    public virtual bool Equals(SequencePlaceholderExpression? other)
        => other is not null && base.Equals(other) && SequenceName == other.SequenceName;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), SequenceName);
}
