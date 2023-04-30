using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.CurvesOptimization;

public class PeriodFactorization
{
    public static IEnumerable<object[]> PeriodFactorizationTestCases()
    {
        var testCases = new (Curve factorizableCurve, int k)[]
        {
            buildTestCase(T: 54, d: 115, c: 30, k: 5),
            buildTestCase(T: 10, d: 10, c: 2, k: 34),
            buildTestCase(T: 14, d: 10, c: 2, k: 23),
            buildTestCase(T: 5, d: 15, c: 3, k: 64),
            buildTestCase(T: 54, d: 115, c: 23, k: 72),
            buildTestCase(T: 10, d: 5, c: 2, k: 14),
            buildTestCase(T: 14, d: 10, c: 2, k: 53),
            buildTestCase(T: 10, d: 5, c: 2, k: 241),
            (
                factorizableCurve: new Curve(
                    baseSequence: new StairCurve(3, 2).Cut(0, 2*10),
                    pseudoPeriodStart: 0,
                    pseudoPeriodLength: 2*10,
                    pseudoPeriodHeight: 3*10
                ),
                k: 10
            )
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.factorizableCurve, testCase.k };

        (Curve factorizableCurve, int k) buildTestCase(Rational T, Rational d, Rational c, int k)
        {
            var kPeriods = Enumerable.Range(0, k)
                .AsParallel()
                .AsOrdered()
                .Select(i => getRepeatedPeriod(i))
                .SelectMany(e => e);

            var kPeriodsSequence = new Sequence(kPeriods);
            var baseSequence = kPeriodsSequence.Delay(T);
            var curve = new Curve(
                baseSequence: baseSequence,
                pseudoPeriodStart: T,
                pseudoPeriodLength: d * k,
                pseudoPeriodHeight: c * k
            );

            return (factorizableCurve: curve, k: k);

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

    public static IEnumerable<object[]> PeriodFactorizationTestCases_Displaced()
    {
        foreach(var testcase in PeriodFactorizationTestCases())
        {
            var curve = (Curve)testcase[0];
            var k = (int)testcase[1];

            var lastSegmentLength = ((Segment) curve.BaseSequence.Elements.Last()).Length;
            var displacement = Rational.Min(lastSegmentLength, curve.PseudoPeriodStart) / 2;

            var displacedCurve = new Curve(
                baseSequence: curve.Cut(0, curve.FirstPseudoPeriodEnd - displacement),
                pseudoPeriodStart: curve.PseudoPeriodStart - displacement,
                pseudoPeriodLength: curve.PseudoPeriodLength,
                pseudoPeriodHeight: curve.PseudoPeriodHeight
            );

            yield return new object[] { displacedCurve, k };
        }
    }

    public static IEnumerable<object[]> PartiallyInfiniteTestCases()
    {
        var testCases = new (Curve factorizableCurve, int k)[]
        {
            buildTestCase( 
                new Sequence(new Element[]
                {
                    Point.Origin(),
                    Segment.PlusInfinite(0, 2)
                }),
                T: 0,d: 2,c: 30,k: 5
            ),
            buildTestCase( 
                new Sequence(new Element[]
                {
                    Point.Origin(),
                    Segment.PlusInfinite(0, 2)
                }),
                T: 54,d: 2,c: 30,k: 5
            ),
            buildTestCase( 
                new Sequence(new Element[]
                {
                    Point.PlusInfinite(0),
                    new Segment(0, 1, 1, 2),
                    Point.PlusInfinite(1),
                    new Segment(1, 3, 2, 2),
                    Point.PlusInfinite(3),
                    Segment.PlusInfinite(3, 4), 
                }),
                T: 1,d: 4,c: 6,k: 1
            ),
            buildTestCase( 
                new Sequence(new Element[]
                {
                    Point.PlusInfinite(0),
                    new Segment(0, 1, 1, 2),
                    Point.PlusInfinite(1),
                    new Segment(1, 3, 2, 2),
                    Point.PlusInfinite(3),
                    Segment.PlusInfinite(3, 4), 
                }),
                T: 1,d: 4,c: 6,k: 5
            ),
            buildTestCase( 
                new Sequence(new Element[]
                {
                    Point.PlusInfinite(0),
                    Segment.PlusInfinite(0, 1),
                    Point.PlusInfinite(1), 
                    new Segment(1, 2, 1, 2),
                    Point.PlusInfinite(2),
                    new Segment(2, 4, 2, 2),
                    Point.PlusInfinite(4),
                    Segment.PlusInfinite(4, 5), 
                }),
                T: 1,d: 5,c: 6,k: 1
            ),
            buildTestCase( 
                new Sequence(new Element[]
                {
                    Point.PlusInfinite(0),
                    Segment.PlusInfinite(0, 1),
                    Point.PlusInfinite(1), 
                    new Segment(1, 2, 1, 2),
                    Point.PlusInfinite(2),
                    new Segment(2, 4, 2, 2),
                    Point.PlusInfinite(4),
                    Segment.PlusInfinite(4, 5), 
                }),
                T: 1,d: 5,c: 6,k: 4
            )
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.factorizableCurve, testCase.k };

        (Curve factorizableCurve, int k) buildTestCase(Sequence s, Rational T, Rational c, Rational d, int k)
        {
            var kPeriods = Enumerable.Range(0, k)
                .AsParallel()
                .AsOrdered()
                .Select(i => s.VerticalShift(i*c, false).Delay(i*d, false))
                .SelectMany(s => s.Elements);

            var kPeriodsSequence = new Sequence(kPeriods);
            var baseSequence = kPeriodsSequence.Delay(T);
            var curve = new Curve(
                baseSequence: baseSequence,
                pseudoPeriodStart: T,
                pseudoPeriodLength: d * k,
                pseudoPeriodHeight: c * k
            );

            return (factorizableCurve: curve, k: k);
        }
    }

    [Theory]
    [MemberData(nameof(PeriodFactorizationTestCases))]
    [MemberData(nameof(PeriodFactorizationTestCases_Displaced))]
    [MemberData(nameof(PartiallyInfiniteTestCases))]
    public void PeriodFactorizationTest(Curve factorizableCurve, int k)
    {
        var factorizedCurve = factorizableCurve.PeriodFactorization();

        Assert.True(factorizedCurve.Equivalent(factorizableCurve));
        Assert.Equal(factorizableCurve.PseudoPeriodStart, factorizedCurve.PseudoPeriodStart);
        Assert.Equal(factorizableCurve.PseudoPeriodLength / k, factorizedCurve.PseudoPeriodLength);
        Assert.Equal(factorizableCurve.PseudoPeriodHeight / k, factorizedCurve.PseudoPeriodHeight);
    }
}