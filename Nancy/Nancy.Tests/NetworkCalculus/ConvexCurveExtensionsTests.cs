using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus;

public class ConvexCurveExtensionsTests
{
    public static IEnumerable<object[]> GetCollectionTestCases()
    {
        var curves = new ConvexCurve[]
        {
            new(new RateLatencyServiceCurve(rate: 4, latency: 3)),
            new(new RateLatencyServiceCurve(rate: 5, latency: 2)),
            new(new RateLatencyServiceCurve(rate: 3, latency: 5)),
        };
        yield return new object[] { curves };
    }

    [Theory]
    [MemberData(nameof(GetCollectionTestCases))]
    public void Maximum_Enumerable_ComputesMaximum(ConvexCurve[] curves)
    {
        var result = curves.AsEnumerable().Maximum();
        var expected = Curve.Maximum(curves);
        Assert.True(Curve.Equivalent(expected, result));
    }

    [Theory]
    [MemberData(nameof(GetCollectionTestCases))]
    public void Maximum_Collection_ComputesMaximum(ConvexCurve[] curves)
    {
        var result = ((IReadOnlyCollection<ConvexCurve>)curves).Maximum();
        var expected = Curve.Maximum(curves);
        Assert.True(Curve.Equivalent(expected, result));
    }

    [Theory]
    [MemberData(nameof(GetCollectionTestCases))]
    public void Convolution_Enumerable_ComputesConvolution(ConvexCurve[] curves)
    {
        var result = curves.AsEnumerable().Convolution();
        var expected = curves.Aggregate((a, b) => a.Convolution(b));
        Assert.True(Curve.Equivalent(expected, result));
    }

    [Theory]
    [MemberData(nameof(GetCollectionTestCases))]
    public void Convolution_Collection_ComputesConvolution(ConvexCurve[] curves)
    {
        var result = ((IReadOnlyCollection<ConvexCurve>)curves).Convolution();
        var expected = Curve.Convolution(curves);
        Assert.True(Curve.Equivalent(expected, result));
    }
}
