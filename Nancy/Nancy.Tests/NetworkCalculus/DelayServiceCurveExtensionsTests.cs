using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus;

public class DelayServiceCurveExtensionsTests
{
    public static IEnumerable<object[]> GetCollectionTestCases()
    {
        var curves = new DelayServiceCurve[]
        {
            new(3),
            new(5),
            new(7),
        };
        yield return new object[] { curves };
    }

    [Theory]
    [MemberData(nameof(GetCollectionTestCases))]
    public void Convolution_Enumerable_ComputesConvolution(DelayServiceCurve[] curves)
    {
        var result = curves.AsEnumerable().Convolution();
        var expected = DelayServiceCurve.Convolution(curves);
        Assert.Equal(expected, result);
    }
}
