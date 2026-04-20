using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;
public class IsUltimatelyAffine
{
    private readonly ITestOutputHelper _testOutputHelper;

    public IsUltimatelyAffine(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Curve curves, bool isUA)> KnownCurves =
    [
        (Curve.Zero(), true),
        (new ConstantCurve(5), true),
        (new RateLatencyServiceCurve(2, 4), true),
        (new StairCurve(1, 2), false),
        (Curve.FromJson("{\"type\":\"sigmaRhoArrivalCurve\",\"sigma\":{\"num\":1,\"den\":1},\"rho\":{\"num\":1,\"den\":1}}"), true),
        (Curve.FromJson("{\"type\":\"rateLatencyServiceCurve\",\"rate\":{\"num\":1,\"den\":1},\"latency\":{\"num\":1,\"den\":1}}"), true),
         #if BIG_RATIONAL
        (Curve.FromJson("{\"type\":\"sigmaRhoArrivalCurve\",\"sigma\":{\"num\":1,\"den\":1},\"rho\":{\"num\":2441407,\"den\":1000000000}}"), true),
        (Curve.FromJson("{\"type\":\"rateLatencyServiceCurve\",\"rate\":{\"num\":149850048000,\"den\":12309415288891},\"latency\":{\"num\":27439,\"den\":40}}"), true),
        #endif
        #if BIG_RATIONAL
        (
            Curve.FromJson("{\"type\":\"sigmaRhoArrivalCurve\",\"sigma\":{\"num\":1,\"den\":1},\"rho\":{\"num\":2441407,\"den\":1000000000}}"),
            true
        ),
        (
            Curve.FromJson("{\"type\":\"rateLatencyServiceCurve\",\"rate\":{\"num\":149850048000,\"den\":12309415288891},\"latency\":{\"num\":27439,\"den\":40}}"),
            true
        ),
        (
            new Curve(Curve.FromJson("{\"type\":\"sigmaRhoArrivalCurve\",\"sigma\":{\"num\":1,\"den\":1},\"rho\":{\"num\":2441407,\"den\":1000000000}}")),
            true
        ),
        (
            new Curve(Curve.FromJson("{\"type\":\"rateLatencyServiceCurve\",\"rate\":{\"num\":149850048000,\"den\":12309415288891},\"latency\":{\"num\":27439,\"den\":40}}")),
            true
        )
        #endif
    ];

    public static IEnumerable<object[]> GetKnownCurveTestCases
        => KnownCurves.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetKnownCurveTestCases))]
    public void TestIsUA(Curve curve, bool expected)
    {
        var isUA = curve.IsUltimatelyAffine;
        Assert.Equal(expected, isUA);
    }
}