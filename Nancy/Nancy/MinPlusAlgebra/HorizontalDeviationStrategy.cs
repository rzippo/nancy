namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// The algorithms that compute the horizontal deviation, chosen by <see cref="ComputationSettings.HorizontalDeviationStrategy"/>.
/// Each gives the same value, and they differ only in performance.
/// </summary>
public enum HorizontalDeviationStrategy
{
    /// <summary>
    /// Computes the horizontal deviation as the supremum of the gap between the lower pseudo-inverses.
    /// For finite-valued, unbounded curves, this is [PS22] Lemma 25.
    /// The general sequence and curve formulations are given in [TBP-SEQ-DEV] § 4.5 and § 4.6, respectively.
    /// </summary>
    PseudoInverseGap,

    /// <summary>
    /// The supremum of the horizontal deviation function, computed through the composition of the lower pseudo-inverse of $g$ with $f$, [DNC18] Lemma 5.2.
    /// </summary>
    Composition,

    /// <summary>
    /// The (min,+) deconvolution of $g^{-1}_\downarrow \circ f$ by $t \mapsto t$, taken at 0, [DNC18] Proposition 5.14.
    /// This form does not cover sequences, so <see cref="Composition"/> is used instead.
    /// </summary>
    CompositionDeconvolution,

    /// <summary>
    /// Computes the horizontal deviation through the (max,+) deconvolution of the upper pseudo-inverses, evaluated at $0$ and negated, [TBP-SEQ-DEV] Proposition 50.
    /// For sequences, <see cref="PseudoInverseGap"/> is used instead.
    /// </summary>
    PseudoInverseDeconvolution,
}
