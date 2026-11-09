namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// Used to specify the computation settings of most operations.
/// If left unspecified, the default settings are used.
/// </summary>
public record ComputationSettings
{
    //todo: reference the affected methods, where makes sense

    /// <summary>
    /// Catch-all property. 
    /// Get: is true if any parallelism is enabled.
    /// Set: sets all parallelism options at once, to the given value.
    /// </summary>
    public bool UseParallelism {
        get
        {
            return
                UseParallelLowerEnvelope || UseParallelUpperEnvelope ||
                UseParallelConvolution || UseParallelDeconvolution ||
                UseParallelListConvolution || UseParallelListMinimum || UseParallelListAddition || UseParallelListLowerEnvelope ||
                UseParallelListMaximum || UseParallelListUpperEnvelope ||
                UseParallelExtend || UseParallelComputeExtensionSequences ||
                UseParallelComputeIntervals || UseParallelInsertionComputeIntervals || UseParallelSortElements ||
                UseParallelSubAdditiveClosure;
        }
        set 
        {
            UseParallelLowerEnvelope = value;
            UseParallelUpperEnvelope = value;
            UseParallelConvolution = value;
            UseParallelDeconvolution = value;
            UseParallelListConvolution = value;
            UseParallelListAddition = value;
            UseParallelListMinimum = value;
            UseParallelListLowerEnvelope = value;
            UseParallelListMaximum = value;
            UseParallelListUpperEnvelope = value;
            UseParallelExtend = value;
            UseParallelComputeExtensionSequences = value;
            UseParallelComputeIntervals = value;
            UseParallelInsertionComputeIntervals = value;
            UseParallelSortElements = value;
            UseParallelSubAdditiveClosure = value;
        }
    }

    /// <summary>
    /// If true, long sequence minimums are processed in parallel.
    /// </summary>
    public bool UseParallelLowerEnvelope { get; set; } = true;

    /// <summary>
    /// The lower envelope of a list of elements is processed in parallel if it has at least this number of elements.
    /// Requires <see cref="UseParallelLowerEnvelope"/>.
    /// </summary>
    public int ElementsLowerEnvelopeParallelizationThreshold { get; set; } = 0;

    /// <summary>
    /// The lower envelope of two sequences is processed in parallel if they have at least this number of elements in all.
    /// Requires <see cref="UseParallelLowerEnvelope"/>.
    /// </summary>
    /// <remarks>
    /// For curve minimum, the threshold applies to the element counts of the required cuts.
    /// Operand extension may still use parallelism under its own settings when the envelope is computed serially.
    /// </remarks>
    public int SequenceLowerEnvelopeParallelizationThreshold { get; set; } = 0;

    /// <summary>
    /// If true, long sequence maximums are processed in parallel.
    /// </summary>
    public bool UseParallelUpperEnvelope { get; set; } = true;

    /// <summary>
    /// The upper envelope of a list of elements is processed in parallel if it has at least this number of elements.
    /// Requires <see cref="UseParallelUpperEnvelope"/>.
    /// </summary>
    public int ElementsUpperEnvelopeParallelizationThreshold { get; set; } = 0;

    /// <summary>
    /// The upper envelope of two sequences is processed in parallel if they have at least this number of elements in all.
    /// Requires <see cref="UseParallelUpperEnvelope"/>.
    /// </summary>
    /// <remarks>
    /// For curve maximum, the threshold applies to the element counts of the required cuts.
    /// Operand extension may still use parallelism under its own settings when the envelope is computed serially.
    /// </remarks>
    public int SequenceUpperEnvelopeParallelizationThreshold { get; set; } = 0;

    /// <summary>
    /// If true, additions of long lists of curves are processed in parallel
    /// </summary>
    public bool UseParallelListAddition { get; set; } = true;

    /// <summary>
    /// The addition of a list of curves is processed in parallel if it has at least this number of curves.
    /// </summary>
    public int ListAdditionParallelizationThreshold { get; set; } = 8;

