using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveScaling
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

    [Theory]
    [MemberData(nameof(GetCurvesAndScalings))]
    public void ScalingValues(Curve curve, decimal scaling)
    {
        var scaled = curve.Scale(scaling);

        Assert.Equal(curve.IsZero, scaled.IsZero);

        var random = new Random();
        for (int i = 0; i < 10; i++)
        {
            var time = RandomTime();
            Assert.Equal(curve.ValueAt(time) * scaling, scaled.ValueAt(time));
        }

        Rational RandomTime()
        {
            Rational maxTime = curve.BaseSequence.DefinedUntil + curve.PseudoPeriodLength;
            int denominator = (int) maxTime.Denominator;
            int numerator = (int) maxTime.Numerator;

            int randomInt = random.Next(0, numerator);
            return new Rational(randomInt, denominator);
        }
    }
}