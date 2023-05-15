using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class ToRightContinuous
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
    public void ResultIsRightContinuous(Curve curve)
    {
        var result = curve.ToRightContinuous();
        Assert.True(result.IsRightContinuous);
        
        if(curve.IsRightContinuous)
            Assert.True(Curve.Equivalent(curve, result));
    }
}