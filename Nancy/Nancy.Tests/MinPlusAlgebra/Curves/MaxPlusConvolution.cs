using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class MaxPlusConvolution
{
    private readonly ITestOutputHelper output;

    public MaxPlusConvolution(ITestOutputHelper output)
    {
        this.output = output;
    }
    
    public static ComputationSettings settings = ComputationSettings.Default() with 
    {
        UseConvolutionIsomorphismOptimization = false,
        UseSubAdditiveConvolutionOptimizations = false
    };
    public static IEnumerable<(Curve f, Curve g, Curve expected)> PairsWithExpected()
    {
        var testcases = new List<(Curve f, Curve g, Curve expected)>
        {
            (
                f: new Curve(
                    baseSequence: new Sequence(new List<Element>()
                    {
                        Point.Origin(),
                        new Segment(0,1, 0, 1),
                        new Point(1, 1),
                        Segment.Constant(1, 3, 1),
                        new Point(3, 1),
                        new Segment(3,4, 1, 1)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 2,
                    pseudoPeriodHeight: 1
                ),
                g: new Curve(
                    baseSequence: new Sequence(new List<Element>()
                    {
                        Point.Origin(),
                        new Segment(0,1, 0, 1),
                        new Point(1, 1),
                        Segment.Constant(1, 3, 1),
                        new Point(3, 1),
                        new Segment(3,4, 1, 1)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 2,
                    pseudoPeriodHeight: 1
                ),
                expected: new Curve(
                    baseSequence: new Sequence(new List<Element>()
                    {
                        Point.Origin(),
                        new Segment(0,2, 0, 1),
                        new Point(2, 2),
                        Segment.Constant(2, 3, 2),
                        new Point(3, 2),
                        Segment.Constant(3,4, 2),
                        new Point(4, 2),
                        new Segment(4,5, 2, 1),
                    }),
                    pseudoPeriodStart: 3,
                    pseudoPeriodLength: 2,
                    pseudoPeriodHeight: 1
                )
            ),
            (
                f: new Curve(
                    baseSequence: new Sequence(new List<Element>()
                    {
                        new Point(time: 0, value: 0),
                        new Segment(startTime: 0, endTime: 1, slope: 1, rightLimitAtStartTime: 0),
                        new Point(time: 1, value: 1),
                        new Segment(startTime: 1, endTime: 2, slope: 0, rightLimitAtStartTime: 1),
                        new Point(time: 2, value: 1),
                        new Segment(startTime: 2, endTime: 3, slope: 0, rightLimitAtStartTime: 1),
                        new Point(time: 3, value: 1),
                        new Segment(startTime: 3, endTime: 4, slope: 1, rightLimitAtStartTime: 1)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 2,
                    pseudoPeriodHeight: 1
                ),
                g: new Curve(
                    baseSequence: new Sequence(new List<Element>()
                    {
                        new Point(time: 0, value: 0),
                        new Segment(startTime: 0, endTime: 1, slope: 1, rightLimitAtStartTime: 0),
                        new Point(time: 1, value: 1),
                        new Segment(startTime: 1, endTime: 2, slope: 0, rightLimitAtStartTime: 1),
                        new Point(time: 2, value: 1),
                        new Segment(startTime: 2, endTime: 3, slope: 0, rightLimitAtStartTime: 1),
                        new Point(time: 3, value: 1),
                        new Segment(startTime: 3, endTime: 4, slope: 1, rightLimitAtStartTime: 1)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 2,
                    pseudoPeriodHeight: 1
                ),
                expected: new Curve(
                    baseSequence: new Sequence(new List<Element>()
                    {
                        Point.Origin(),
                        new Segment(0,2, 0, 1),
                        new Point(2, 2),
                        Segment.Constant(2, 3, 2),
                        new Point(3, 2),
                        Segment.Constant(3,4, 2),
                        new Point(4, 2),
                        new Segment(4,5, 2, 1),
                    }),
                    pseudoPeriodStart: 3,
                    pseudoPeriodLength: 2,
                    pseudoPeriodHeight: 1
                )
            ),
            (
                f: new Curve(
                    baseSequence: new Sequence(new List<Element>()
                    {
                        new Point(0, 0),
                        new Segment(0, 2, 0, 0),
                        new Point(2, 4),
                        new Segment(2, 3, 4, new Rational(1, 2))
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: new Rational(1, 2)
                ),
                g: new Curve(
                    baseSequence: new Sequence(new List<Element>()
                    {
                        new Point(0, 0),
                        new Segment(0, 2, 0, 0),
                        new Point(2, 4),
                        new Segment(2, 3, 4, new Rational(1, 2))
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: new Rational(1, 2)
                ),
                expected: new Curve(
                    baseSequence: new Sequence(new List<Element>()
                    {
                        new Point(0, 0),
                        new Segment(0, 2,  0, 0),
                        new Point(2, 4),
                        new Segment(2, 4, 4, new Rational(1, 2)),
                        new Point(4, 8),
                        new Segment(4, 6, 8, new Rational(1, 2)),
                    }),
                    pseudoPeriodStart: 4,
                    pseudoPeriodLength: 2,
                    pseudoPeriodHeight: 1
                )
            ),
            (
                f: Curve.FromJson(
                    "{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":0,\"type\":\"point\"},{\"startTime\":0,\"endTime\":2,\"rightLimitAtStartTime\":0,\"slope\":0,\"type\":\"segment\"},{\"time\":2,\"value\":0,\"type\":\"point\"},{\"startTime\":2,\"endTime\":3,\"rightLimitAtStartTime\":0,\"slope\":1,\"type\":\"segment\"},{\"time\":3,\"value\":2,\"type\":\"point\"},{\"startTime\":3,\"endTime\":5,\"rightLimitAtStartTime\":2,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":3,\"periodLength\":2,\"periodHeight\":1}"
                ),
                g: Curve.FromJson(
                    "{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":0,\"type\":\"point\"},{\"startTime\":0,\"endTime\":6,\"rightLimitAtStartTime\":0,\"slope\":0,\"type\":\"segment\"},{\"time\":6,\"value\":0,\"type\":\"point\"},{\"startTime\":6,\"endTime\":8,\"rightLimitAtStartTime\":0,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":6,\"periodLength\":2,\"periodHeight\":3}"
                ),
                expected: Curve.FromJson(
                    "{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":0,\"type\":\"point\"},{\"startTime\":0,\"endTime\":2,\"slope\":0,\"rightLimitAtStartTime\":0,\"type\":\"segment\"},{\"time\":2,\"value\":0,\"type\":\"point\"},{\"startTime\":2,\"endTime\":3,\"slope\":1,\"rightLimitAtStartTime\":0,\"type\":\"segment\"},{\"time\":3,\"value\":2,\"type\":\"point\"},{\"startTime\":3,\"endTime\":5,\"slope\":0,\"rightLimitAtStartTime\":2,\"type\":\"segment\"},{\"time\":5,\"value\":3,\"type\":\"point\"},{\"startTime\":5,\"endTime\":7,\"slope\":0,\"rightLimitAtStartTime\":3,\"type\":\"segment\"},{\"time\":7,\"value\":4,\"type\":\"point\"},{\"startTime\":7,\"endTime\":9,\"slope\":0,\"rightLimitAtStartTime\":4,\"type\":\"segment\"},{\"time\":9,\"value\":5,\"type\":\"point\"},{\"startTime\":9,\"endTime\":10,\"slope\":0,\"rightLimitAtStartTime\":5,\"type\":\"segment\"},{\"time\":10,\"value\":6,\"type\":\"point\"},{\"startTime\":10,\"endTime\":12,\"slope\":0,\"rightLimitAtStartTime\":6,\"type\":\"segment\"}]},\"periodStart\":10,\"periodLength\":2,\"periodHeight\":3}"
                )
            ),
            (
                f: new Curve(baseSequence: new Sequence(new List<Element>{new Point(0,1),new Segment(0,1,1,2),new Point(1,3),new Segment(1,2,3,1),}),pseudoPeriodStart: 1,pseudoPeriodLength: 1,pseudoPeriodHeight: 2),
                g: new Curve(baseSequence: new Sequence(new List<Element>{new Point(0,new Rational(-1, 0)),new Segment(0,4,new Rational(-1, 0),0),new Point(4,0),new Segment(4,5,0,2),}),pseudoPeriodStart: 4,pseudoPeriodLength: 1,pseudoPeriodHeight: 2),
                expected: new Curve(baseSequence: new Sequence(new List<Element>{new Point(0,new Rational(-1, 0)),new Segment(0,4,new Rational(-1, 0),0),new Point(4,1),new Segment(4,new Rational(22, 5),1,2),}),pseudoPeriodStart: 4,pseudoPeriodLength: new Rational(2, 5),pseudoPeriodHeight: new Rational(4, 5))
            )
        };

        return testcases;
    }

    public static IEnumerable<(Curve f, Curve g)> PairTestCases()
    {
        foreach (var (f, g, _) in PairsWithExpected())
        {
            yield return (f, g);
        }

        foreach (var objArray in ConvolutionIsomorphism.MaxPlusIsomorphismTestCases())
        {
            var curves = (Curve[]) objArray[0];
            for (int i = 0; i <= curves.Length - 2; i++)
            {
                var a = curves[i];
                var b = curves[i + 1];

                yield return (a, b);
            }
        }
    }

    public static IEnumerable<object[]> MaxPlusConvolutionEquivalenceTestCases()
    {
        foreach (var (f, g, expected) in PairsWithExpected())
        {
            yield return new object[] { f, g, expected };
        }
    }

    [Theory]
    [MemberData(nameof(MaxPlusConvolutionEquivalenceTestCases))]
    void MaxPlusConvolutionEquivalence(Curve f, Curve g, Curve expected)
    {
        var result = Curve.MaxPlusConvolution(f, g, settings: settings);
        Assert.True(Curve.Equivalent(expected, result));
    }

    [Theory]
    [MemberData(nameof(MaxPlusConvolutionEquivalenceTestCases))]
    void MaxPlusSetConvolutionEquivalence(Curve f, Curve g, Curve expected)
    {
        var result_pair = Curve.MaxPlusConvolution(f, g, settings: settings);
        var result_set = Curve.MaxPlusConvolution(new List<Curve> {f, g}, settings: settings);

        Assert.True(Curve.Equivalent(result_pair, result_set));
        Assert.True(Curve.Equivalent(expected, result_pair));
    }

    public static IEnumerable<object[]> MaxPlusConvolutionViaNegativeTestCases()
    {
        foreach (var (f, g, _) in PairsWithExpected())
        {
            yield return new object[] { f, g };
        }

        foreach (var (f, g) in ConvolutionIsomorphism.MaxPlusConvolutionPairs())
        {
            yield return new object[] { f, g };
        }
    }

    /// <summary>
    /// Tests the equivalence in [DNC18] Section 2.4
    /// </summary>
    [Theory]
    [MemberData(nameof(MaxPlusConvolutionViaNegativeTestCases))]
    void MaxPlusConvolutionViaNegative(Curve a, Curve b)
    {
        var maxConv = Curve.MaxPlusConvolution(a, b, settings);
        var viaNegative = -Curve.Convolution(-a, -b, settings);

        Assert.True(Curve.Equivalent(maxConv, viaNegative));
    }

    public static IEnumerable<object[]> RightContinuityBothTestCases()
    {
        var rc = ConvolutionIsomorphism.ContinuousExamples
            .Concat(ConvolutionIsomorphism.RightContinuousExamples);
        var testcases = rc.SelectMany(f => rc.Select(g => (f, g)));

        foreach (var (f, g) in testcases)
            yield return new object[] { f, g };
    }

    [Theory]
    [MemberData(nameof(RightContinuityBothTestCases))]
    public void RightContinuityBoth(Curve f, Curve g)
    {
        Assert.True(f.IsRightContinuous && g.IsRightContinuous);
        var conv = Curve.MaxPlusConvolution(f, g, settings);

        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");
        output.WriteLine($"var conv = {conv.ToCodeString()};");

        Assert.True(conv.IsRightContinuous);
    }
    
    #if ONE_SIDED_RIGHT_CONTINUITY_TH
    // The following tests show that the analogous result of [Lie17, p.134], 
    // for which it is sufficient that one of the operands is right-continuous for the (max,+) convolution to be right-continuous, 
    // does *not* apply for functions defined only in [0, +\infty[

    public static IEnumerable<object[]> RightContinuityAtLeastOneTestCases()
    {
        var rc = ConvolutionIsomorphism.ContinuousExamples
            .Concat(ConvolutionIsomorphism.RightContinuousExamples);
        var lc = ConvolutionIsomorphism.LeftContinuousExamples;
        var testcases = rc.SelectMany(f => lc.Select(g => (f, g)));

        foreach (var (f, g) in testcases)
            yield return new object[] { f, g };
    }

    [Theory]
    [MemberData(nameof(RightContinuityAtLeastOneTestCases))]
    public void RightContinuityAtLeastOne(Curve f, Curve g)
    {
        Assert.True(f.IsRightContinuous || g.IsRightContinuous);
        var conv = Curve.MaxPlusConvolution(f, g, settings);

        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");
        output.WriteLine($"var conv = {conv.ToCodeString()};");

        Assert.True(conv.IsRightContinuous);
    }
    #endif
}