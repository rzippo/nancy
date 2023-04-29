using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.CurvesOptimization;

public class TransientReduction
{
    public static IEnumerable<object[]> TransientReductionByPeriodTestCases()
    {
        var testCases = new (Curve reducibleCurve, int k)[]
        {
            buildTestCase(T: 5400, d: 115, c: 30, k: 5),
            buildTestCase(T: 1000, d: 10, c: 2, k: 34),
            buildTestCase(T: 1400, d: 10, c: 2, k: 23),
            buildTestCase(T: 5000, d: 15, c: 3, k: 64),
            buildTestCase(T: 5400, d: 35, c: 23, k: 72),
            buildTestCase(T: 1000, d: 5, c: 2, k: 14)
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.reducibleCurve, testCase.k };

        (Curve reducibleCurve, int k) buildTestCase(Rational T, Rational d, Rational c, int k)
        {
            if (T < d * k)
                throw new ArgumentException("Invalid time parameters");

            var periods = Enumerable.Range(0, k + 1)
                .AsParallel()
                .AsOrdered()
                .Select(i => getRepeatedPeriod(i))
                .SelectMany(e => e);

            var periodsSequence = new Sequence(periods);
            var baseSequence = periodsSequence.Delay(T - d * k);
            var curve = new Curve(
                baseSequence: baseSequence,
                pseudoPeriodStart: T,
                pseudoPeriodLength: d,
                pseudoPeriodHeight: c
            );

            return (reducibleCurve: curve, k: k);

            Element[] getRepeatedPeriod(int i)
            {
                var startTime = i * d;
                var midTime = i * d + d / 2;
                var endTime = (i + 1) * d;

                var slope = c / (d / 2);

                var startValue = i * c;
                var endValue = (i + 1) * c;

                return new Element[]
                {
                    new Point(
                        time: startTime,
                        value: startValue
                    ),
                    new Segment(
                        startTime: startTime,
                        endTime: midTime,
                        rightLimitAtStartTime: startValue,
                        slope: 0
                    ),
                    new Point(
                        time: midTime,
                        value: endValue
                    ),
                    new Segment(
                        startTime: midTime,
                        endTime: endTime,
                        rightLimitAtStartTime: endValue,
                        slope: slope
                    )
                };
            }
        }
    }

    public static IEnumerable<object[]> TransientReductionBySegmentTestCases()
    {
        var testCases = new (Curve reducibleCurve, Rational expectedStart)[]
        {
            buildTestCase(T: 5400, d: 115, c: 30, k: 5),
            buildTestCase(T: 1000, d: 10, c: 2, k: 34),
            buildTestCase(T: 1400, d: 10, c: 2, k: 23),
            buildTestCase(T: 5000, d: 15, c: 3, k: 64),
            buildTestCase(T: 5400, d: 35, c: 23, k: 72),
            buildTestCase(T: 1000, d: 5, c: 2, k: 14),
            (
                reducibleCurve: new Curve(
                    baseSequence: new Sequence(new List<Element>
                    {
                        new Point(0, 0),
                        new Segment(0, 2, 0, 1),
                        new Point(2, 2),
                        new Segment(2, new Rational(7, 2), 2, 0), 
                        new Point(new Rational(7, 2), 2),
                        new Segment(new Rational(7, 2), new Rational(11, 2), 3, 0)
                    }), 
                    pseudoPeriodStart: new Rational(7, 2), 
                    pseudoPeriodLength: 2, 
                    pseudoPeriodHeight: 1
                ),
                expectedStart: 2
            )
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.reducibleCurve, testCase.expectedStart };

        (Curve reducibleCurve, Rational expectedStart) buildTestCase(Rational T, Rational d, Rational c, int k)
        {
            if (T < d * k)
                throw new ArgumentException("Invalid time parameters: T too short for by period reduction");

            var s = d / 2;
            if (T - d * k < s)
                throw new ArgumentException("Invalid time parameters: T too short for by segment reduction");

            var periods = Enumerable.Range(0, k + 1)
                .AsParallel()
                .AsOrdered()
                .Select(i => getRepeatedPeriod(i))
                .SelectMany(e => e);

            var periodsSequence = new Sequence(periods);
            var baseSequence = periodsSequence.Delay(T - d * k);
            var curve = new Curve(
                baseSequence: baseSequence,
                pseudoPeriodStart: T,
                pseudoPeriodLength: d,
                pseudoPeriodHeight: c
            );

            var expectedStart = curve.PseudoPeriodStart - curve.PseudoPeriodLength * k - s;
            return (reducibleCurve: curve, expectedStart: expectedStart);

            Element[] getRepeatedPeriod(int i)
            {
                var startTime = i * d;
                var midTime = i * d + d / 2;
                var endTime = (i + 1) * d;

                var slope = c / (d / 2);

                var startValue = i * c;
                var endValue = (i + 1) * c;

                return new Element[]
                {
                    new Point(
                        time: startTime,
                        value: startValue
                    ),
                    new Segment(
                        startTime: startTime,
                        endTime: midTime,
                        rightLimitAtStartTime: startValue,
                        slope: slope
                    ),
                    new Point(
                        time: midTime,
                        value: endValue
                    ),
                    new Segment(
                        startTime: midTime,
                        endTime: endTime,
                        rightLimitAtStartTime: endValue,
                        slope: 0
                    )
                };
            }
        }
    }

    [Theory]
    [MemberData(nameof(TransientReductionByPeriodTestCases))]
    public void TransientReductionByPeriodTest(Curve reducibleCurve, int k)
    {
        var reducedCurve = reducibleCurve.TransientReduction();

        Assert.True(reducedCurve.Equivalent(reducibleCurve));
        Assert.Equal(reducibleCurve.PseudoPeriodStart - reducibleCurve.PseudoPeriodLength * k, reducedCurve.PseudoPeriodStart);
        Assert.Equal(reducibleCurve.PseudoPeriodLength, reducedCurve.PseudoPeriodLength);
        Assert.Equal(reducibleCurve.PseudoPeriodHeight, reducedCurve.PseudoPeriodHeight);
    }

    [Theory]
    [MemberData(nameof(TransientReductionBySegmentTestCases))]
    public void TransientReductionBySegmentTest(Curve reducibleCurve, Rational expectedStart)
    {
        var reducedCurve = reducibleCurve.TransientReduction();

        Assert.True(reducedCurve.Equivalent(reducibleCurve));
        Assert.Equal(expectedStart, reducedCurve.PseudoPeriodStart);
        Assert.Equal(reducibleCurve.PseudoPeriodLength, reducedCurve.PseudoPeriodLength);
        Assert.Equal(reducibleCurve.PseudoPeriodHeight, reducedCurve.PseudoPeriodHeight);
    }

    public static IEnumerable<object[]> AffineTransientReductionTestCases()
    {
        var testcases = new List<(Curve curve, Rational expected)>
        {
            (
                curve: Curve.Minimum(
                    new RateLatencyServiceCurve(3, 3),
                    new SigmaRhoArrivalCurve(4, 1)
                ),
                expected: 6.5m
            )
        };

        foreach (var (curve, expected) in testcases)
            yield return new object[] { curve, expected };
    }

    [Theory]
    [MemberData(nameof(AffineTransientReductionTestCases))]
    public void AffineTransientReductionTest(Curve reducibleCurve, Rational expected)
    {
        var reducedCurve = reducibleCurve.TransientReduction();

        Assert.True(reducedCurve.Equivalent(reducibleCurve));
        Assert.Equal(expected, reducedCurve.PseudoPeriodStart);
    }
}