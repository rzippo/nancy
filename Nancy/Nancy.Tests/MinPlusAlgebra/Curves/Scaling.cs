using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class Scaling
{
    /// <summary>
    /// Generate test cases
    /// </summary>
    public static IEnumerable<object[]> GetCurvesAndScalings()
    {
        var scalingValues = new decimal[] { 1, 2, 0.5m, 0.35m, 0.25m, 0.125m, 0.11m };
        var curves = new Curve[] {
            new ConstantCurve(4),
            new DelayServiceCurve(14),
            new RateLatencyServiceCurve(11, 10),
            new SigmaRhoArrivalCurve(4, 2),
            new ConstantCurve(0)
        };

        foreach (var curve in curves)
        foreach (var scaling in scalingValues)
            yield return new object[] { curve, scaling };            
    }

    /// <summary>
    /// The times the values of a curve are compared at: the multiples of $1 / q$ below the end of the base sequence and one period, where $q$ is the denominator of that end.
    /// They are few, and all of them are used.
    /// </summary>
    public static IEnumerable<Rational> TimesToCompare(Curve curve)
    {
        Rational maxTime = curve.BaseSequence.DefinedUntil + curve.PseudoPeriodLength;
        int denominator = (int) maxTime.Denominator;
        int numerator = (int) maxTime.Numerator;
        for (var i = 0; i < numerator; i++)
            yield return new Rational(i, denominator);
    }

    [Theory]
    [MemberData(nameof(GetCurvesAndScalings))]
    public void ScalingValues(Curve curve, decimal scaling)
    {
        var scaled = curve.Scale(scaling);

        Assert.Equal(curve.IsZero, scaled.IsZero);

        foreach (var time in TimesToCompare(curve))
            Assert.Equal(curve.ValueAt(time) * scaling, scaled.ValueAt(time));
    }

    [Theory]
    [MemberData(nameof(GetCurvesAndScalings))]
    public void ScalingOperatorValues(Curve curve, decimal scaling)
    {
        var scaled = curve * scaling;

        Assert.Equal(curve.IsZero, scaled.IsZero);

        foreach (var time in TimesToCompare(curve))
            Assert.Equal(curve.ValueAt(time) * scaling, scaled.ValueAt(time));
    }

    [Theory]
    [MemberData(nameof(GetCurvesAndScalings))]
    public void ScalingOperatorDivisionValues(Curve curve, decimal scaling)
    {
        var scaled = curve / scaling;

        Assert.Equal(curve.IsZero, scaled.IsZero);

        foreach (var time in TimesToCompare(curve))
            Assert.Equal(curve.ValueAt(time) / scaling, scaled.ValueAt(time));
    }
}