    /// <summary>
    /// When <see cref="UseParallelListAddition"/> is enabled, this is the minimum number of curves for the complexity criterion.
    /// Their average number of base sequence elements must also reach <see cref="ListAdditionComplexityThreshold"/>.
    /// </summary>
    public int ListAdditionComplexityMinimumCount { get; set; } = 4;

    /// <summary>
    /// Minimum average number of base sequence elements per curve for the complexity criterion, which also requires at least <see cref="ListAdditionComplexityMinimumCount"/> curves.
    /// </summary>
    public int ListAdditionComplexityThreshold { get; set; } = 5_000;

    /// <summary>
    /// If true, minimums of long lists of curves are processed in parallel
    /// </summary>
    public bool UseParallelListMinimum { get; set; } = true;

    /// <summary>
    /// The minimum of a list of curves is processed in parallel if it has at least this number of curves.
    /// </summary>
    public int ListMinimumParallelizationThreshold { get; set; } = 8;

    /// <summary>
    /// When <see cref="UseParallelListMinimum"/> is enabled, this is the minimum number of curves for the complexity criterion.
    /// Their average number of base sequence elements must also reach <see cref="ListMinimumComplexityThreshold"/>.
    /// </summary>
    public int ListMinimumComplexityMinimumCount { get; set; } = 4;

    /// <summary>
    /// Minimum average number of base sequence elements per curve for the complexity criterion, which also requires at least <see cref="ListMinimumComplexityMinimumCount"/> curves.
    /// </summary>
    public int ListMinimumComplexityThreshold { get; set; } = 5_000;

    /// <summary>
    /// If true, maximums of long lists of curves are processed in parallel
    /// </summary>
    public bool UseParallelListMaximum { get; set; } = true;

    /// <summary>
    /// The maximum of a list of curves is processed in parallel if it has at least this number of curves.
    /// </summary>
    public int ListMaximumParallelizationThreshold { get; set; } = 8;

    /// <summary>
    /// When <see cref="UseParallelListMaximum"/> is enabled, this is the minimum number of curves for the complexity criterion.
    /// Their average number of base sequence elements must also reach <see cref="ListMaximumComplexityThreshold"/>.
    /// </summary>
    public int ListMaximumComplexityMinimumCount { get; set; } = 4;

    /// <summary>
    /// Minimum average number of base sequence elements per curve for the complexity criterion, which also requires at least <see cref="ListMaximumComplexityMinimumCount"/> curves.
    /// </summary>
    public int ListMaximumComplexityThreshold { get; set; } = 5_000;

    /// <summary>
    /// If true, (min,+) and (max,+) convolutions of long lists of curves are processed in parallel.
    /// </summary>
    public bool UseParallelListConvolution { get; set; } = true;

    /// <summary>
    /// The convolution of a list of curves is processed in parallel if it has more than this number of curves.
    /// Requires <see cref="UseParallelListConvolution"/>.
    /// </summary>
    public int ListConvolutionParallelizationThreshold { get; set; } = 8;

    /// <summary>
    /// The (max,+) convolution of a list of curves is processed in parallel if it has more than this number of curves.
    /// Requires <see cref="UseParallelListConvolution"/>.
    /// </summary>
    public int ListMaxPlusConvolutionParallelizationThreshold { get; set; } = 8;

    /// <summary>
    /// If true, minimums of long lists of curves are processed in parallel
    /// </summary>
    public bool UseParallelListLowerEnvelope { get; set; } = true;

    /// <summary>
    /// The lower envelope of a list of sequences is processed in parallel if it has more than this number of sequences.
    /// </summary>
    public int ListLowerEnvelopeParallelizationThreshold { get; set; } = 8;

    /// <summary>
    /// If true, maximums of long lists of curves are processed in parallel
    /// </summary>
    public bool UseParallelListUpperEnvelope { get; set; } = true;

