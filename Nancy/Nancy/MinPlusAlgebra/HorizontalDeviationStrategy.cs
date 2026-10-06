namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// The algorithms that compute the horizontal deviation, chosen by <see cref="ComputationSettings.HorizontalDeviationStrategy"/>.
/// Each gives the same value, and they differ only in performance.
/// </summary>
public enum HorizontalDeviationStrategy
{
    /// <summary>
    /// The largest gap between the lower pseudo-inverses, $\sup_y \left( g^{-1}_\downarrow(y) - f^{-1}_\downarrow(y) \right)$,
    /// the height form of [TBP-SEQ-DEV] § 4.5 for sequences and § 4.6 for curves, which for curves with finite values that are not bounded above is [PS22] Lemma 25.
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
    /// The (max,+) deconvolution of the upper pseudo-inverses, taken at 0.
    /// It is the height form of [PS22] Lemma 25 with the upper pseudo-inverses, [TBP-SEQ-DEV] Proposition 42.
    /// This form does not cover sequences, so <see cref="PseudoInverseGap"/> is used instead.
    /// </summary>
    PseudoInverseDeconvolution,
}
