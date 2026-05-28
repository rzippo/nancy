using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus;

public class RateLatencyServiceCurveExtensionsTests
{
    public static IEnumerable<object[]> GetCollectionTestCases()
    {
        var curves = new RateLatencyServiceCurve[]
        {
            new(rate: 4, latency: 3),
            new(rate: 5, latency: 2),
            new(rate: 3, latency: 5),
        };
        yield return new object[] { curves };
    }

    [Theory]
    [MemberData(nameof(GetCollectionTestCases))]
    public void Convolution_Collection_ComputesConvolution(RateLatencyServiceCurve[] curves)
    {
        var result = ((IReadOnlyCollection<RateLatencyServiceCurve>)curves).Convolution();
        var expected = RateLatencyServiceCurve.Convolution(curves);
        Assert.Equal(expected, result);
    }
}
