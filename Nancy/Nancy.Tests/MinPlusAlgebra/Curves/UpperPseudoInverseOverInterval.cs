using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class UpperPseudoInverseOverInterval
{
    public static IEnumerable<object[]> UpiOverPeriodTestCases()
    {
        var testcases = new (Curve operand, Curve expected)[]
        {
            (
                operand: new Curve(
                    baseSequence: new Sequence(new List<Element>()
                    {
                        Point.Origin(),
                        Segment.Zero(0,1),
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
                        Point.MinusInfinite(0),
                        Segment.MinusInfinite(0,1),
                        new Point(1, 3),
                        new Segment(1, 2, 3, 1)
                    }),
                    pseudoPeriodStart: 1,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 2
                )
            ),
            (
                operand: new Curve(
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
                        Point.MinusInfinite(0),
                        Segment.MinusInfinite(0,1),
                        new Point(1, 3),
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
                        Point.MinusInfinite(0),
                        Segment.MinusInfinite(0, 2),
                        Point.PlusInfinite(2),
                        Segment.PlusInfinite(2, 3)
                    }),
                    pseudoPeriodStart: 2,
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
                        Point.MinusInfinite(0),
                        Segment.MinusInfinite(0, 2),
                        new Point(2, 2),
                        Segment.Constant(2, 3, 2)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 0
                )
            )
        };

        foreach (var (operand, expected) in testcases)
        {
            yield return new object[] { operand, expected };
        }
    }

    [Theory]
    [MemberData(nameof(UpiOverPeriodTestCases))]
    public void UpiOverPeriod(Curve operand, Curve expected)
    {
        var upi = operand.UpperPseudoInverseOverInterval(operand.PseudoPeriodStart, Rational.PlusInfinity);
        Assert.True(Curve.Equivalent(expected, upi));
    }

    public static IEnumerable<object[]> UpiOverFiniteIntervalTestCases()
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
                        Segment.Zero(0,1),
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
                        new Point(0, 1),
                        new Segment(0, 1, 1, 2),
                        Point.MinusInfinite(1),
                        Segment.MinusInfinite(1,2)
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
                        Segment.Zero(0,1),
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
                        Point.MinusInfinite(0),
                        Segment.MinusInfinite(0,2),
                        new Point(2, 5),
                        new Segment(2, 3, 5, 1),
                        new Point(3, 7),
                        Segment.MinusInfinite(3,4),
                        Point.MinusInfinite(4),
                        Segment.MinusInfinite(4,5),
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
    [MemberData(nameof(UpiOverFiniteIntervalTestCases))]
    public void UpiOverFiniteInterval(
        Curve operand, 
        Rational start, 
        Rational end,
        bool isStartIncluded,
        bool isEndIncluded,
        Curve expected
    )
    {
        var upi = operand.UpperPseudoInverseOverInterval(start, end, isStartIncluded, isEndIncluded);
        Assert.True(Curve.Equivalent(expected, upi));
    }

    public static IEnumerable<object[]> UpiOverZeroToInfTestCases()
    {
        foreach (var testCase in UpperPseudoInverse.ContinuousTestCases())
            yield return new object[] { testCase[0] };

        foreach (var testCase in UpperPseudoInverse.LeftContinuousTestCases())
            yield return new object[] { testCase[0] };

        foreach (var testCase in UpperPseudoInverse.RightContinuousTestCases())
            yield return new object[] { testCase[0] };

        foreach (var testCase in UpperPseudoInverse.CornerTestCases())
            yield return new object[] { testCase[0] };

        foreach (var testCase in ConvolutionIsomorphismOverInterval.UpiDecompositionEquivalenceTestCases())
            yield return new object[] { testCase[0] };
    }

    [Theory]
    [MemberData(nameof(UpiOverZeroToInfTestCases))]
    public void UpiOverZeroToInf(Curve f)
    {
        var f_upi = f.UpperPseudoInverse();
        var f_upi_D = f.UpperPseudoInverseOverInterval(0, Rational.PlusInfinity);

        Assert.True(Curve.Equivalent(f_upi, f_upi_D));
    }
}