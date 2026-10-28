using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves.Streams;

/// <summary>
/// Checks finite-curve stream bounds and dominance against materialized minima, including infinite-curve fallback cases.
/// </summary>
public class Bounds
{
    public static IEnumerable<object[]> Pairs() => VerticalDeviation.CurvePairs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Pairs))]
    public void BoundsEqualTheMaterializedMinimum(Curve f, Curve g)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        var minimum = Curve.Minimum(f, g, settings);
        var fIsLower = Curve.Equivalent(f, minimum, settings);
        var gIsLower = Curve.Equivalent(g, minimum, settings);
        Assert.Equal(fIsLower, Curve.IsLowerBoundOf(f, g, settings));
        Assert.Equal(gIsLower, Curve.IsUpperBoundOf(f, g, settings));
        var dominance = Curve.Dominance(f, g, settings);
        Assert.Equal(fIsLower || gIsLower, dominance.Verified);
        if (dominance.Verified)
        {
            Assert.True(Curve.Equivalent(dominance.Lower, minimum, settings));
            Assert.True(Curve.Equivalent(dominance.Upper, fIsLower ? g : f, settings));
        }
    }
}
