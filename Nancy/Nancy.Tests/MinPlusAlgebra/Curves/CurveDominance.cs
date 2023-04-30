using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveDominance
{
    public static IEnumerable<object[]> GetDominanceTestCases()
    {
        IEnumerable<(Curve a, Curve b, (bool verified, Curve lower, Curve upper) expected)> testCases()
        {
            var a = new FlowControlCurve(1, 1, 2);
            var b = new FlowControlCurve(1, 1, 4);
            yield return (a, b, (true, a, b));
            yield return (b, a, (true, a, b));

            var c = new FlowControlCurve(2, 1, 8);
            yield return (b, c, (true, b, c));
            yield return (c, b, (true, b, c));
            yield return (a, c, (true, a, c));
            yield return (c, a, (true, a, c));
        };

        foreach (var testCase in testCases())
        {
            yield return new object[] {testCase.a, testCase.b, testCase.expected};
        }
    }

    public static IEnumerable<object[]> GetAsymptoticDominanceTestCases()
    {
        IEnumerable<(Curve a, Curve b, (bool verified, Curve lower, Curve upper) expected)> testCases()
        {
            var a = new FlowControlCurve(40, 40, 40);
            var b = new FlowControlCurve(4, 4, 8);
            yield return (a, b, (true, a, b));
            yield return (b, a, (true, a, b));

            var sc = new RateLatencyServiceCurve(8, 4);
            var ac = new SigmaRhoArrivalCurve(4, 4);
            yield return (sc, ac, (true, ac, sc));
            yield return (ac, sc, (true, ac, sc));

            var counterExampleA = new Curve(
                baseSequence: new Sequence(
                    new Element[]{
                        Point.Origin(), 
                        new Segment(0, 3, 1, 3), 
                        new Point(3, 10), 
                        new Segment(3, 6, 10, 2)
                    }),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 3,
                pseudoPeriodHeight: 6
            );

            var counterExampleB = new Curve(
                baseSequence: new Sequence(
                    new Element[]{
                        Point.Origin(), 
                        new Segment(0, 2, 0, 4), 
                        new Point(2, 8), 
                        new Segment(2, 5, 8, new Rational(2, 3)),
                        new Point(5, 10),
                        new Segment(5, 10, 10, new Rational(10, 5))
                    }),
                pseudoPeriodStart: 5,
                pseudoPeriodLength: 5,
                pseudoPeriodHeight: 10
            );
            yield return (counterExampleA, counterExampleB, (true, counterExampleB, counterExampleA));
            yield return (counterExampleB, counterExampleA, (true, counterExampleB, counterExampleA));
        };

        foreach (var testCase in testCases())
        {
            yield return new object[] {testCase.a, testCase.b, testCase.expected};
        }
    }

    public static IEnumerable<object[]> GetNoDominanceTestCases()
    {
        IEnumerable<(Curve a, Curve b, (bool verified, Curve lower, Curve upper) expected)> testCases()
        {
            var a = new FlowControlCurve(2, 40, 2);
            var b = new FlowControlCurve(8, 4, 8);
            yield return (a, b, (false, a, b));
            yield return (b, a, (false, a, b));
        };

        foreach (var testCase in testCases())
        {
            yield return new object[] {testCase.a, testCase.b, testCase.expected};
        }
    }

    [Theory]
    [MemberData(nameof(GetDominanceTestCases))]
    [MemberData(nameof(GetNoDominanceTestCases))]
    public void DominanceTests(Curve a, Curve b, (bool verified, Curve lower, Curve upper) expected)
    {
        var result = Curve.Dominance(a, b);
        Assert.Equal(expected.verified, result.Verified);
        if (expected.verified)
        {
            Assert.Equal(expected.lower, result.Lower);
            Assert.Equal(expected.upper, result.Upper);
        }
    }

    // the unused parameter is needed to reuse the testcases
    [Theory]
    [MemberData(nameof(GetAsymptoticDominanceTestCases))]
#pragma warning disable xUnit1026
    public void DominanceTests2(Curve a, Curve b, (bool verified, Curve lower, Curve upper) expected)
#pragma warning restore xUnit1026
    {
        var result = Curve.Dominance(a, b);
        Assert.False(result.Verified);
    }

    [Theory]
    [MemberData(nameof(GetDominanceTestCases))]
    [MemberData(nameof(GetAsymptoticDominanceTestCases))]
    [MemberData(nameof(GetNoDominanceTestCases))]
    public void AsymptoticDominanceTests(Curve a, Curve b, (bool verified, Curve lower, Curve upper) expected)
    {
        var result = Curve.AsymptoticDominance(a, b);
        Assert.Equal(expected.verified, result.Verified);
        if (expected.verified)
        {
            Assert.Equal(expected.lower, result.Lower);
            Assert.Equal(expected.upper, result.Upper);
        }
    }
}