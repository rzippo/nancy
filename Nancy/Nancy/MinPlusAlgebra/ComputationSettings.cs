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
                UseParallelLowerEnvelope || UseParallelConvolution ||
                UseParallelListConvolution || UseParallelListMinimum || UseParallelListAddition || UseParallelListLowerEnvelope ||
                UseParallelListMaximum || UseParallelListUpperEnvelope ||
                UseParallelExtend || UseParallelComputeExtensionSequences ||
                UseParallelComputeIntervals || UseParallelInsertionComputeIntervals || UseParallelSortElements;
        }
        set 
        {
            UseParallelLowerEnvelope = value;
            UseParallelConvolution = value;
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
        }
    }

    /// <summary>
    /// If true, long sequence minimums are processed in parallel.
    /// </summary>
    public bool UseParallelLowerEnvelope { get; set; } = true;

    /// <summary>
    /// If true, long sequence maximums are processed in parallel.
    /// </summary>
    public bool UseParallelUpperEnvelope { get; set; } = true;

    /// <summary>
    /// If true, additions of long lists of curves are processed in parallel
    /// </summary>
    public bool UseParallelListAddition { get; set; } = true;

    /// <summary>
    /// If true, minimums of long lists of curves are processed in parallel
    /// </summary>
    public bool UseParallelListMinimum { get; set; } = true;

    /// <summary>
    /// If true, maximums of long lists of curves are processed in parallel
    /// </summary>
    public bool UseParallelListMaximum { get; set; } = true;

    /// <summary>
    /// If true, convolutions of long lists of curves are processed in parallel
    /// </summary>
    public bool UseParallelListConvolution { get; set; } = true;

    /// <summary>
    /// If true, minimums of long lists of curves are processed in parallel
    /// </summary>
    public bool UseParallelListLowerEnvelope { get; set; } = true;

    /// <summary>
    /// If true, maximums of long lists of curves are processed in parallel
    /// </summary>
    public bool UseParallelListUpperEnvelope { get; set; } = true;

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
    /// If true, Interval.ComputeIntervals may use parallel implementation
    /// </summary>
    public bool UseParallelComputeIntervals { get; set; } = true;

    /// <summary>
    /// Interval.ComputeIntervals is done with parallel implementation if the number of elements is above this threshold.
    /// </summary>
    public int ParallelComputeIntervalsThreshold { get; set; } = 5_000;

    /// <summary>
    /// In parallel Interval.ComputeIntervals, do element-in-interval insertion in parallel too.
    /// </summary>
    public bool UseParallelInsertionComputeIntervals { get; set; } = true;

    /// <summary>
    /// In Curve.Extend, compute extension sequences in parallel.
    /// </summary>
    public bool UseParallelExtend { get; set; } = true;

    /// <summary>
    /// In Curve.ComputeExtensionSequences, compute elements of the sequence in parallel.
    /// </summary>
    public bool UseParallelComputeExtensionSequences { get; set; } = true;

    /// <summary>
    /// In Extensions.SortElements, sort the elements in parallel.
    /// </summary>
    public bool UseParallelSortElements { get; set; } = true;

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

    /// <exclude/>
    /// <summary>
    /// If true, isomorphism properties between min-plus and max-plus are used to optimize
    /// <see cref="Curve.Convolution(Unipi.Nancy.MinPlusAlgebra.Curve,Unipi.Nancy.MinPlusAlgebra.ComputationSettings?)"/> and
    /// <see cref="Curve.MaxPlusConvolution(Unipi.Nancy.MinPlusAlgebra.Curve,Unipi.Nancy.MinPlusAlgebra.ComputationSettings?)"/>.
    /// </summary>
    /// <remarks>Optimization discussed in [ZNS23a].</remarks>
    public bool UseConvolutionIsomorphismOptimization { get; set; } = true;

    /// <exclude/>
    /// <summary>
    /// If true, isomorphism properties between min-plus and max-plus are used to optimize
    /// <see cref="Sequence.Convolution(Unipi.Nancy.MinPlusAlgebra.Sequence,Unipi.Nancy.MinPlusAlgebra.Sequence,Unipi.Nancy.MinPlusAlgebra.ComputationSettings?,System.Nullable{Unipi.Nancy.Numerics.Rational},System.Nullable{Unipi.Nancy.Numerics.Rational},bool,bool,bool)"/> and
    /// <see cref="Sequence.MaxPlusConvolution(Unipi.Nancy.MinPlusAlgebra.Sequence,Unipi.Nancy.MinPlusAlgebra.Sequence,Unipi.Nancy.MinPlusAlgebra.ComputationSettings?,System.Nullable{Unipi.Nancy.Numerics.Rational},System.Nullable{Unipi.Nancy.Numerics.Rational},bool,bool,bool)"/>.
    /// </summary>
    /// <remarks>Optimization discussed in [ZNS23a].</remarks>
    public bool UseBySequenceConvolutionIsomorphismOptimization { get; set; } = true;

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