using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus;

public class SubAdditiveConvolutions
{
    private readonly ITestOutputHelper output;

    public SubAdditiveConvolutions(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static List<(FlowControlCurve a, FlowControlCurve b)> StaircasePairs = new()
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
    
    public static List<(SubAdditiveCurve a, SubAdditiveCurve b)> NonStaircasePairs = new()
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
        ),
        (
            new SubAdditiveCurve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0) }),pseudoPeriodStart: 0,pseudoPeriodLength: 1,pseudoPeriodHeight: 0),
            new SubAdditiveCurve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,5,new Rational(1, 0),0) }),pseudoPeriodStart: 0,pseudoPeriodLength: 5,pseudoPeriodHeight: -12)
        )
    };
    
    public static IEnumerable<object[]> ConvolutionEquivalence_Pair_TestCases()
    {
        foreach (var testCase in StaircasePairs)
        {
            yield return new object[] {testCase.a, testCase.b};
        }
        
        foreach (var testCase in NonStaircasePairs)
        {
            yield return new object[] {testCase.a, testCase.b};
        }
    }
    
    [Theory]
    [MemberData(nameof(ConvolutionEquivalence_Pair_TestCases))]
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
        var testCases = new FlowControlCurve[][]
        {
            new []
            {
                new FlowControlCurve(3, 3, 2),
                new FlowControlCurve(3,5, 5),
                new FlowControlCurve(416, 835, 313)
            },
            new []
            {
                new FlowControlCurve(416, 835, 313),
                new FlowControlCurve(552,571, 970),
                new FlowControlCurve(3, 3, 2)
            },
            new []
            {
                new FlowControlCurve(3, 3, 2),
                new FlowControlCurve(3,0, 5),
                new FlowControlCurve(4, 12, 4)
            },
            new []
            {
                new FlowControlCurve(4, 12, 4),
                new FlowControlCurve(3,12, 3),
                new FlowControlCurve(4, 12, 4)
            },
            new []
            {
                new FlowControlCurve(4, 12, 4),
                new FlowControlCurve(3,11, 3),
                new FlowControlCurve(5, 12, 4)
            },
            new []
            {
                new FlowControlCurve(5, 12, 4),
                new FlowControlCurve(3,11, 3),
                new FlowControlCurve(3, 3, 2)
            }
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] {testCase};
        }
    }

    [Theory]
    [MemberData(nameof(StaircaseChainedTestCases))]
    public void ConvolutionEquivalence_Chained(FlowControlCurve[] curves)
    {
        var optimizedConvolution = FlowControlCurve.Convolution(curves);

        var castCurves = curves.Select(sc => new Curve(sc));
        var unoptimizedConvolution = Curve.Convolution(castCurves);

        Assert.True(Curve.Equivalent(optimizedConvolution, unoptimizedConvolution));
    }

    public static IEnumerable<object[]> StaircaseSelfTestCases()
    {
        var testCases = new FlowControlCurve[]
        {
            new FlowControlCurve(3, 3, 2),
            new FlowControlCurve(3,5, 5),
            new FlowControlCurve(416, 835, 313),                
            new FlowControlCurve(552,571, 970),
            new FlowControlCurve(3, 3, 2),
            new FlowControlCurve(3,0, 5),
            new FlowControlCurve(4, 12, 4),            
            new FlowControlCurve(3,12, 3),
            new FlowControlCurve(4, 12, 4),            
            new FlowControlCurve(3,11, 3),
            new FlowControlCurve(5, 12, 4),            
            new FlowControlCurve(3,11, 3)
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] {testCase};
        }
    }

    [Theory]
    [MemberData(nameof(StaircaseSelfTestCases))]
    public void ConvolutionEquivalence_Self(FlowControlCurve curve)
    {
        var optimizedConvolution = Curve.Convolution(curve, curve);

        var castCurve = new Curve(curve);
        var unoptimizedConvolution = Curve.Convolution(castCurve, castCurve);

        Assert.True(Curve.Equivalent(optimizedConvolution, unoptimizedConvolution));
        Assert.True(Curve.Equivalent(optimizedConvolution, curve));
    }

    /// <summary>Finite at the multiples of <paramref name="period"/>, where it grows by <paramref name="height"/>, and $+\infty$ between them.</summary>
    private static SubAdditiveCurve PlusInfiniteBetweenPoints(Rational period, Rational height)
        => new(new Curve(new Sequence([Point.Origin(), Segment.PlusInfinite(0, period)]), 0, period, height));

    /// <summary>$b + r t$ on $]0, t_0[$, and $-\infty$ from <paramref name="t0"/> on.</summary>
    private static SubAdditiveCurve UltimatelyMinusInfinite(Rational burst, Rational rate, Rational t0)
        => new(new Curve(new Sequence([Point.Origin(), new Segment(0, t0, burst, rate), Point.MinusInfinite(t0), Segment.MinusInfinite(t0, t0 + 1)]), t0, 1, 0));

    /// <summary>
    /// Sub-additive operands reaching infinities, each taking a different path of the optimized convolution.
    /// A sparse operand against a finite one of equal slope has its minimum self-convolved, an operand reaching $-\infty$ admits no asymptotic dominance, and sparse operands come with equal and different slopes.
    /// </summary>
    public static List<(SubAdditiveCurve a, SubAdditiveCurve b)> PairsWithInfinities =
    [
        (new SubAdditiveCurve(new StairCurve(1, 2)), PlusInfiniteBetweenPoints(3, new Rational(3, 2))),
        (PlusInfiniteBetweenPoints(3, new Rational(3, 2)), new SubAdditiveCurve(new StairCurve(1, 2))),
        (new SubAdditiveCurve(new SigmaRhoArrivalCurve(1, 1)), UltimatelyMinusInfinite(2, new Rational(1, 2), 5)),
        (UltimatelyMinusInfinite(1, 1, 3), UltimatelyMinusInfinite(2, new Rational(1, 2), 5)),
        (PlusInfiniteBetweenPoints(2, 2), new SubAdditiveCurve(new SigmaRhoArrivalCurve(1, new Rational(1, 2)))),
        (PlusInfiniteBetweenPoints(2, 1), PlusInfiniteBetweenPoints(3, new Rational(3, 2))),
        (new SubAdditiveCurve(new StairCurve(1, 2)), new SubAdditiveCurve(new StairCurve(2, 3))),
    ];

    public static IEnumerable<object[]> PairsWithInfinitiesTestCases()
        => PairsWithInfinities.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(PairsWithInfinitiesTestCases))]
    public void TheOptimizedConvolutionAgreesWithTheGeneralOneOverInfinities(SubAdditiveCurve a, SubAdditiveCurve b)
    {
        var optimized = Curve.Convolution(a, b);
        var general = Curve.Convolution(new Curve(a), new Curve(b));

        Assert.True(Curve.Equivalent(optimized, general));
    }

    /// <summary>
    /// A convolution of a curve reaching $+\infty$ with one reaching $-\infty$ is undefined, and both algorithms say so, in either order.
    /// </summary>
    public static List<(SubAdditiveCurve a, SubAdditiveCurve b)> OppositeInfinitiesPairs =
    [
        (PlusInfiniteBetweenPoints(2, 1), UltimatelyMinusInfinite(1, 1, 3)),
        (UltimatelyMinusInfinite(1, 1, 3), PlusInfiniteBetweenPoints(2, 1)),
    ];

    public static IEnumerable<object[]> OppositeInfinitiesPairsTestCases()
        => OppositeInfinitiesPairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(OppositeInfinitiesPairsTestCases))]
    public void AConvolutionOfOppositeInfinitiesIsUndefinedEitherWay(SubAdditiveCurve a, SubAdditiveCurve b)
    {
        Assert.Throws<UndeterminedResultException>(() => Curve.Convolution(a, b));
        Assert.Throws<UndeterminedResultException>(() => Curve.Convolution(new Curve(a), new Curve(b)));
    }

    /// <summary>
    /// Sub-additive operands with $f(0) = 0$ of every kind that admits infinities: finite, $+\infty$ between points, and ultimately $-\infty$.
    /// Every pair is convolved both ways, by the optimized algorithm and by the general one.
    /// </summary>
    private static readonly SubAdditiveCurve[] OperandKinds =
    [
        new(new SigmaRhoArrivalCurve(1, 1)), new(new SigmaRhoArrivalCurve(3, new Rational(1, 2))), new(new SigmaRhoArrivalCurve(2, 2)),
        new(new StairCurve(1, 2)), new(new StairCurve(2, 3)),
        new(new Curve(new Sequence([Point.Origin(), new Segment(0, 2, 1, 2), new Point(2, 5), new Segment(2, 3, 5, 1)]), 2, 1, 1)),
        PlusInfiniteBetweenPoints(2, 1), PlusInfiniteBetweenPoints(3, 1), PlusInfiniteBetweenPoints(2, 2), PlusInfiniteBetweenPoints(3, new Rational(3, 2)),
        UltimatelyMinusInfinite(1, 1, 3), UltimatelyMinusInfinite(2, new Rational(1, 2), 5)
    ];

    public static List<(SubAdditiveCurve a, SubAdditiveCurve b)> OperandKindPairs =
        (from a in OperandKinds from b in OperandKinds select (a, b)).ToList();

    public static IEnumerable<object[]> OperandKindPairsTestCases()
        => OperandKindPairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(OperandKindPairsTestCases))]
    public void TheOptimizedConvolutionAgreesWithTheGeneralOneOrBothAreUndefined(SubAdditiveCurve a, SubAdditiveCurve b)
    {
        Curve general;
        try
        {
            general = Curve.Convolution(new Curve(a), new Curve(b));
        }
        catch (UndeterminedResultException)
        {
            Assert.Throws<UndeterminedResultException>(() => Curve.Convolution(a, b));
            return;
        }

        Assert.True(Curve.Equivalent(Curve.Convolution(a, b), general));
    }

    /// <summary>
    /// Pairs the optimized convolution leaves to the general algorithm: opposite infinities, and a minimum the predicate does not vouch for.
    /// </summary>
    public static List<(SubAdditiveCurve a, SubAdditiveCurve b)> PairsLeftToTheGeneralAlgorithm =
    [
        (PlusInfiniteBetweenPoints(2, 1), UltimatelyMinusInfinite(1, 1, 3)),
        (UltimatelyMinusInfinite(1, 1, 3), PlusInfiniteBetweenPoints(2, 1)),
        // +inf between points with the lower slope, so the minimum is not vouched for
        (PlusInfiniteBetweenPoints(3, 1), new SubAdditiveCurve(new SigmaRhoArrivalCurve(1, 1))),
        (new SubAdditiveCurve(new SigmaRhoArrivalCurve(1, 1)), PlusInfiniteBetweenPoints(3, 1)),
    ];

    public static IEnumerable<object[]> PairsLeftToTheGeneralAlgorithmTestCases()
        => PairsLeftToTheGeneralAlgorithm.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(PairsLeftToTheGeneralAlgorithmTestCases))]
    public void TheEstimateCountsTheGeneralAlgorithmWhereTheConvolutionTakesIt(SubAdditiveCurve a, SubAdditiveCurve b)
    {
        var generalA = new Curve(a);
        var generalB = new Curve(b);

        Assert.Equal(generalA.EstimateConvolution(generalB), a.EstimateConvolution(b));
        Assert.Equal(generalA.EstimateConvolution(generalB, countElements: true), a.EstimateConvolution(b, countElements: true));
    }
}
