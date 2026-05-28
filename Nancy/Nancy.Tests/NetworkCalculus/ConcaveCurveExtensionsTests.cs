using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus;

public class ConcaveCurveExtensionsTests
{
    public static IEnumerable<object[]> GetCollectionTestCases()
    {
        var curves = new ConcaveCurve[]
        {
            new(new SigmaRhoArrivalCurve(sigma: 5, rho: 2)),
            new(new SigmaRhoArrivalCurve(sigma: 10, rho: 1)),
            new(new SigmaRhoArrivalCurve(sigma: 3, rho: 4)),
        };
        yield return new object[] { curves };
    }

    [Theory]
    [MemberData(nameof(GetCollectionTestCases))]
    public void Minimum_Enumerable_ComputesMinimum(ConcaveCurve[] curves)
    {
        var result = curves.AsEnumerable().Minimum();
        var expected = Curve.Minimum(curves);
        Assert.True(Curve.Equivalent(expected, result));
    }

    [Theory]
    [MemberData(nameof(GetCollectionTestCases))]
    public void Minimum_Collection_ComputesMinimum(ConcaveCurve[] curves)
    {
        var result = ((IReadOnlyCollection<ConcaveCurve>)curves).Minimum();
        var expected = Curve.Minimum(curves);
        Assert.True(Curve.Equivalent(expected, result));
    }

    [Theory]
    [MemberData(nameof(GetCollectionTestCases))]
    public void Convolution_Enumerable_EqualsMinimum(ConcaveCurve[] curves)
    {
        var result = curves.AsEnumerable().Convolution();
        var minResult = curves.AsEnumerable().Minimum();
        Assert.True(Curve.Equivalent(result, minResult));
    }

    [Theory]
    [MemberData(nameof(GetCollectionTestCases))]
    public void Convolution_Collection_EqualsMinimum(ConcaveCurve[] curves)
    {
        var result = ((IReadOnlyCollection<ConcaveCurve>)curves).Convolution();
        var minResult = ((IReadOnlyCollection<ConcaveCurve>)curves).Minimum();
        Assert.True(Curve.Equivalent(result, minResult));
    }
}
