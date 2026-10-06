using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Checks the (·,+) convolutions of the periodic parts of curves, which are $-\infty$ or $+\infty$ before their start, under the settings that use the isomorphism and under those that do not,
/// and the isomorphism itself, [ZNS26] Theorems 31 and 32.
/// </summary>
public class PeriodicPartIsomorphism
{
    private static readonly ComputationSettings WithoutIsomorphism = ComputationSettings.Default() with
    {
        UseConvolutionIsospeedOptimization = false,
        UseConvolutionSuperIsospeedOptimization = false,
        UseBySequenceConvolutionIsospeedOptimization = false
    };

    /// <summary>
    /// Pairs of right-continuous, non-decreasing curves with finite values, that are not ultimately constant.
    /// </summary>
    public static List<(Curve f, Curve g)> Pairs =
    [
        (
            new Curve(new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 0), new Point(1, 1), new Segment(1, 2, 1, 1) ]), 1, 1, 1),
            new Curve(new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 1) ]), 0, 1, 1)
        ),
        (
            new Curve(new Sequence([ new Point(0, 0), new Segment(0, 2, 0, 1), new Point(2, 3), new Segment(2, 3, 3, 2) ]), 2, 1, 2),
            new Curve(new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 2) ]), 0, 1, 2)
        ),
    ];

    public static IEnumerable<object[]> PairTestCases()
        => Pairs.ToXUnitTestCases();

    /// <summary>
    /// The (max,+) convolution of the periodic parts $f^\vee_p$ and $g^\vee_p$, which are $-\infty$ before $T_f$ and $T_g$,
    /// is the same with and without the isomorphism.
    /// </summary>
    [Theory]
    [MemberData(nameof(PairTestCases))]
    public void MaxPlusConvolutionOfPeriodicPartsIsTheSameWithTheIsomorphism(Curve f, Curve g)
    {
        var f_p = f.Decompose(f.PseudoPeriodStart, minDecomposition: false).right;
        var g_p = g.Decompose(g.PseudoPeriodStart, minDecomposition: false).right;

        var expected = Curve.MaxPlusConvolution(f_p, g_p, WithoutIsomorphism);
        var result = Curve.MaxPlusConvolution(f_p, g_p);
        Assert.True(Curve.Equivalent(expected, result));
    }
}
