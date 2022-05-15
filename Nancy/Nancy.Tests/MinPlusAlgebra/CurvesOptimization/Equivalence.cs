﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.CurvesOptimization;

public class Equivalence
{
    private readonly ITestOutputHelper output;

    public Equivalence(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static IEnumerable<object[]> PeriodicPointClosureTestCases()
    {
        var testCases = new Curve[]
        {
            buildPeriodicPointClosureTest(time: 1, value: 2, pseudoPeriodLength: 2, pseudoPeriodHeight: 1, k: 2),
            buildPeriodicPointClosureTest(time: 54, value: 51, pseudoPeriodLength: 115, pseudoPeriodHeight: 30, k: 5),
            buildPeriodicPointClosureTest(time: 10, value: 12, pseudoPeriodLength: 10, pseudoPeriodHeight: 2, k: 34),
            buildPeriodicPointClosureTest(time: 14, value: 41, pseudoPeriodLength: 10, pseudoPeriodHeight: 2, k: 23),
            buildPeriodicPointClosureTest(time: 5, value: 2, pseudoPeriodLength: 15, pseudoPeriodHeight: 3, k: 64),
            buildPeriodicPointClosureTest(time: 54, value: 7, pseudoPeriodLength: 115, pseudoPeriodHeight: 23, k: 72),
            buildPeriodicPointClosureTest(time: 10, value: 8, pseudoPeriodLength: 5, pseudoPeriodHeight: 2, k: 14),
            buildPeriodicPointClosureTest(time: 14, value: 12, pseudoPeriodLength: 10, pseudoPeriodHeight: 2, k: 53),
            buildPeriodicPointClosureTest(time: 10, value: 3, pseudoPeriodLength: 5, pseudoPeriodHeight: 2, k: 241)
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase };

        Curve buildPeriodicPointClosureTest(
            Rational time, 
            Rational value, 
            Rational pseudoPeriodLength,
            Rational pseudoPeriodHeight,
            int k)
        {
            Point pointK = new Point(time: k * time, value: k * value);

            Curve iteratedPointK = new Curve(
                baseSequence: new Sequence(
                    new[] { Point.Origin(), pointK },  //Addition of origin handles f^0
                    fillFrom: 0,
                    fillTo: pointK.Time + pseudoPeriodLength),
                pseudoPeriodStart: pointK.Time,
                pseudoPeriodLength: pseudoPeriodLength,
                pseudoPeriodHeight: pseudoPeriodHeight
            );

            return iteratedPointK;
        }
    }

    [Theory]
    [MemberData(nameof(PeriodicPointClosureTestCases))]
    public void PeriodicPointClosure(Curve curve)
    {
        var optimizedCurve = curve.Optimize();

        Assert.True(optimizedCurve.Equivalent(curve));
    }

    public static IEnumerable<object[]> StaircaseCurvesTestCases()
    {
        var testCases = new StaircaseCurve[][]
        {
            new []
            {
                new StaircaseCurve(4, 12, 4),
                new StaircaseCurve(3, 12, 3),
                new StaircaseCurve(4, 12, 4)
            },
            new []
            {
                new StaircaseCurve(4, 12, 4),
                new StaircaseCurve(3,12, 3)
            },
            new []
            {
                new StaircaseCurve(4, 12, 4),
                new StaircaseCurve(3,11, 3)   
            },
            new []
            {
                new StaircaseCurve(5, 12, 4),
                new StaircaseCurve(3,11, 3),
                new StaircaseCurve(3, 3, 2)
            },
            new []
            {
                new StaircaseCurve(3, 3, 2),
                new StaircaseCurve(3,5, 5)
            },
            new []
            {
                new StaircaseCurve(3, 3, 2),
                new StaircaseCurve(3,5, 5),
                new StaircaseCurve(416, 835, 313)
            }
        };
            
        foreach (var testCase in testCases)
        {
            yield return new object[] {testCase};
        }
    }

    [Theory]
    [MemberData(nameof(StaircaseCurvesTestCases))]
    public void StairCaseOperations(StaircaseCurve[] curves)
    {
        var minOptimized = Curve.Minimum(curves, new ComputationSettings{AutoOptimize = true});
        var minUnoptimized = Curve.Minimum(curves, new ComputationSettings{AutoOptimize = false});
        output.WriteLine(minOptimized.ToString());
        output.WriteLine(minUnoptimized.ToString());
        Assert.True(Curve.Equivalent(minOptimized, minUnoptimized));
            
        var convOptimized = Curve.Convolution(curves, new ComputationSettings{AutoOptimize = true});
        var convUnoptimized = Curve.Convolution(curves, new ComputationSettings{AutoOptimize = false});
        Assert.True(Curve.Equivalent(convOptimized, convUnoptimized));
            
        var minConvOptimized = Curve.Convolution(minOptimized, minOptimized, new ComputationSettings{AutoOptimize = true});
        var minConvUnoptimized = Curve.Convolution(minUnoptimized, minUnoptimized, new ComputationSettings{AutoOptimize = false});
        Assert.True(Curve.Equivalent(minConvOptimized, minConvUnoptimized));
    }
        
    [Theory]
    [MemberData(nameof(StaircaseCurvesTestCases))]
    public void StairCaseOperations_General(StaircaseCurve[] curves)
    {
        var castCurves = curves.Select(c => new Curve(c)).ToArray();
            
        var minOptimized = Curve.Minimum(castCurves, new ComputationSettings{AutoOptimize = true});
        var minUnoptimized = Curve.Minimum(castCurves, new ComputationSettings{AutoOptimize = false});
        Assert.True(Curve.Equivalent(minOptimized, minUnoptimized));
            
        var convOptimized = Curve.Convolution(castCurves, new ComputationSettings{AutoOptimize = true});
        var convUnoptimized = Curve.Convolution(castCurves, new ComputationSettings{AutoOptimize = false});
        Assert.True(Curve.Equivalent(convOptimized, convUnoptimized));
            
        var minConvOptimized = Curve.Convolution(minOptimized, minOptimized, new ComputationSettings{AutoOptimize = true});
        var minConvUnoptimized = Curve.Convolution(minUnoptimized, minUnoptimized, new ComputationSettings{AutoOptimize = false});
        Assert.True(Curve.Equivalent(minConvOptimized, minConvUnoptimized));
    }
        
    [Theory]
    [MemberData(nameof(StaircaseCurvesTestCases))]
    public void StairCaseOperations_General_AsArgs(StaircaseCurve[] curves)
    {
        var unoptimized = curves.Select(c => new Curve(c)).ToArray();
        var optimized = unoptimized.Select(c => c.Optimize()).ToArray();
            
        var minOptimized = Curve.Minimum(optimized, new ComputationSettings{AutoOptimize = false});
        var minUnoptimized = Curve.Minimum(unoptimized, new ComputationSettings{AutoOptimize = false});
        Assert.True(Curve.Equivalent(minOptimized, minUnoptimized));
            
        var convOptimized = Curve.Convolution(optimized, new ComputationSettings{AutoOptimize = false});
        var convUnoptimized = Curve.Convolution(unoptimized, new ComputationSettings{AutoOptimize = false});
        Assert.True(Curve.Equivalent(convOptimized, convUnoptimized));
            
        var minConvOptimized = Curve.Convolution(minOptimized, minOptimized, new ComputationSettings{AutoOptimize = false});
        var minConvUnoptimized = Curve.Convolution(minUnoptimized, minUnoptimized, new ComputationSettings{AutoOptimize = false});
        Assert.True(Curve.Equivalent(minConvOptimized, minConvUnoptimized));
    }
        
    public static IEnumerable<object[]> StaircaseTestCases()
    {
        var testCases = new StaircaseCurve[]
        {
            new (4, 12, 4),
            new (3, 12, 3),
            new (3,11, 3),
            new (5, 12, 4),
            new (3, 3, 2),
            new (3,5, 5),
            new (416, 835, 313)
        };
            
        foreach (var testCase in testCases)
        {
            yield return new object[] {testCase};
        }
    }

        
    [Theory]
    [MemberData(nameof(StaircaseTestCases))]
    public void SelfConvolutionEquivalence(Curve curve)
    {
        var optimized = curve.Optimize();

        var optimizedConv = Curve.Convolution(optimized, optimized, new ComputationSettings{AutoOptimize = false});
        var unoptimizedConv = Curve.Convolution(curve, curve, new ComputationSettings{AutoOptimize = false});
            
        Assert.True(Curve.Equivalent(optimizedConv, unoptimizedConv));
    }
        
    [Theory]
    [MemberData(nameof(StaircaseTestCases))]
    public void SelfConvolutionEquivalence_General(Curve curve)
    {
        var copy = new Curve(curve);
        var optimized = copy.Optimize();

        var optimizedConv = Curve.Convolution(optimized, optimized, new ComputationSettings{AutoOptimize = false});
        var unoptimizedConv = Curve.Convolution(copy, copy, new ComputationSettings{AutoOptimize = false});
            
        Assert.True(Curve.Equivalent(optimizedConv, unoptimizedConv));
    }

}