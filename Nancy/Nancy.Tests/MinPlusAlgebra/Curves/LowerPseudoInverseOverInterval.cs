using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveLowerPseudoInverseOverInterval
{
    private readonly ITestOutputHelper output;

    public CurveLowerPseudoInverseOverInterval(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static List<(Curve operand, Curve expected)> LpiOverPeriodPairs = new List<(Curve operand, Curve expected)>
    {
        (
            operand: new Curve(
                baseSequence: new Sequence(new List<Element>()
                {
                    Point.Origin(),
                    Segment.Zero(0, 1),
                    new Point(1, 0),
                    new Segment(1, 3, 0, 0.5m),
                    new Point(3, 1),
                    new Segment(3, 4, 1, 1),
                    new Point(4, 2),
                    Segment.Constant(4, 5, 2)
                }),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 1
            ),
            expected: new Curve(
                baseSequence: new Sequence(new List<Element>()
                {
                    Point.PlusInfinite(0),
                    Segment.PlusInfinite(0, 1),
                    new Point(1, 3),
                    new Segment(1, 2, 3, 1),
                    new Point(2, 4),
                    new Segment(2, 3, 5, 1)
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 2
            )
        ),
        (
            operand: new Curve(
                baseSequence: new Sequence(new List<Element>()
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    Segment.Constant(1, 3, 1),
                    new Point(3, 1),
                    new Segment(3, 4, 1, 1)
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 1
            ),
            expected: new Curve(
                baseSequence: new Sequence(new List<Element>()
                {
                    Point.PlusInfinite(0),
                    Segment.PlusInfinite(0, 1),
                    new Point(1, 2),
                    new Segment(1, 2, 3, 1)
                }),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 2
            )
        ),
        // UC edge case
        (
            operand: new Curve(
                baseSequence: new Sequence(new List<Element>()
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    new Segment(1, 2, 1, 1),
                    new Point(2, 2),
                    Segment.Constant(2, 3, 2)
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            expected: new Curve(
                baseSequence: new Sequence(new List<Element>()
                {
                    Point.PlusInfinite(0),
                    Segment.PlusInfinite(0, 2),
                    new Point(2, 2),
                    Segment.PlusInfinite(2, 3),
                    Point.PlusInfinite(3),
                    Segment.PlusInfinite(3, 4),
                }),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            )
        ),
        // UI edge case
        (
            operand: new Curve(
                baseSequence: new Sequence(new List<Element>()
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    new Segment(1, 2, 1, 1),
                    Point.PlusInfinite(2),
                    Segment.PlusInfinite(2, 3)
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            expected: new Curve(
                baseSequence: new Sequence(new List<Element>()
                {
                    Point.PlusInfinite(0),
                    Segment.PlusInfinite(0, 2),
                    new Point(2, 2),
                    Segment.Constant(2, 3, 2)
                }),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            )
        )
    };

    public static IEnumerable<object[]> LpiOverPeriodTestCases()
    {
        foreach (var (operand, expected) in LpiOverPeriodPairs)
        {
            yield return new object[] { operand, expected };
        }
    }

    [Theory]
    [MemberData(nameof(LpiOverPeriodTestCases))]
    public void LpiOverPeriod(Curve operand, Curve expected)
    {
        var lpi = operand.LowerPseudoInverseOverInterval(operand.PseudoPeriodStart, Rational.PlusInfinity);
        Assert.True(Curve.Equivalent(expected, lpi));
    }

    public static IEnumerable<object[]> LpiOverFiniteIntervalTestCases()
    {
        var testcases = new (
            Curve operand,
            Rational start,
            Rational end,
            bool isStartIncluded,
            bool isEndIncluded,
            Curve expected
            )[]
            {
                (
                    operand: new Curve(
                        baseSequence: new Sequence(new List<Element>()
                        {
                            Point.Origin(),
                            Segment.Zero(0, 1),
                            new Point(1, 0),
                            new Segment(1, 3, 0, 0.5m),
                            new Point(3, 1),
                            new Segment(3, 4, 1, 1),
                            new Point(4, 2),
                            Segment.Constant(4, 5, 2)
                        }),
                        pseudoPeriodStart: 3,
                        pseudoPeriodLength: 2,
                        pseudoPeriodHeight: 1
                    ),
                    start: 0,
                    end: 3,
                    isStartIncluded: true,
                    isEndIncluded: false,
                    expected: new Curve(
                        baseSequence: new Sequence(new List<Element>()
                        {
                            Point.Origin(),
                            new Segment(0, 1, 1, 2),
                            Point.PlusInfinite(1),
                            Segment.PlusInfinite(1, 2)
                        }),
                        pseudoPeriodStart: 1,
                        pseudoPeriodLength: 1,
                        pseudoPeriodHeight: 0
                    )
                ),
                (
                    operand: new Curve(
                        baseSequence: new Sequence(new List<Element>()
                        {
                            Point.Origin(),
                            Segment.Zero(0, 1),
                            new Point(1, 0),
                            new Segment(1, 3, 0, 0.5m),
                            new Point(3, 1),
                            new Segment(3, 4, 1, 1),
                            new Point(4, 2),
                            Segment.Constant(4, 5, 2)
                        }),
                        pseudoPeriodStart: 3,
                        pseudoPeriodLength: 2,
                        pseudoPeriodHeight: 1
                    ),
                    start: 4,
                    end: 7,
                    isStartIncluded: false,
                    isEndIncluded: false,
                    expected: new Curve(
                        baseSequence: new Sequence(new List<Element>()
                        {
                            Point.PlusInfinite(0),
                            Segment.PlusInfinite(0, 2),
                            new Point(2, 4),
                            new Segment(2, 3, 5, 1),
                            new Point(3, 6),
                            Segment.PlusInfinite(3, 4),
                            Point.PlusInfinite(4),
                            Segment.PlusInfinite(4, 5),
                        }),
                        pseudoPeriodStart: 4,
                        pseudoPeriodLength: 1,
                        pseudoPeriodHeight: 0
                    )
                )
            };

        foreach (var (operand, start, end, isStartIncluded, isEndIncluded, expected) in testcases)
        {
            yield return new object[] { operand, start, end, isStartIncluded, isEndIncluded, expected };
        }
    }

    [Theory]
    [MemberData(nameof(LpiOverFiniteIntervalTestCases))]
    public void LpiOverFiniteInterval(
        Curve operand,
        Rational start,
        Rational end,
        bool isStartIncluded,
        bool isEndIncluded,
        Curve expected
    )
    {
        var lpi = operand.LowerPseudoInverseOverInterval(start, end, isStartIncluded, isEndIncluded);
        Assert.True(Curve.Equivalent(expected, lpi));
    }

    public static IEnumerable<object[]> LpiOverZeroToInfTestCases()
    {
        foreach (var testCase in LowerPseudoInverse.ContinuousTestCases())
            yield return new object[] { testCase[0] };

        foreach (var testCase in LowerPseudoInverse.LeftContinuousTestCases())
            yield return new object[] { testCase[0] };

        foreach (var testCase in LowerPseudoInverse.RightContinuousTestCases())
            yield return new object[] { testCase[0] };

        foreach (var testCase in LowerPseudoInverse.CornerTestCases())
            yield return new object[] { testCase[0] };

        foreach (var testCase in ConvolutionIsomorphismOverInterval.LpiDecompositionEquivalenceTestCases())
            yield return new object[] { testCase[0] };
    }

    [Theory]
    [MemberData(nameof(LpiOverZeroToInfTestCases))]
    public void LpiOverZeroToInf(Curve f)
    {
        var f_lpi = f.LowerPseudoInverse();
        var f_lpi_D = f.LowerPseudoInverseOverInterval(0, Rational.PlusInfinity);

        Assert.True(Curve.Equivalent(f_lpi, f_lpi_D));
    }

    public static IEnumerable<object[]> LpiOverPeriod_PeriodStart_TestCases()
    {
        var curves =
            LpiOverPeriodPairs.Select(tuple => tuple.operand)
                .Concat(ConvolutionIsomorphism.ContinuousExamples)
                .Concat(ConvolutionIsomorphism.RightContinuousExamples)
                .Concat(ConvolutionIsomorphism.LeftContinuousExamples);

        foreach (var f in curves)
        {
            if(f.IsUltimatelyAffine || f.IsUltimatelyConstant || f.IsUltimatelyInfinite)
                continue;

            yield return new object[] { f.Optimize() };
        }
    }

    [Theory]
    [MemberData(nameof(LpiOverPeriod_PeriodStart_TestCases))]
    public void LpiOverPeriod_PeriodStart(Curve f)
    {
        output.WriteLine(f.ToCodeString());

        var tf = f.PseudoPeriodStart;
        var f_tf = f.ValueAt(tf);

        var lpip = f.LowerPseudoInverseOverInterval(tf);
        lpip = lpip.Optimize();
        if (f.GetSegmentBefore(tf + f.PseudoPeriodLength) is { IsConstant: true } s && 
            f.IsLeftContinuousAt(tf + f.PseudoPeriodLength))
        {
            output.WriteLine("Case 2");
            Assert.Equal(s.RightLimitAtStartTime, lpip.PseudoPeriodStart);
        }
        else
        {
            output.WriteLine("Case 1");
            Assert.Equal(f_tf, lpip.PseudoPeriodStart);
        }
    }
}