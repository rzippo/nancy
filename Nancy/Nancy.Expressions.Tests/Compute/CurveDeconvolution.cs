using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CurveDeconvolution
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CurveDeconvolution(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Curve first, Curve second)> TestCases = 
    [
        // Simple sigma-rho arrival / rate-latency service
        (
            new SigmaRhoArrivalCurve(sigma: 10, rho: 5),
            new RateLatencyServiceCurve(rate: 2, latency: 3)
        ),
        // Different sigma-rho values
        (
            new SigmaRhoArrivalCurve(sigma: 20, rho: 10),
            new RateLatencyServiceCurve(rate: 5, latency: 2)
        ),
        // Arrival with higher flow than service rate
        (
            new SigmaRhoArrivalCurve(sigma: 15, rho: 8),
            new RateLatencyServiceCurve(rate: 4, latency: 1)
        ),
        // Zero latency service
        (
            new SigmaRhoArrivalCurve(sigma: 5, rho: 3),
            new RateLatencyServiceCurve(rate: 10, latency: 0)
        ),
        // Small values
        (
            new SigmaRhoArrivalCurve(sigma: 1, rho: 1),
            new RateLatencyServiceCurve(rate: 1, latency: 1)
        ),
    ];

    public static IEnumerable<object[]> DeconvolutionTestCases
        => TestCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DeconvolutionTestCases))]
    public void DeconvolutionEquivalence(Curve first, Curve second)
    {
        _testOutputHelper.WriteLine($"first: {first.ToCodeString()}");
        _testOutputHelper.WriteLine($"second: {second.ToCodeString()}");

        // Compute through Nancy
        var nancyResult = Curve.Deconvolution(first, second);
        _testOutputHelper.WriteLine($"Nancy result: {nancyResult.ToCodeString()}");

        // Compute through Nancy.Expressions
        var firstExp = Expressions.FromCurve(first);
        var secondExp = Expressions.FromCurve(second);
        var deconvExp = Expressions.Deconvolution(firstExp, secondExp);
        var expressionResult = deconvExp.Compute();
        _testOutputHelper.WriteLine($"Nancy.Expressions result: {expressionResult.ToCodeString()}");

        // Verify equivalence
        Assert.True(Curve.Equivalent(nancyResult, expressionResult),
            $"Results differ: Nancy={nancyResult.ToCodeString()}, Expressions={expressionResult.ToCodeString()}");
    }
}
