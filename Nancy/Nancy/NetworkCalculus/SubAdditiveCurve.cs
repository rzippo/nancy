using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

#if DO_LOG
using NLog;
using System.Diagnostics;
#endif

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// Used to represent curves that are known to be sub-additive with $f(0) = 0$ (see <see cref="Curve.IsRegularSubAdditive"/>),
/// and exploit these properties to optimize computations.
/// </summary>
/// <remarks>
/// $f(0) = 0$ is required for the curve to be <see cref="Curve.IsRegularSubAdditive"/> and 
/// provides optimized algorithms for convolution as described in [ZS22],
/// but is not required for sub-additivity to be stable on addition and convolution.
/// To keep the type system simple for the common cases of NC, we opted not to support non-regular sub-additive functions with their own type.
/// </remarks>
public class SubAdditiveCurve : Curve
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="baseSequence">Describes the curve from 0 to <see cref="Curve.PseudoPeriodStart"/> + <see cref="Curve.PseudoPeriodLength"/>.</param>
    /// <param name="pseudoPeriodStart">Instant after which the curve is pseudo-periodic.</param>
    /// <param name="pseudoPeriodLength">Length of each pseudo-period.</param>
    /// <param name="pseudoPeriodHeight">Step gained after each pseudo-period.</param>
    /// <param name="doTest">
    /// If true, the sub-additive property is tested.
    /// This test can be computationally expensive.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// If <paramref name="doTest"/> is true and the sub-additive property was not successfully verified.
    /// </exception>
    public SubAdditiveCurve(Sequence baseSequence, Rational pseudoPeriodStart, Rational pseudoPeriodLength,
        Rational pseudoPeriodHeight, bool doTest = true)
        : base(baseSequence, pseudoPeriodStart, pseudoPeriodLength, pseudoPeriodHeight)
    {
        if (doTest && !base.IsRegularSubAdditive)
            throw new InvalidOperationException("The curve constructed is not actually sub-additive with f(0) = 0");
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="other">The <see cref="Curve"/> object to copy from.</param>
    /// <param name="doTest">
    /// If true, the sub-additive property is tested.
    /// This test can be computationally expensive.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// If <paramref name="doTest"/> is true and the sub-additive property was not successfully verified.
    /// </exception>
    public SubAdditiveCurve(Curve other, bool doTest = true)
        : base(other)
    {
        if (doTest && !base.IsRegularSubAdditive)
            throw new InvalidOperationException("The curve constructed is not actually sub-additive with f(0) = 0");
    }

    /// <summary>
    /// True if the curve is sub-additive.
    /// </summary>
    /// <remarks>
    /// For a <see cref="SubAdditiveCurve"/> this will always return true, without performing any checks.
    /// </remarks>
    public override bool IsSubAdditive => true;

    /// <summary>
    /// Forced check for sub-additive property.
    /// </summary>
    /// <remarks>
    /// Can be computationally expensive the first time it is invoked, the result is cached afterwards.
    /// </remarks>
    public bool IsSubAdditiveCheck()
    {
        return base.IsSubAdditive;
    }

    /// <summary>
    /// Forced check for sub-additive property with f(0) = 0.
    /// </summary>
    /// <remarks>
    /// Can be computationally expensive the first time it is invoked, the result is cached afterwards.
    /// </remarks>
    public bool IsRegularSubAdditiveCheck()
    {
        return base.IsSubAdditive && ValueAt(0) == 0;
    }

    /// <inheritdoc />
    public override SubAdditiveCurve SubAdditiveClosure(ComputationSettings? settings = null)
    {
        return this;
    }

    #region Convolution

    /// <summary>
    /// Computes the convolution of the two curves.
    /// </summary>
    /// <remarks>
    /// Default <see cref="ComputationSettings"/> are used. 
    /// For more control over the algorithms used, use explicit <see cref="Curve.Convolution(Curve, Curve, ComputationSettings?)"/> instead.
    /// </remarks>
    public static SubAdditiveCurve operator *(SubAdditiveCurve c1, SubAdditiveCurve c2)
    {
        return c1.Convolution(c2);
    }

    /// <summary>
    /// Computes the convolution of the two curves.
    /// </summary>
    /// <remarks>
    /// Attempts to optimize the computations using theorems in [ZS22].
    /// </remarks>
    public override Curve Convolution(Curve curve, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        if(settings.UseSubAdditiveConvolutionOptimizations && curve is SubAdditiveCurve sa)
        {
            return Convolution(sa, settings);
        }
        else
        {
            //todo: in this case, the convolution may still be optimized with th 1 and 2, although the result is not sub-additive
            //sub-additive x generic
            return base.Convolution(curve, settings);
        }
    }

    /// <summary>
    /// Computes the convolution of the two curves.
    /// </summary>
    /// <remarks>
    /// Attempts to optimize the computations using theorems in [ZS22].
    /// </remarks>    
    public SubAdditiveCurve Convolution(SubAdditiveCurve curve, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        #if DO_LOG
        var stopwatch = Stopwatch.StartNew();
        #endif

        var minimum = Curve.Minimum(this, curve, settings with {UseRepresentationMinimization = false}).PeriodFactorization(); // we need a stable T for th. 2
        bool isThisLower = Curve.Equivalent(this, minimum, settings); // this <= curve
        bool isCurveLower = Curve.Equivalent(curve, minimum, settings); // curve <= this

        if (isThisLower || isCurveLower) // this <= curve || curve <= this
        {
            // Use theorem 1 [3]
            #if DO_LOG
            logger.Trace($"Optimized convolution between sub-additive curves: complete skip." +
                         $"{this.GetHashCode():X} of {this.BaseSequence.Count} elements " +
                         $"and {curve.GetHashCode():X} of {curve.BaseSequence.Count} elements");
            #endif

            if (isThisLower)
                return this;
            else
                return curve;
        }
        else 
        {
            if(
                this.PseudoPeriodSlope != curve.PseudoPeriodSlope ||
                this.Match(minimum.PseudoPeriodicSequence, settings) ||
                curve.Match(minimum.PseudoPeriodicSequence, settings)
            )
            {
                // Use theorem 3.5.8 [3]
                #if DO_LOG
                logger.Trace($"Optimized convolution between sub-additive curves: partial skip." +
                             $"{this.GetHashCode():X} of {this.BaseSequence.Count} elements " +
                             $"and {curve.GetHashCode():X} of {curve.BaseSequence.Count} elements");
                #endif

                SubAdditiveCurve higher, lower;
                if (this.PseudoPeriodSlope < curve.PseudoPeriodSlope)
                {
                    lower = this;
                    higher = curve;
                }
                else if (this.PseudoPeriodSlope > curve.PseudoPeriodSlope)
                {
                    lower = curve;
                    higher = this;
                }
                else if (this.Match(minimum.PseudoPeriodicSequence))
                {
                    lower = this;
                    higher = curve;
                }
                else
                {
                    lower = curve;
                    higher = this;
                }

                var higher_a = new Curve(
                    baseSequence: higher.Cut(0, minimum.PseudoPeriodStart, settings: settings),
                    pseudoPeriodStart: minimum.PseudoPeriodStart,
                    pseudoPeriodLength: lower.PseudoPeriodLength,
                    pseudoPeriodHeight: 0,
                    isPartialCurve: true
                );

                // Compare costs, do generic algorithm if costs less
                if (EstimateConvolution(lower, higher_a, false, settings) <=
                    EstimateConvolution(lower, higher, false, settings with { UseSubAdditiveConvolutionOptimizations = false }))
                {
                    return new SubAdditiveCurve(
                        Curve.Minimum(
                            Curve.Convolution(lower, higher_a, settings),
                            lower,
                            settings
                        ),
                        false
                    );
                }
                else
                {
                    return new SubAdditiveCurve(base.Convolution(curve, settings), false);
                }
            }
            else if(
                this.ValueAt(0) == 0 && curve.ValueAt(0) == 0 &&
                (settings.UseMinimumSelfConvolutionForCurvesWithInfinities || (this.IsFinite && curve.IsFinite))        
            )
            {
                // Use theorem 3 and related properties [3]
                return SpecializedConvolution(this, curve, minimum.TransientReduction(), settings);
            }
        }

        // if all else fails, use the costly generic algorithm
        #if DO_LOG
        stopwatch.Stop();
        logger.Trace($"Optimization failed, wasted {stopwatch.Elapsed}");
        #endif
        return new SubAdditiveCurve(base.Convolution(curve, settings), false);
    }

    private static SubAdditiveCurve SpecializedConvolution(SubAdditiveCurve a, SubAdditiveCurve b, Curve minimum, ComputationSettings? settings)
    {
        settings ??= ComputationSettings.Default();

        var d = Rational.Min(
            minimum.PseudoPeriodLength,
            Rational.LeastCommonMultiple(a.PseudoPeriodLength, b.PseudoPeriodLength)
        );
        var T = Rational.Min(
            2*minimum.PseudoPeriodStart, 
            a.PseudoPeriodStart + b.PseudoPeriodStart) + d;
        var c = d * minimum.PseudoPeriodSlope;

        #if DO_LOG
        logger.Debug($"SpecializedConvolution: extending from T_min {minimum.PseudoPeriodStart} d_min {minimum.PseudoPeriodLength} to T {T} d {d}");
        #endif

        var cutEnd = T + d;
        var minimumCut = minimum.Cut(0, cutEnd, settings: settings);

        #if DO_LOG
        var coloringStopwatch = Stopwatch.StartNew();
        #endif
        var colors = minimumCut.Elements
            .Select(element =>
            {
                if(a.Match(element, settings))
                    return Color.A;
                else if (b.Match(element, settings))
                    return Color.B;
                else
                    return Color.Both;
            })
            .ToList();
        #if DO_LOG
        coloringStopwatch.Stop();
        logger.Trace($"SpecializedConvolution: coloring took {coloringStopwatch.Elapsed}");
        #endif

        var convolutionSequence = SpecializedSequenceConvolution(minimumCut, minimumCut);
        var result = new SubAdditiveCurve(
            baseSequence: convolutionSequence.Cut(0, cutEnd),
            pseudoPeriodStart: T,
            pseudoPeriodLength: d,
            pseudoPeriodHeight: c
        );
        return settings.UseRepresentationMinimization ? new SubAdditiveCurve(result.Optimize(), false) : result;

        // a and b are expected to be either the same, or a is a partition of b
        Sequence SpecializedSequenceConvolution(Sequence sa, Sequence sb, int startIndexOfA = 0)
        {
            #if DO_LOG
            var countStopwatch = Stopwatch.StartNew();
            #endif
            var pairsCount = GetElementPairs().Count();
            #if DO_LOG
            countStopwatch.Stop();
            logger.Trace($"Specialized Convolution: counted {pairsCount} pairs in {countStopwatch.Elapsed}");
            #endif

            if (settings.UseConvolutionPartitioning && pairsCount > settings.ConvolutionPartitioningThreshold)
                return PartitionedConvolution();
            else
            if (settings.UseParallelConvolution && pairsCount > settings.ConvolutionParallelizationThreshold)
                return ParallelConvolution();
            else
                return SerialConvolution();

            IEnumerable<(Element ea, Element eb)> GetElementPairs()
            {
                return sa.Elements
                    .SelectMany((ea, ia) => sb.Elements
                        .Where((_, ib) => colors[startIndexOfA + ia] != colors[ib] || colors[ib] == Color.Both)    // filter out same-color pairs
                        .Where(eb => ea.StartTime < eb.StartTime)   // filter out symmetric pairs
                        .Where(eb => ea.StartTime + eb.StartTime < cutEnd) // filter out pairs outside the cut boundary
                        .Select(eb => (a: ea, b: eb))
                    )
                    .Where(pair => pair.a.IsFinite && pair.b.IsFinite)
                    .Concat(
                        minimumCut.Elements
                            .Select<Element,(Element,Element)>(e => (Point.Origin(), e))
                    );
            }

            Sequence SerialConvolution()
            {
                #if DO_LOG
                logger.Trace($"Running serial specialized-convolution, {pairsCount} pairs.");
                #endif
                var convolutionElements = GetElementPairs() 
                    .SelectMany(pair => pair.ea.Convolution(pair.eb))
                    .ToList();

                if (sa.IsFinite && sb.IsFinite)
                {
                    return convolutionElements
                        .LowerEnvelope(settings)
                        .ToSequence();
                }
                else
                {
                    // gaps may be expected
                    return convolutionElements
                        .LowerEnvelope(settings)
                        .ToSequence(
                            fillFrom: sa.DefinedFrom + sb.DefinedFrom,
                            fillTo: sa.DefinedUntil + sb.DefinedUntil
                        );
                }
            }

            Sequence ParallelConvolution()
            {
                #if DO_LOG
                logger.Trace($"Running parallel specialized-convolution, {pairsCount} pairs.");
                #endif
                var convolutionElements = GetElementPairs()
                    .AsParallel()
                    .SelectMany(pair => pair.ea.Convolution(pair.eb))                        
                    .ToList();

                if (sa.IsFinite && sb.IsFinite)
                {
                    return convolutionElements
                        .LowerEnvelope(settings)
                        .ToSequence();
                }
                else
                {
                    // gaps may be expected
                    return convolutionElements
                        .LowerEnvelope(settings)
                        .ToSequence(
                            fillFrom: sa.DefinedFrom + sb.DefinedFrom,
                            fillTo: sa.DefinedUntil + sb.DefinedUntil
                        );
                }
            }

            // The elementPairs are partitioned in smaller sets (without any ordering)
            // From each set, the convolutions are computed and then their lower envelope
            // The resulting sequences will have gaps
            // Those partial convolutions are then merged via Sequence.LowerEnvelope
            Sequence PartitionedConvolution()
            {
                #if DO_LOG
                logger.Trace($"Running partitioned specialized-convolution, {pairsCount} pairs.");
                #endif

                var partialConvolutions = PartitionConvolutionElements()
                    .Select(elements => elements
                        .LowerEnvelope(settings)
                        .ToSequence(
                            fillFrom: 0,
                            fillTo: cutEnd
                        )
                    )
                    .ToList();

                #if DO_LOG
                logger.Trace($"Partitioned convolutions computed, proceding with lower envelope of {partialConvolutions.Count} sequences");
                #endif

                if (sa.IsFinite && sb.IsFinite)
                {
                    return partialConvolutions
                        .LowerEnvelope(settings)
                        .ToSequence();
                }
                else
                {
                    // gaps may be expected
                    return partialConvolutions
                        .LowerEnvelope(settings)
                        .ToSequence(
                            fillFrom: sa.DefinedFrom + sb.DefinedFrom,
                            fillTo: sa.DefinedUntil + sb.DefinedUntil
                        );
                }

                IEnumerable<IReadOnlyList<Element>> PartitionConvolutionElements()
                {
                    int partitionsCount = (int)
                        Math.Ceiling((double)pairsCount / settings.ConvolutionPartitioningThreshold);
                    #if DO_LOG
                    logger.Trace($"Partitioning {pairsCount} pairs in {partitionsCount} chunks of {settings.ConvolutionPartitioningThreshold}.");
                    #endif

                    var partitions = GetElementPairs()
                        .Chunk(settings.ConvolutionPartitioningThreshold);

                    foreach (var partition in partitions)
                    {
                        IReadOnlyList<Element> convolutionElements;
                        if (settings.UseParallelConvolution)
                        {
                            convolutionElements = partition
                                .AsParallel()
                                .SelectMany(pair => pair.ea.Convolution(pair.eb))
                                .ToList()
                                .SortElements(settings);    
                        }
                        else
                        {
                            convolutionElements = partition
                                .SelectMany(pair => pair.ea.Convolution(pair.eb))
                                .ToList()
                                .SortElements(settings);
                        }

                        yield return convolutionElements;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Computes the convolution of a set of sub-additive curves.
    /// Order of operations is optimized to exploit theorems in [ZS22]
    /// </summary>
    /// <param name="curves">The set of sub-additive curves to be convolved.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the overall convolution.</returns>
    public static SubAdditiveCurve Convolution(IEnumerable<SubAdditiveCurve> curves, ComputationSettings? settings = null)
    {
        #if DO_LOG
        logger.Debug("Specialized list convolution");
        #endif
        var ordered = curves
            .OrderByDescending(c => c.RightLimitAt(0))
            .ThenByDescending(c => c.PseudoPeriodSlope)
            .ToList();
        var current = ordered.Last();   //initialize current with the candidate lowest curve

        int i = 1;
        foreach (var curve in ordered.Take(ordered.Count - 1))
        {
            #if DO_LOG
            logger.Debug($"Convolution #{i}, current size {current.BaseSequence.Count}");
            logger.Trace($"\n {current}\n {curve}");
            var timer = Stopwatch.StartNew();
            #endif
            current = current.Convolution(curve, settings);
            #if DO_LOG
            timer.Stop();
            logger.Debug($"Convolution #{i}, took {timer.Elapsed}");
            #endif
            i++;
        }
        return current;
    }

    private enum Color
    {
        /** The element belongs to A */
        A, 
        /** The element belongs to B */
        B, 
        /** Used in those cases where multiple segments from both A and B align to form a larger segment in min(A,B).  */
        Both
    }

    #endregion

    #region EstimateConvolution

    /// <inheritdoc />
    public override long EstimateConvolution(Curve curve, bool countElements = false, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        if (settings.UseSubAdditiveConvolutionOptimizations && curve is SubAdditiveCurve sa)
        {
            return EstimateConvolution(sa, countElements, settings);
        }
        else
        {
            //todo: in this case, the convolution may still be optimized with th 1 and 2, although the result is not sub-additive
            //sub-additive x generic
            return base.EstimateConvolution(curve, countElements, settings);    
        }
    }

    /// <summary>
    /// Computes the number of elementary convolutions involved in computing the convolution of the two curves,
    /// avoiding allocations as much as possible.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="countElements">If true, instead of counting only how many convolutions are done, it counts how many convolution elements are produced.</param>
    /// <param name="settings"></param>
    /// <returns>
    /// The number of elementary convolutions involved in computing the result of the convolution,
    /// or the number of elements resulting from these convolutions if <paramref name="countElements"/> is true
    /// </returns>
    /// <remarks>
    /// Attempts to optimize the computations using theorems in [ZS22]
    /// </remarks>
    public long EstimateConvolution(SubAdditiveCurve curve, bool countElements = false, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();

        var minimum = Curve.Minimum(this, curve, settings with {UseRepresentationMinimization = false}).PeriodFactorization(); // we need a stable T for th. 2);
        bool isThisLower = Curve.Equivalent(this, minimum, settings); // this <= curve
        bool isCurveLower = Curve.Equivalent(curve, minimum, settings); // curve <= this

        if (isThisLower || isCurveLower) // this <= curve || curve <= this
        {
            // Use theorem 1 [3]
            #if DO_LOG
            logger.Trace($"Optimized convolution between sub-additive curves: complete skip." +
                         $"{this.GetHashCode().ToString("X")} of {this.BaseSequence.Count} elements " +
                         $"and {curve.GetHashCode().ToString("X")} of {curve.BaseSequence.Count} elements");
            #endif

            return 0;
        }
        else 
        {
            if(
                this.PseudoPeriodSlope != curve.PseudoPeriodSlope ||
                this.Match(minimum.PseudoPeriodicSequence, settings) ||
                curve.Match(minimum.PseudoPeriodicSequence, settings)
            )
            {
                // Use theorem 3.5.8 [3]
                #if DO_LOG
                logger.Trace($"Optimized convolution between sub-additive curves: partial skip." +
                             $"{this.GetHashCode():X} of {this.BaseSequence.Count} elements " +
                             $"and {curve.GetHashCode():X} of {curve.BaseSequence.Count} elements");
                #endif

                SubAdditiveCurve higher, lower;
                if (this.PseudoPeriodSlope < curve.PseudoPeriodSlope)
                {
                    lower = this;
                    higher = curve;
                }
                else if (this.PseudoPeriodSlope > curve.PseudoPeriodSlope)
                {
                    lower = curve;
                    higher = this;
                }
                else if (this.Match(minimum.PseudoPeriodicSequence))
                {
                    lower = this;
                    higher = curve;
                }
                else
                {
                    lower = curve;
                    higher = this;
                }

                var higher_a = new Curve(
                    baseSequence: higher.Cut(0, minimum.PseudoPeriodStart, settings: settings),
                    pseudoPeriodStart: minimum.PseudoPeriodStart,
                    pseudoPeriodLength: lower.PseudoPeriodLength,
                    pseudoPeriodHeight: 0,
                    isPartialCurve: true
                );

                return Curve.EstimateConvolution(lower, higher_a, countElements, settings);
            }
            else if(
                this.ValueAt(0) == 0 && curve.ValueAt(0) == 0 &&
                (settings.UseMinimumSelfConvolutionForCurvesWithInfinities || (this.IsFinite && curve.IsFinite))        
            )
            {
                // Use theorem 3 and related properties [3]
                return SpecializedEstimateConvolution(this, curve, minimum, countElements, settings);
            }
        }

        // if all else fails, use the costly generic algorithm
        return base.EstimateConvolution(curve, countElements, settings);
    }

    private static long SpecializedEstimateConvolution(SubAdditiveCurve a, SubAdditiveCurve b, Curve minimum, bool countElements = false, ComputationSettings? settings = null)
    {
        var d = Rational.Min(
            minimum.PseudoPeriodLength,
            Rational.LeastCommonMultiple(a.PseudoPeriodLength, b.PseudoPeriodLength)
        );
        var T = Rational.Min(
            2*minimum.PseudoPeriodStart, 
            a.PseudoPeriodStart + b.PseudoPeriodStart) + d;
        //var c = d * minimum.PseudoPeriodSlope;

        #if DO_LOG
        logger.Debug($"SpecializedEstimateConvolution: extending from T_min {minimum.PseudoPeriodStart} d_min {minimum.PseudoPeriodLength} to T {T} d {d}");
        #endif

        var minimumCut = minimum.Cut(0, T + d);

        var cutEnd = T + d;

        #if DO_LOG
        var coloringStopwatch = Stopwatch.StartNew();
        #endif
        var colors = minimumCut.Elements
            .Select(element =>
            {
                if(a.Match(element))
                    return Color.A;
                else if (b.Match(element))
                    return Color.B;
                else
                    return Color.Both;
            })
            .ToList();
        #if DO_LOG
        coloringStopwatch.Stop();
        logger.Trace($"SpecializedEstimateConvolution: coloring took {coloringStopwatch.Elapsed}");
        #endif

        var result = SpecializedEstimateSequenceConvolution(minimumCut, minimumCut);
        return result;

        // a and b are expected to be either the same, or a is a partition of b
        long SpecializedEstimateSequenceConvolution(Sequence sa, Sequence sb, int startIndexOfA = 0)
        {
            if (!countElements)
            {
                #if DO_LOG
                var countStopwatch = Stopwatch.StartNew();
                #endif
                var pairsCount = GetElementPairs().LongCount();
                #if DO_LOG
                countStopwatch.Stop();
                logger.Debug(
                    $"Specialized Estimate Convolution: counted {pairsCount} pairs in {countStopwatch.Elapsed}");
                #endif
                return pairsCount;
            }
            else
            {
                #if DO_LOG
                var deepCountStopwatch = Stopwatch.StartNew();
                #endif
                var deepCount = GetElementPairs()
                    .SelectMany(p => p.ea.Convolution(p.eb))
                    .Count();
                #if DO_LOG
                deepCountStopwatch.Stop();
                logger.Debug(
                    $"Specialized Estimate Convolution: counted {deepCount} total elements in {deepCountStopwatch.Elapsed}");
                #endif
                return deepCount;
            }

            IEnumerable<(Element ea, Element eb)> GetElementPairs()
            {
                return sa.Elements
                    .SelectMany((ea, ia) => sb.Elements
                        .Where((_, ib) => colors[startIndexOfA + ia] != colors[ib])    // filter out same-color pairs
                        .Where(eb => ea.StartTime < eb.StartTime)   // filter out symmetric pairs
                        .Where(eb => ea.StartTime + eb.StartTime <= cutEnd) // filter out pairs outside the cut boundary
                        .Select(eb => (a: ea, b: eb))
                    )
                    .Where(pair => pair.a.IsFinite && pair.b.IsFinite)
                    .Concat(
                        minimumCut.Elements
                            .Select<Element,(Element,Element)>(e => (Point.Origin(), e))
                    );
            }
        }
    }

    #endregion
}