    /// <summary>
    /// The upper envelope of a list of sequences is processed in parallel if it has more than this number of sequences.
    /// </summary>
    public int ListUpperEnvelopeParallelizationThreshold { get; set; } = 8;

    /// <summary>
    /// If true, long sequence convolutions are processed in parallel
    /// </summary>
    public bool UseParallelConvolution { get; set; } = true;

    /// <summary>
    /// Convolution parallelization is done if the number of element convolutions is above this threshold.
    /// </summary>
    public int ConvolutionParallelizationThreshold { get; set; } = 2_000;

    /// <summary>
    /// If true, long sequence convolutions are partitioned into smaller ones to reduce memory impact
    /// </summary>
    public bool UseConvolutionPartitioning { get; set; } = true;

    /// <summary>
    /// Convolution partitioning is done if the number of element convolutions is above this threshold.
    /// </summary>
    public int ConvolutionPartitioningThreshold { get; set; } = 50_000;

    /// <summary>
    /// If true, Interval.ComputeIntervals may use parallel implementation, including the sort of the endpoints.
    /// </summary>
    public bool UseParallelComputeIntervals { get; set; } = true;

    /// <summary>
    /// Interval.ComputeIntervals uses parallel endpoint collection and insertion if the number of elements is at least this threshold and <see cref="UseParallelComputeIntervals"/> is true.
    /// </summary>
    public int ParallelComputeIntervalsThreshold { get; set; } = 5_000;

    /// <summary>
    /// Distinct endpoints may be sorted in parallel, if <see cref="UseParallelComputeIntervals"/> is true, when they induce at least this number of interval buckets.
    /// Each endpoint induces a singleton bucket, and each consecutive pair induces an open bucket.
    /// </summary>
    public int SortIntervalsParallelizationThreshold { get; set; } = 10_000;

    /// <summary>
    /// In parallel Interval.ComputeIntervals, do element-in-interval insertion in parallel too.
    /// </summary>
    public bool UseParallelInsertionComputeIntervals { get; set; } = true;

    /// <summary>
    /// If true, long sequence convolutions are processed in parallel
    /// </summary>
    public bool UseParallelDeconvolution { get; set; } = true;

    /// <summary>
    /// Deconvolution parallelization is done if the number of element deconvolutions is above this threshold.
    /// </summary>
    public int DeconvolutionParallelizationThreshold { get; set; } = 1_000;

    /// <summary>
    /// Enable parallel translation of complete pseudo-periods in materialized curve cuts and extensions.
    /// </summary>
    public bool UseParallelExtend { get; set; } = true;

    /// <summary>
    /// Minimum number of complete pseudo-periods for outer parallel execution in materialized curve cuts and extensions.
    /// Requires <see cref="UseParallelExtend"/>.
    /// </summary>
    /// <remarks>Parallel execution also depends on <see cref="ExtensionMinimumElementsPerPartition"/> and available parallelism.</remarks>
    public int ExtendParallelizationThreshold { get; set; } = 0;

    /// <summary>
    /// Enable parallel translation of elements within a pseudo-period.
    /// </summary>
    public bool UseParallelComputeExtensionSequences { get; set; } = true;

    /// <summary>
    /// Minimum number of selected period elements for inner parallel execution in curve cuts and extensions, materialized or streamed.
    /// Requires <see cref="UseParallelComputeExtensionSequences"/>.
    /// </summary>
    /// <remarks>Parallel execution also depends on <see cref="ExtensionMinimumElementsPerPartition"/> and available parallelism.</remarks>
    public int ComputeExtensionSequencesParallelizationThreshold { get; set; } = 0;

    /// <summary>
    /// Minimum average number of translated elements per parallel work partition in curve extensions.
    /// </summary>
    /// <remarks>
    /// The extension parallelism switches and thresholds still govern eligibility.
    /// Values less than one are treated as one.
    /// </remarks>
    public int ExtensionMinimumElementsPerPartition { get; set; } = 1_024;

