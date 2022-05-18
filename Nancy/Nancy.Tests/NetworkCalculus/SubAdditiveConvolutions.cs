using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.NetworkCalculus;

public class SubAdditiveConvolutions
{
    private readonly ITestOutputHelper output;

    public SubAdditiveConvolutions(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static IEnumerable<object[]> StaircasePairTestCases()
    {
        var testCases = new (StaircaseCurve a, StaircaseCurve b)[]
        {
            (
                new StaircaseCurve(height: 363, latency: 149, rate: 2), 
                new StaircaseCurve(height: 682, latency: 341, rate: 924)
            ),
            (
                new StaircaseCurve(3, 3, 2),
                new StaircaseCurve(3,5, 5)
            ),
            (
                new StaircaseCurve(416, 835, 313),
                new StaircaseCurve(552,571, 970)
            ),
            (
                new StaircaseCurve(3, 3, 2),
                new StaircaseCurve(3,0, 5)
            ),
            (
                new StaircaseCurve(4, 12, 4),
                new StaircaseCurve(3,12, 3)
            ),
            (
                new StaircaseCurve(4, 12, 4),
                new StaircaseCurve(3,11, 3)
            ),
            (
                new StaircaseCurve(5, 12, 4),
                new StaircaseCurve(3,11, 3)
            ),
            #if !SKIP_LONG_TESTS
            (
                new StaircaseCurve(new Rational(11, 13), 4000, new Rational(11, 13)),
                new StaircaseCurve(new Rational(5, 7), 5000, new Rational(5, 7))
            ),
            (
                new StaircaseCurve(new Rational(2*5*11), 4000, new Rational(2*5*11)),
                new StaircaseCurve(new Rational(3*7*13), 5000, new Rational(3*7*13))
            ),
            #endif
            // (
            //     new StaircaseCurve(new Rational(11, 13), 4000, new Rational(11, 13)),
            //     new StaircaseCurve(new Rational(17, 19), 5000, new Rational(17, 19))
            // )
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] {testCase.a, testCase.b};
        }
    }

    public static IEnumerable<object[]> NonStaircasePairTestCases()
    {
        var testCases = new (SubAdditiveCurve a, SubAdditiveCurve b)[]
        {
            (
                new SubAdditiveCurve(
                    baseSequence: new Sequence(
                        new Element[]
                        {
                            Point.Origin(),
                            new Segment(0, 3, 1, 3),
                            new Point(3, 10),
                            new Segment(3, 6, 10, 2)
                        }),
                    pseudoPeriodStart: 3,
                    pseudoPeriodLength: 3,
                    pseudoPeriodHeight: 6
                ),
                new SubAdditiveCurve(
                    baseSequence: new Sequence(
                        new Element[]
                        {
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
                )
            )
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] {testCase.a, testCase.b};
        }
    }
        
    [Theory]
    [MemberData(nameof(StaircasePairTestCases))]
    [MemberData(nameof(NonStaircasePairTestCases))]
    public void ConvolutionEquivalence_Pair(SubAdditiveCurve a, SubAdditiveCurve b)
    {
        var settings = new ComputationSettings
        {
            ConvolutionPartitioningThreshold = 500
        };
            
        var optimizedConvolution = a.Convolution(b, settings);

        var castA = new Curve(a);
        var castB = new Curve(b);
        var unoptimizedConvolution = castA.Convolution(castB, settings);
            
        Assert.True(Curve.Equivalent(optimizedConvolution, unoptimizedConvolution));
    }
        
    public static IEnumerable<object[]> StaircaseChainedTestCases()
    {
        var testCases = new StaircaseCurve[][]
        {
            new []
            {
                new StaircaseCurve(3, 3, 2),
                new StaircaseCurve(3,5, 5),
                new StaircaseCurve(416, 835, 313)
            },
            new []
            {
                new StaircaseCurve(416, 835, 313),
                new StaircaseCurve(552,571, 970),
                new StaircaseCurve(3, 3, 2)
            },
            new []
            {
                new StaircaseCurve(3, 3, 2),
                new StaircaseCurve(3,0, 5),
                new StaircaseCurve(4, 12, 4)
            },
            new []
            {
                new StaircaseCurve(4, 12, 4),
                new StaircaseCurve(3,12, 3),
                new StaircaseCurve(4, 12, 4)
            },
            new []
            {
                new StaircaseCurve(4, 12, 4),
                new StaircaseCurve(3,11, 3),
                new StaircaseCurve(5, 12, 4)
            },
            new []
            {
                new StaircaseCurve(5, 12, 4),
                new StaircaseCurve(3,11, 3),
                new StaircaseCurve(3, 3, 2)
            }
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] {testCase};
        }
    }
        
    [Theory]
    [MemberData(nameof(StaircaseChainedTestCases))]
    public void ConvolutionEquivalence_Chained(StaircaseCurve[] curves)
    {
        var optimizedConvolution = StaircaseCurve.Convolution(curves);

        var castCurves = curves.Select(sc => new Curve(sc));
        var unoptimizedConvolution = Curve.Convolution(castCurves);
            
        Assert.True(Curve.Equivalent(optimizedConvolution, unoptimizedConvolution));
    }
        
    public static IEnumerable<object[]> StaircaseSelfTestCases()
    {
        var testCases = new StaircaseCurve[]
        {
            new StaircaseCurve(3, 3, 2),
            new StaircaseCurve(3,5, 5),
            new StaircaseCurve(416, 835, 313),                
            new StaircaseCurve(552,571, 970),
            new StaircaseCurve(3, 3, 2),
            new StaircaseCurve(3,0, 5),
            new StaircaseCurve(4, 12, 4),            
            new StaircaseCurve(3,12, 3),
            new StaircaseCurve(4, 12, 4),            
            new StaircaseCurve(3,11, 3),
            new StaircaseCurve(5, 12, 4),            
            new StaircaseCurve(3,11, 3)
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] {testCase};
        }
    }
        
    [Theory]
    [MemberData(nameof(StaircaseSelfTestCases))]
    public void ConvolutionEquivalence_Self(StaircaseCurve curve)
    {
        var optimizedConvolution = Curve.Convolution(curve, curve);

        var castCurve = new Curve(curve);
        var unoptimizedConvolution = Curve.Convolution(castCurve, castCurve);
            
        Assert.True(Curve.Equivalent(optimizedConvolution, unoptimizedConvolution));
        Assert.True(Curve.Equivalent(optimizedConvolution, curve));
    }
}