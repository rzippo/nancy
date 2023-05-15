using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class ToLeftContinuous
{
    public static IEnumerable<object[]> Testcases()
    {
        var cases = UpperPseudoInverse.LeftContinuousTestCases().Concat(
            UpperPseudoInverse.RightContinuousTestCases()).Concat(
            UpperPseudoInverse.ContinuousTestCases());

        foreach (var c in cases)
        {
            var f = (Curve) c[0];
            yield return new object[]{ f };
        }
    }
    
    [Theory]
    [MemberData(nameof(Testcases))]
    public void ResultIsLeftContinuous(Curve curve)
    {
        var result = curve.ToLeftContinuous();
        Assert.True(result.IsLeftContinuous);
        
        if(curve.IsLeftContinuous)
            Assert.True(Curve.Equivalent(curve, result));
    }
}