    /// <summary>
    /// In Extensions.SortElements, sort the elements in parallel.
    /// </summary>
    public bool UseParallelSortElements { get; set; } = true;

    /// <summary>
    /// Elements are sorted in parallel, if <see cref="UseParallelSortElements"/> is true, when there are at least this number of them.
    /// </summary>
    public int SortElementsParallelizationThreshold { get; set; } = 10_000;

    /// <summary>
    /// Enables parallel computation of the element closures in <see cref="Curve.SubAdditiveClosure(ComputationSettings)"/>.
    /// </summary>
    public bool UseParallelSubAdditiveClosure { get; set; } = false;

    /// <summary>
    /// Minimum number of base-sequence elements for parallel computation of element closures.
    /// Requires <see cref="UseParallelSubAdditiveClosure"/>.
    /// </summary>
    public int SubAdditiveClosureParallelizationThreshold { get; set; } = 0;

    /// <summary>
    /// If true, when two Curves have the same slope the convolution is done in a single pass
    /// </summary>
    public bool SinglePassConvolution { get; set; } = true;

    /// <summary>
    /// If set, results of each operation are optimized towards minimal representation
    /// </summary>
    public bool UseRepresentationMinimization { get; set; } = true;

    /// <summary>
    /// If set, convolutions between sub-additive curves are optimized
    /// </summary>
    public bool UseSubAdditiveConvolutionOptimizations { get; set; } = true;

    /// <summary>
    /// If set, the convolution with a curve that is finite only at the origin is shortcut to a vertical shift of the other operand.
    /// Applies to both the (min,+) and the (max,+) convolution.
    /// </summary>
    public bool UseOriginConvolutionShortcut { get; set; } = true;

    /// <summary>
    /// If set, the four closures ($\mathrm{UND}$, $\mathrm{LND}$, $\mathrm{UNI}$, $\mathrm{LNI}$) are computed by bounding each breakpoint that would break their monotonicity, rather than through the algebraic method.
    /// </summary>
    /// <remarks>
    /// Bounding the breakpoints costs with the number of offending breakpoints, the algebraic method with the size of the representation.
    /// </remarks>
    public bool UseNonDecreasingClosureOptimizations { get; set; } = true;

    /// <summary>
    /// If set, the convolution with the zero curve is shortcut to the constant value at the origin when the other operand is non-decreasing.
    /// </summary>
    public bool UseZeroConvolutionShortcut { get; set; } = true;

    /// <summary>
    /// If set, the convolution of a delay curve with a non-decreasing curve through the origin is shortcut to a horizontal shift.
    /// </summary>
    public bool UseDelayConvolutionShortcut { get; set; } = true;

    /// <summary>
    /// If set, the deconvolution of a non-decreasing curve through the origin by a delay curve is shortcut to a horizontal shift.
    /// </summary>
    public bool UseDelayDeconvolutionShortcut { get; set; } = true;

    /// <summary>
    /// If set, a vertical shift by $+\infty$ is shortcut to a curve that is $+\infty$ wherever it is shifted, rather than shifting each element of the curve.
    /// Applies to <see cref="Curve.VerticalShift(Unipi.Nancy.Numerics.Rational, bool, ComputationSettings?)"/>.
    /// </summary>
    public bool UseInfiniteVerticalShiftShortcut { get; set; } = true;

    /// <summary>
    /// Enables returning the zero curve directly when the closure is known to be zero.
    /// For <see cref="Curve.SubAdditiveClosure(ComputationSettings?)"/>, this applies when the operand $f$ is non-negative and $f(t) = 0$ on $(0, \varepsilon)$ for some $\varepsilon > 0$, [TBP-CLOS-NEG] Theorem 5.2.
    /// For <see cref="Curve.SuperAdditiveClosure(ComputationSettings?)"/>, the same zero-interval condition applies with $f$ non-positive, [TBP-CLOS-NEG] Corollary 5.7.
    /// </summary>
    public bool UseZeroClosureShortcut { get; set; } = true;

