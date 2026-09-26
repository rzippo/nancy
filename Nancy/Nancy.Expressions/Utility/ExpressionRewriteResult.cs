using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Utility;

/// <summary>
/// The outcome of a rewrite:
/// the new expression, how many replacements were made, where, and the bindings of an equivalence if one was applied.
/// </summary>
public sealed record ExpressionRewriteResult
{
    /// <summary>
    /// The expression the rewrite produced.
    /// When nothing matched, this is the expression the rewrite was called on, unchanged.
    /// </summary>
    public required IExpression Expression { get; init; }

    /// <summary>
    /// How many sites were replaced.
    /// </summary>
    public int ReplacementCount { get; init; }

    /// <summary>
    /// The positions of the replaced sites, in the order they were visited.
    /// </summary>
    public IReadOnlyList<ExpressionPosition> Positions { get; init; } = [];

    /// <summary>
    /// What the placeholders of the last equivalence applied bound to, by name, or <see langword="null"/> if none did.
    /// </summary>
    /// <remarks>
    /// The placeholders of one equivalence may stand for expressions of different value types, so a caller casts where it knows what it asked for.
    /// </remarks>
    public IReadOnlyDictionary<string, IExpression>? Bindings { get; init; }

    /// <summary>
    /// True if at least one site was replaced.
    /// </summary>
    public bool Matched => ReplacementCount > 0;
}
