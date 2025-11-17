using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class Delay
{
    /// <summary>
    /// Generate test cases
    /// </summary>
    public static IEnumerable<object[]> GetCurvesAndDelays()
    {
        var scalingValues = new decimal[] { 0, 1, 20, 10.5m, 30.35m };
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
    [MemberData(nameof(GetCurvesAndDelays))]
    public void DelayValues(Curve curve, decimal delay)
    {
        var delayed = curve.DelayBy(delay);

        Assert.Equal(curve.IsZero, delayed.IsZero);
        Assert.True( delay == 0 || delayed.Cut(0, delay).IsZero);

        var random = new Random();
        for (int i = 0; i < 10; i++)
        {
            var time = RandomTime();
            Assert.Equal(curve.ValueAt(time), delayed.ValueAt(time + delay));
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

    public static List<(DelayServiceCurve a, DelayServiceCurve b)> DelayCompositions =
    [
        (
            a: new DelayServiceCurve(0),
            b: new DelayServiceCurve(0)
        ),
        (
            a: new DelayServiceCurve(0),
            b: new DelayServiceCurve(10)
        ),
        (
            a: new DelayServiceCurve(10),
            b: new DelayServiceCurve(0)
        ),
        (
            a: new DelayServiceCurve(5),
            b: new DelayServiceCurve(7)
        ),
        (
            a: new DelayServiceCurve(new Rational(296, 625)),
            b: new DelayServiceCurve(new Rational(16, 125))
        )
    ];

    public static IEnumerable<object[]> DelayCompositionTestCases
        => DelayCompositions.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DelayCompositionTestCases))]
    public void DelayCompositionTest(DelayServiceCurve a, DelayServiceCurve b)
    {
        var sum = a.Delay + b.Delay;
        var conv = Curve.Convolution(a, b);
        Assert.True(conv is DelayServiceCurve ds && ds.Delay == sum);
    }

    [Theory]
    [MemberData(nameof(DelayCompositionTestCases))]
    public void DelayCompositionAsGenericTest(DelayServiceCurve a, DelayServiceCurve b)
    {
        var sum = a.Delay + b.Delay;
        var aGeneric = new Curve(a);
        var bGeneric = new Curve(b);
        var conv = Curve.Convolution(aGeneric, bGeneric);
        
        Assert.True(conv.IsUltimatelyPlusInfinite);
        var tl = conv.PseudoPeriodStartInfimum;
        Assert.Equal(sum, tl);
        var cut = conv.Cut(0, tl, isStartIncluded: true, isEndIncluded: true);
        var expectedSeq = Sequence.Zero(0, tl, isStartIncluded: true, isEndIncluded: true);
        Assert.True(Sequence.Equivalent(expectedSeq, cut));
    }
}