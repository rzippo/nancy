using System;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.Compute;

/// <summary>
/// Tests for scaling operators (* and /) with Rational in Nancy.Expressions
/// </summary>
public class CurveScalingOperators
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CurveScalingOperators(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ScalingMultiplicationByRational()
    {
        var curve = new RateLatencyServiceCurve(rate: 2, latency: 3);
        var scaleFactor = new Rational(3);

        // Compute through Nancy
        var nancyResult = curve * scaleFactor;
        _testOutputHelper.WriteLine($"Nancy result: {nancyResult.ToCodeString()}");

        // Compute through Nancy.Expressions with operator
        var curveExp = Expressions.FromCurve(curve);
        var expressionResult = (curveExp * scaleFactor).Compute();
        _testOutputHelper.WriteLine($"Nancy.Expressions result: {expressionResult.ToCodeString()}");

        // Verify equivalence
        Assert.True(Curve.Equivalent(nancyResult, expressionResult));
    }

    [Fact]
    public void ScalingRationalMultiplicationByCurve()
    {
        var curve = new RateLatencyServiceCurve(rate: 2, latency: 3);
        var scaleFactor = new Rational(2);

        // Compute through Nancy
        var nancyResult = scaleFactor * curve;
        _testOutputHelper.WriteLine($"Nancy result: {nancyResult.ToCodeString()}");

        // Compute through Nancy.Expressions with operator
        var curveExp = Expressions.FromCurve(curve);
        var expressionResult = (scaleFactor * curveExp).Compute();
        _testOutputHelper.WriteLine($"Nancy.Expressions result: {expressionResult.ToCodeString()}");

        // Verify equivalence
        Assert.True(Curve.Equivalent(nancyResult, expressionResult));
    }

    [Fact]
    public void ScalingDivisionByRational()
    {
        var curve = new RateLatencyServiceCurve(rate: 4, latency: 6);
        var scaleFactor = new Rational(2);

        // Compute through Nancy
        var nancyResult = curve / scaleFactor;
        _testOutputHelper.WriteLine($"Nancy result: {nancyResult.ToCodeString()}");

        // Compute through Nancy.Expressions with operator
        var curveExp = Expressions.FromCurve(curve);
        var expressionResult = (curveExp / scaleFactor).Compute();
        _testOutputHelper.WriteLine($"Nancy.Expressions result: {expressionResult.ToCodeString()}");

        // Verify equivalence
        Assert.True(Curve.Equivalent(nancyResult, expressionResult));
    }

    [Fact]
    public void ScalingWithFractionalScaleFactor()
    {
        var curve = new SigmaRhoArrivalCurve(sigma: 10, rho: 4);
        var scaleFactor = new Rational(1, 2);

        // Compute through Nancy
        var nancyResult = curve * scaleFactor;
        _testOutputHelper.WriteLine($"Nancy result: {nancyResult.ToCodeString()}");

        // Compute through Nancy.Expressions with operator
        var curveExp = Expressions.FromCurve(curve);
        var expressionResult = (curveExp * scaleFactor).Compute();
        _testOutputHelper.WriteLine($"Nancy.Expressions result: {expressionResult.ToCodeString()}");

        // Verify equivalence
        Assert.True(Curve.Equivalent(nancyResult, expressionResult));
    }

    [Fact]
    public void CombinedScalingWithOtherOperations()
    {
        var curve = new RateLatencyServiceCurve(rate: 2, latency: 3);
        var scaleFactor = new Rational(2);

        // Compute through Nancy
        var nancyResult = (curve.ToNonNegative()) * scaleFactor;
        _testOutputHelper.WriteLine($"Nancy result: {nancyResult.ToCodeString()}");

        // Compute through Nancy.Expressions with operator
        var curveExp = Expressions.FromCurve(curve);
        var expressionResult = (curveExp.ToNonNegative() * scaleFactor).Compute();
        _testOutputHelper.WriteLine($"Nancy.Expressions result: {expressionResult.ToCodeString()}");

        // Verify equivalence
        Assert.True(Curve.Equivalent(nancyResult, expressionResult));
    }
}