    /// <exclude />
    /// <summary>
    /// If set, optimized convolution of sub-additive curves is used even if the operands are not finite for any $t$.
    /// Mostly used for testing.
    /// </summary>
    public bool UseMinimumSelfConvolutionForCurvesWithInfinities { get; set; } = true;

    /// <exclude />
    /// <summary>
    /// If set, the composition operator is optimized when one or both operands are <see cref="Curve.IsUltimatelyAffine"/>
    /// </summary>
    public bool UseCompositionOptimizations { get; set; } = true;

    /// <summary>
    /// The algorithm by which <see cref="Curve.HorizontalDeviation(Curve, Curve, ComputationSettings?)"/> and <see cref="Sequence.HorizontalDeviation(Sequence, Sequence, ComputationSettings?)"/> compute the horizontal deviation.
    /// </summary>
    public HorizontalDeviationStrategy HorizontalDeviationStrategy { get; set; } = HorizontalDeviationStrategy.PseudoInverseGap;

    /// <exclude/>
    /// <summary>
    /// If true, isomorphism properties between min-plus and max-plus are used to optimize
    /// <see cref="Curve.Convolution(Unipi.Nancy.MinPlusAlgebra.Curve,Unipi.Nancy.MinPlusAlgebra.ComputationSettings?)"/> and
    /// <see cref="Curve.MaxPlusConvolution(Unipi.Nancy.MinPlusAlgebra.Curve,Unipi.Nancy.MinPlusAlgebra.ComputationSettings?)"/>.
    /// </summary>
    /// <remarks>Optimization discussed in [ZNS23a].</remarks>
    public bool UseConvolutionIsospeedOptimization { get; set; } = true;

    /// <exclude/>
    /// <summary>
    /// If true, the additional isomorphism properties from [ZNS26] are used as well to optimize     
    /// <see cref="Curve.Convolution(Unipi.Nancy.MinPlusAlgebra.Curve,Unipi.Nancy.MinPlusAlgebra.ComputationSettings?)"/> and
    /// <see cref="Curve.MaxPlusConvolution(Unipi.Nancy.MinPlusAlgebra.Curve,Unipi.Nancy.MinPlusAlgebra.ComputationSettings?)"/>.
    /// </summary>
    /// <remarks>Optimization discussed in [ZNS26].</remarks>
    public bool UseConvolutionSuperIsospeedOptimization { get; set; } = true;

    /// <exclude/>
    /// <summary>
    /// If true, isomorphism properties between min-plus and max-plus are used to optimize
    /// <see cref="Sequence.Convolution(Unipi.Nancy.MinPlusAlgebra.Sequence,Unipi.Nancy.MinPlusAlgebra.Sequence,Unipi.Nancy.MinPlusAlgebra.ComputationSettings?,System.Nullable{Unipi.Nancy.Numerics.Rational},System.Nullable{Unipi.Nancy.Numerics.Rational},bool,bool,bool)"/> and
    /// <see cref="Sequence.MaxPlusConvolution(Unipi.Nancy.MinPlusAlgebra.Sequence,Unipi.Nancy.MinPlusAlgebra.Sequence,Unipi.Nancy.MinPlusAlgebra.ComputationSettings?,System.Nullable{Unipi.Nancy.Numerics.Rational},System.Nullable{Unipi.Nancy.Numerics.Rational},bool,bool,bool)"/>.
    /// </summary>
    /// <remarks>Optimization discussed in [ZNS23a], refined in [ZNS26].</remarks>
    public bool UseBySequenceConvolutionIsospeedOptimization { get; set; } = true;

    /// <summary>
    /// Default settings
    /// </summary>
    public static ComputationSettings Default()
    {
        return _default ??= new ComputationSettings{};
    }

    /// <summary>
    /// Cached instance of default settings
    /// </summary>
    private static ComputationSettings? _default = null;
}
