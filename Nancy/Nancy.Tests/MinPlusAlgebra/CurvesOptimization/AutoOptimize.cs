using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.CurvesOptimization;

public class AutoOptimize
{
    public static IEnumerable<object[]> StaircasePairTestCases()
    {
        var testCases = new (FlowControlCurve a, FlowControlCurve b)[]
        {
            (
                new FlowControlCurve(height: 363, latency: 149, rate: 2), 
                new FlowControlCurve(height: 682, latency: 341, rate: 924)
            ),
            (
                new FlowControlCurve(3, 3, 2),
                new FlowControlCurve(3,5, 5)
            ),
            (
                new FlowControlCurve(416, 835, 313),
                new FlowControlCurve(552,571, 970)
            ),
            (
                new FlowControlCurve(3, 3, 2),
                new FlowControlCurve(3,0, 5)
            ),
            (
                new FlowControlCurve(4, 12, 4),
                new FlowControlCurve(3,12, 3)
            ),
            (
                new FlowControlCurve(4, 12, 4),
                new FlowControlCurve(3,11, 3)
            ),
            (
                new FlowControlCurve(5, 12, 4),
                new FlowControlCurve(3,11, 3)
            ),
            #if !SKIP_LONG_TESTS
            (
                new FlowControlCurve(new Rational(11, 13), 4000, new Rational(11, 13)),
                new FlowControlCurve(new Rational(5, 7), 5000, new Rational(5, 7))
            ),
            (
                new FlowControlCurve(new Rational(2*5*11), 4000, new Rational(2*5*11)),
                new FlowControlCurve(new Rational(3*7*13), 5000, new Rational(3*7*13))
            ),
            #endif
            // (
            //     new FlowControlCurve(new Rational(11, 13), 4000, new Rational(11, 13)),
            //     new FlowControlCurve(new Rational(17, 19), 5000, new Rational(17, 19))
            // )
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] {testCase.a, testCase.b};
        }
    }

    [Theory]
    [MemberData(nameof(StaircasePairTestCases))]
    public void Minimum(Curve a, Curve b)
    {
        var settings = new ComputationSettings { UseRepresentationMinimization = true };
        var minimum = Curve.Minimum(a, b, settings);
        var optimized = minimum.Optimize();

        Assert.Equal(optimized.BaseSequence.Count, minimum.BaseSequence.Count);
    }

    [Theory]
    [MemberData(nameof(StaircasePairTestCases))]
    public void Convolution(Curve a, Curve b)
    {
        var settings = new ComputationSettings { UseRepresentationMinimization = true };
        var convolution = Curve.Convolution(a, b, settings);
        var optimized = convolution.Optimize();

        Assert.Equal(optimized.BaseSequence.Count, convolution.BaseSequence.Count);
    }

    [Theory]
    [MemberData(nameof(StaircasePairTestCases))]
    public void GenericConvolution(Curve a, Curve b)
    {
        var settings = new ComputationSettings { UseRepresentationMinimization = true, UseSubAdditiveConvolutionOptimizations = false };
        var convolution = Curve.Convolution(a, b, settings);
        var optimized = convolution.Optimize();

        Assert.Equal(optimized.BaseSequence.Count, convolution.BaseSequence.Count);
    }
}