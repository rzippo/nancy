using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveUpperPseudoInverse
{
    public static IEnumerable<object[]> ContinuousTestCases()
    {
        var testcases = new (Curve operand, Curve expected)[]
        {
            (
                operand: new RateLatencyServiceCurve(3, 5),
                expected: new Curve(
                        baseSequence: new Sequence(new Element[]
                        {
                            new Point(0, 5),
                            new Segment(0, 1, 5, new Rational(1, 3))
                        }),
                        pseudoPeriodStart: 0,
                        pseudoPeriodLength: 1,
                        pseudoPeriodHeight: new Rational(1, 3)
                    )
            ),
            (
                operand: new RateLatencyServiceCurve(1, 1),
                expected: new Curve(
                        baseSequence: new Sequence(new Element[]
                        {
                            new Point(0, 1),
                            new Segment(0, 1, 1, 1)
                        }),
                        pseudoPeriodStart: 0,
                        pseudoPeriodLength: 1,
                        pseudoPeriodHeight: 1
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
                expected: new Curve(
                    baseSequence: new Sequence(new List<Element>()
                    {
                        new Point(0, 1),
                        new Segment(0,1, 1, 2),
                        new Point(1, 3),
                        new Segment(1, 2, 3, 1)
                    }),
                    pseudoPeriodStart: 1,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 2
                )
            )
        };

        foreach (var (operand, expected) in testcases)
        {
            if (!operand.IsContinuous) throw new InvalidOperationException();
            yield return new object[] { operand, expected };
        }
    }

    public static IEnumerable<object[]> LeftContinuousTestCases()
    {
        var testcases = new (Curve operand, Curve expected1, Curve expected2)[]
        {
            (
                operand: new Curve(
                    baseSequence: new Sequence( new List<Element>
                    {
                        Point.Origin(),
                        new Segment(0, 1, 0, 1),
                        new Point(1, 1),
                        new Segment(1, 2, 2, 1),
                        new Point(2, 3),
                        new Segment(2, 3, 3, 1)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1
                ),
                expected1: new Curve(
                    baseSequence: new Sequence( new List<Element>
                    {
                        Point.Origin(),
                        new Segment(0, 1, 0, 1),
                        new Point(1, 1),
                        new Segment(1, 2, 1, 0),
                        new Point(2, 1),
                        new Segment(2, 3, 1, 1)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1
                ),
                expected2: new Curve(
                    baseSequence: new Sequence( new List<Element>
                    {
                        Point.Origin(),
                        new Segment(0, 1, 0, 1),
                        new Point(1, 2),
                        new Segment(1, 2, 2, 1),
                        new Point(2, 3),
                        new Segment(2, 3, 3, 1)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1
                )
            ),
            (
                // example from [2] Figure 3.6
                operand: new Curve(
                    baseSequence: new Sequence( new List<Element>
                    {
                        Point.Origin(),
                        Segment.Zero(0, 1),
                        new Point(1, 0),
                        new Segment(1, 2, 0, 2),
                        new Point(2, 2),
                        new Segment(2, 3, 2, 0),
                        new Point(3, 2),
                        new Segment(3, 4, 2, 1),
                        new Point(4, 3),
                        new Segment(4, 5, 4, 2), // right-discontinuity here
                        new Point(5, 6),
                        new Segment(5, 6, 6, 0),
                        new Point(6, 6),
                        new Segment(6, 7, 6, 1)
                    }),
                    pseudoPeriodStart: 6,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1
                ),
                expected1: new Curve(
                    baseSequence: new Sequence( new List<Element>
                    {
                        new Point(0, 1),
                        new Segment(0, 2, 1, new Rational(1, 2)), // right-discontinuity due to constant segment
                        new Point(2, 3),
                        new Segment(2, 3, 3, 1), // right-discontinuity due to constant segment
                        new Point(3, 4),
                        new Segment(3, 4, 4, 0), // constant segment due to right-discontinuity
                        new Point(4, 4),
                        new Segment(4, 6, 4, new Rational(1, 2)),
                        new Point(6, 6),
                        new Segment(6, 7, 6, 1), // right-discontinuity due to constant segment
                        new Point(7, 7), // move pseudo-period forward because the discontinuity is not part of the pseudo-period
                        new Segment(7, 8, 7, 1),
                    }),
                    pseudoPeriodStart: 7,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1
                ),
                expected2: new Curve(
                    baseSequence: new Sequence( new List<Element>
                    {
                        // Initial latency becomes -infty
                        Point.MinusInfinite(0),
                        Segment.MinusInfinite(0, 1),
                        new Point(1, 0),
                        new Segment(1, 2, 0, 2),
                        new Point(2, 2),
                        new Segment(2, 3, 2, 0),
                        new Point(3, 2),
                        new Segment(3, 4, 2, 1),
                        new Point(4, 4),
                        new Segment(4, 5, 4, 2), // right-discontinuity here
                        new Point(5, 6),
                        new Segment(5, 6, 6, 0),
                        new Point(6, 6),
                        new Segment(6, 7, 6, 1)
                    }),
                    pseudoPeriodStart: 6,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1
                )
            )
        };

        foreach (var (operand, expected1, expected2) in testcases)
        {
            if (!operand.IsLeftContinuous) throw new InvalidOperationException();
            yield return new object[] { operand, expected1, expected2 };
        }
    }

    public static IEnumerable<object[]> RightContinuousTestCases()
    {
        var testcases = new (Curve operand, Curve expected)[]
        {
            (
                operand: new Curve(
                    baseSequence: new Sequence( new List<Element>
                    {
                        Point.Origin(),
                        new Segment(0, 1, 0, 1),
                        new Point(1, 2),
                        new Segment(1, 2, 2, 1)
                    }),
                    pseudoPeriodStart: 1,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1
                ),
                expected: new Curve(
                    baseSequence: new Sequence( new List<Element>
                    {
                        Point.Origin(),
                        new Segment(0, 1, 0, 1),
                        new Point(1, 1),
                        new Segment(1, 2, 1, 0),
                        new Point(2, 1),
                        new Segment(2, 3, 1, 1)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1
                )
            ),
            (
                // discontinuity at each pseudo-period
                operand: new Curve( 
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 1, 0, 2),
                        new Point(1, 2),
                        new Segment(1, 2, 2, 1)
                    }),
                    pseudoPeriodStart: 1,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 2
                ),
                expected: new Curve( 
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 2, 0, new Rational(1, 2)),
                        new Point(2, 1),
                        new Segment(2, 3, 1, 1),
                        new Point(3, 2),
                        Segment.Constant(3, 4, 2)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 2,
                    pseudoPeriodHeight: 1
                )
            ),
            (
                // discontinuity and constant segment at each pseudo-period
                operand: new Curve( 
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 1, 0, 2),
                        new Point(1, 2),
                        Segment.Constant(1, 2, 2)
                    }),
                    pseudoPeriodStart: 1,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 2
                ),
                expected: new Curve( 
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 2, 0, new Rational(1, 2)),
                        new Point(2, 2),
                        Segment.Constant(2, 4, 2)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 2,
                    pseudoPeriodHeight: 1
                )
            )
        };

        foreach (var (operand, expected) in testcases)
        {
            if (!operand.IsRightContinuous) throw new InvalidOperationException();
            yield return new object[] { operand, expected };
        }
    }

    public static IEnumerable<object[]> CornerTestCases()
    {
        var testcases = new (Curve operand, Curve expected)[]
        {
            (
                // ultimately constant
                operand: Curve.Minimum(
                    new RateLatencyServiceCurve(4, 0),
                    new ConstantCurve(12)
                ),
                expected: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 12, 0, new Rational(1, 4)),
                        Point.PlusInfinite(12),
                        Segment.PlusInfinite(12, 13)
                    }),
                    pseudoPeriodStart: 12,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 0
                )
            ),
            (
                // ultimately constant
                operand: Curve.Minimum(
                    new RateLatencyServiceCurve(3, 3),
                    new ConstantCurve(12)
                ),
                expected: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        new Point(0, 3),
                        new Segment(0, 12, 3, new Rational(1, 3)),
                        Point.PlusInfinite(12),
                        Segment.PlusInfinite(12, 13)
                    }),
                    pseudoPeriodStart: 12,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 0
                )
            ),
            (
                operand: new DelayServiceCurve(0),
                expected: Curve.Zero()
            ),
            (
                // f(0) = v > 0 implies upi -inf in [0, v[
                operand: new Curve(
                    baseSequence: new Sequence( new List<Element>
                    {
                        new Point(0, 2),
                        new Segment(0, 1, 2, 1)
                    }),
                    0,
                    1,
                    1
                ),
                expected: new Curve(
                    baseSequence: new Sequence(new List<Element>
                    {
                        new Point(0, Rational.MinusInfinity),
                        Segment.MinusInfinite(0, 2),
                        new Point(2, 0),
                        new Segment(2, 3, 0, 1)
                    }),
                    2,
                    1,
                    1
                )
            ),
            (
                // just madness
                operand: new Curve(
                    baseSequence: new Sequence(new List<Element>()
                    {
                        new Point(0, 5),
                        Segment.PlusInfinite(0, 1),
                        Point.PlusInfinite(1),
                        Segment.PlusInfinite(1, 2),
                    }),
                    pseudoPeriodStart: 1,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 0
                ),
                expected: Curve.Zero()
            )
        };

        foreach (var (operand, expected) in testcases)
        {
            yield return new object[] { operand, expected };
        }
    }

    public static IEnumerable<object[]> NonRevertibleCornerTestCases()
    {
        var testcases = new (Curve operand, Curve expected1, Curve expected2)[]
        {
            (
                // ultimately constant with jump, non-reversible corner case
                // note also the initial latency, which becomes -infty 
                operand: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        Segment.Zero(0, 1),
                        new Point(1, 0),
                        new Segment(1, 2, 0, 1),
                        new Point(2, 1),
                        Segment.Constant(2, 3, 2),
                        new Point(3, 2),
                        Segment.Constant(3, 4, 2)
                    }),
                    pseudoPeriodStart: 3,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 0
                ),
                expected1: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        new Point(0, 1),
                        new Segment(0, 1, 1, 1),
                        new Point(1, 2),
                        Segment.Constant(1, 2, 2),
                        Point.PlusInfinite(2),
                        Segment.PlusInfinite(2, 3)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 0
                ),
                expected2: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.MinusInfinite(0),
                        Segment.MinusInfinite(0, 1),
                        new Point(1, 0),
                        new Segment(1, 2, 0, 1),
                        new Point(2, 2),
                        Segment.Constant(2, 3, 2)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 0
                )
            ),
            (
                operand: new SigmaRhoArrivalCurve(2, 0),
                expected1: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        Segment.Constant(0, 2, 0),
                        Point.PlusInfinite(2),
                        Segment.PlusInfinite(2, 3)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 0
                ),
                expected2: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        new Point(0, 2),
                        Segment.Constant(0, 1, 2)
                    }),
                    pseudoPeriodStart: 0,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 0
                )
            )
        };

        foreach (var (operand, expected1, expected2) in testcases)
        {
            if (!operand.IsLeftContinuous) throw new InvalidOperationException();
            yield return new object[] { operand, expected1, expected2 };
        }
    }

    [Theory]
    [MemberData(nameof(ContinuousTestCases))]
    [MemberData(nameof(RightContinuousTestCases))]
    public void RevertiblePseudoInverseTest(Curve operand, Curve expected)
    {
        // reversability of upi in U has the extra hypothesis of f(t) > 0 for t > 0
        // otherwise, the initial latency becomes -\infty when reversed

        var result = operand.UpperPseudoInverse();
        Assert.True(result.IsRightContinuous);
        Assert.True(Curve.Equivalent(expected, result));
        Assert.Equal(operand.FirstNonZeroTime, result.ValueAt(0));

        var result2 = result.UpperPseudoInverse();
        Assert.True(result2.IsRightContinuous);
        if (operand.FirstNonZeroTime == 0)
            Assert.True(Curve.Equivalent(operand, result2));
        else
        {
            // initial latency becomes -infty
            Assert.True(Sequence.Equivalent(
                Sequence.MinusInfinite(0, operand.FirstNonZeroTime),
                result2.Cut(0, operand.FirstNonZeroTime)
            ));
            Assert.True(Curve.EquivalentAfter(operand, result2, operand.FirstNonZeroTime));
        }
    }

    [Theory]
    [MemberData(nameof(LeftContinuousTestCases))]
    public void NonRevertibleInverseTest(Curve operand, Curve expected1, Curve expected2)
    {
        var result = operand.UpperPseudoInverse();
        Assert.True(result.IsRightContinuous);
        Assert.True(Curve.Equivalent(expected1, result));

        var result2 = result.UpperPseudoInverse();
        Assert.True(result2.IsRightContinuous);
        Assert.True(Curve.Equivalent(expected2, result2));
    }

    [Theory]
    [MemberData(nameof(CornerTestCases))]
    public void CornerCases(Curve operand, Curve expected)
    {
        var result = operand.UpperPseudoInverse();
        if(!operand.IsUltimatelyConstant && result.ValueAt(0) == 0)
            Assert.True(result.IsLeftContinuous);
        Assert.True(Curve.Equivalent(expected, result));

        if (operand.IsUltimatelyInfinite && operand.PseudoPeriodStartInfimum == 0)
            // uninteresting edge case, don't try to reverse
            return;

        var result2 = result.UpperPseudoInverse();
        if(!result.IsUltimatelyConstant && result.ValueAt(0) == 0)
            Assert.True(result2.IsLeftContinuous);
        Assert.True(Curve.EquivalentAfter(operand, result2, operand.FirstNonZeroTime));
    }

    [Theory]
    [MemberData(nameof(NonRevertibleCornerTestCases))]
    public void NonRevertibleCornerCases(Curve operand, Curve expected1, Curve expected2)
    {
        var result = operand.UpperPseudoInverse();
        Assert.True(Curve.Equivalent(expected1, result));

        var result2 = result.UpperPseudoInverse();
        Assert.True(Curve.Equivalent(expected2, result2));
    }

    public static IEnumerable<object[]> UcPropertiesTestCases()
    {
        var testcases = new Curve[]
        {
            new SigmaRhoArrivalCurve(4, 0),
            Curve.Minimum(
                new RateLatencyServiceCurve(3, 3),
                new ConstantCurve(12)
            ),
            new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    Segment.Zero(0, 1),
                    new Point(1, 0),
                    new Segment(1, 2, 0, 1),
                    new Point(2, 1),
                    Segment.Constant(2, 3, 2),
                    new Point(3, 2),
                    Segment.Constant(3, 4, 2)
                }),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            )
        };

        foreach (var curve in testcases)
        {
            yield return new object[] { curve };
        }
    }

    [Theory]
    [MemberData(nameof(UcPropertiesTestCases))]
    public void UcProperties(Curve curve)
    {
        Assert.True(curve.IsUltimatelyConstant);
        var t_c = curve.PseudoPeriodStartInfimum;
        var f_t_c = curve.ValueAt(t_c);
        var c = curve.ValueAt(curve.PseudoPeriodStart);

        var upi = curve.UpperPseudoInverse();
        if(f_t_c < c)
            Assert.Equal(t_c, upi.RightLimitAt(f_t_c));
        Assert.Equal(t_c, upi.LeftLimitAt(c));
        Assert.Equal(Rational.PlusInfinity, upi.ValueAt(c));
        Assert.Equal(Rational.PlusInfinity, upi.RightLimitAt(c));
        Assert.True(upi.IsUltimatelyInfinite);
    }

    public static IEnumerable<object[]> UiPropertiesTestCases()
    {
        var testcases = new Curve[]
        {
            new DelayServiceCurve(4),
            new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    Segment.Zero(0, 1),
                    new Point(1, 0),
                    new Segment(1, 2, 0, 1),
                    new Point(2, 1),
                    Segment.PlusInfinite(2, 3),
                    Point.PlusInfinite(3),
                    Segment.PlusInfinite(3, 4)
                }),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            )
        };

        foreach (var curve in testcases)
        {
            yield return new object[] { curve };
        }
    }

    [Theory]
    [MemberData(nameof(UiPropertiesTestCases))]
    public void UiProperties(Curve curve)
    {
        Assert.True(curve.IsUltimatelyInfinite);
        var t_i = curve.PseudoPeriodStartInfimum;
        var f_t_i = curve.ValueAt(t_i);
        var l = (f_t_i.IsFinite) ? f_t_i :
            (t_i > 0) ? curve.LeftLimitAt(t_i) : 0;

        var upi = curve.UpperPseudoInverse();
        Assert.Equal(t_i, upi.ValueAt(l));
        Assert.Equal(t_i, upi.RightLimitAt(l));
        Assert.True(upi.IsUltimatelyConstant);
    }

    public static IEnumerable<object[]> NegativeTestCases()
    {
        var testcases = new (Curve operand, Curve expected)[]
        {
            (
                operand: new Curve(
                    baseSequence: new Sequence(
                        new Element[]
                        {
                            new Point(0, new Rational(-11, 4)),
                            new Segment(startTime: 0, 3, new Rational(-11, 4), new Rational(1, 4)),
                            new Point(3, -2),
                            new Segment(3, 4, -2, new Rational(1, 2))
                        }
                    ),
                    pseudoPeriodStart: 3,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: new Rational(1, 2)
                ),
                expected: new Curve(
                    baseSequence: new Sequence(
                        new Element[]
                        {
                            new Point(0, 7),
                            new Segment(startTime: 0, 1, 7, 2)
                        }
                    ),
                    pseudoPeriodStart: 0,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 2
                )
            )
        };

        foreach (var (operand, expected) in testcases)
        {
            yield return new object[] { operand, expected };
        }
    }

    [Theory]
    [MemberData(nameof(NegativeTestCases))]
    public void NegativeCases(Curve operand, Curve expected)
    {
        var result = operand.UpperPseudoInverse();
        Assert.True(result.IsRightContinuous);
        Assert.True(Curve.Equivalent(expected, result));
    }
}