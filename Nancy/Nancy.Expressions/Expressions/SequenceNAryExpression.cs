using System.Linq;
using System;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class representing expressions whose value is a <see cref="Sequence"/> object and the root is an operation which accepts n (n >= 2) operands (which are sequence expressions) and is commutative and associative.
/// </summary>
public abstract record
    SequenceNAryExpression : SequenceExpression, IGenericNAryExpression<Sequence, Sequence> // For operators on sequences that are commutative and associative
{
    /// <inheritdoc />
    public IReadOnlyCollection<IGenericExpression<Sequence>> Operands { get; init; }

    /// <summary>
    /// Creates the n-ary expression starting from a collection of expression operands.
    /// </summary>
    public SequenceNAryExpression(
        IReadOnlyCollection<IGenericExpression<Sequence>> expressions,
        string expressionName = "", ExpressionSettings? settings = null) : base(expressionName, settings)
    {
        Operands = expressions;
    }

    /// <summary>
    /// Creates the n-ary expression starting from a collection of operands of type <see cref="Sequence"/> (converted to <see cref="ConcreteSequenceExpression"/> objects).
    /// </summary>
    public SequenceNAryExpression(
        IReadOnlyCollection<Sequence> sequences,
        IReadOnlyCollection<string> names,
        string expressionName = "",
        ExpressionSettings? settings = null) : base(expressionName, settings)
    {
        List<IGenericExpression<Sequence>> expressions = [];
        foreach (var (sequence, name) in sequences.Zip(names, (c, n) => (sequence: c, name: n)))
            expressions.Add(new ConcreteSequenceExpression(sequence, name));
        Operands = expressions;
    }

    /// <summary>
    /// Adds another operand to the expression.
    /// </summary>
    public SequenceExpression Append(IGenericExpression<Sequence> expression, string expressionName = "", ExpressionSettings? settings = null)
    {
        IReadOnlyCollection<IGenericExpression<Sequence>> operands =
            GetType() == expression.GetType() && string.IsNullOrEmpty(expression.Name)
                ? [.. Operands, .. ((SequenceNAryExpression)expression).Operands]
                : [.. Operands, expression];
        return this with { Operands = operands, Name = expressionName, Settings = settings, Generation = 0 };
    }

    /// <summary>
    /// Returns a copy of this node with the given operands in place of its own.
    /// </summary>
    /// <param name="operands">The new operands.</param>
    /// <remarks>
    /// The copy carries <see cref="SequenceExpression.Name"/>, <see cref="SequenceExpression.Generation"/> and <see cref="SequenceExpression.Settings"/>, and leaves the computed-value caches behind.
    /// It is the concrete node's own type that is copied, so a node with state beyond its operands keeps that state.
    /// </remarks>
    public virtual IGenericExpression<Sequence> WithOperands(IReadOnlyCollection<IGenericExpression<Sequence>> operands)
        => this with { Operands = operands };

    /// <summary>
    /// True if <paramref name="other"/> is the same operator over the same operands, as an unordered multiset.
    /// </summary>
    /// <remarks>
    /// <c>Addition(a, b)</c> equals <c>Addition(b, a)</c>, the operator being commutative.
    /// Each operand is paired off against a candidate confirmed by a real <see cref="object.Equals(object?)"/> call, with hash equality serving as a cheap filter first, so two distinct operands that collide on their hash still compare correctly.
    /// </remarks>
    public virtual bool Equals(SequenceNAryExpression? other)
    {
        if (other is null || !base.Equals(other))
            return false;
        if (Operands.Count != other.Operands.Count)
            return false;

        var remaining = other.Operands.ToList();
        foreach (var operand in Operands)
        {
            var hash = operand.GetHashCode();
            var index = remaining.FindIndex(candidate => candidate.GetHashCode() == hash && operand.Equals(candidate));
            if (index < 0)
                return false;
            remaining.RemoveAt(index);
        }
        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        foreach (var operandHash in Operands.Select(operand => operand.GetHashCode()).OrderBy(h => h))
            hash.Add(operandHash);
        return hash.ToHashCode();
    }
}
