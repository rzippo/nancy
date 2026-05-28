using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class FirstFiniteTime
{
    public static List<(Curve curve, Rational expectedFirstFiniteTime, Rational expectedFirstFiniteTimeExceptOrigin)> KnownCases =
        new()
        {
            (
                curve: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        new Point(0, 0),
                        new Segment(0, 5, 0, 2),
                        new Point(5, 10),
                        new Segment(5, 10, 10, 1),
                        new Point(10, 15)
                    }),
                    pseudoPeriodStart: 5,
                    pseudoPeriodLength: 5,
                    pseudoPeriodHeight: 3
                ),
                expectedFirstFiniteTime: 0,
                expectedFirstFiniteTimeExceptOrigin: 0
            ),
            (
                curve: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        new Point(0, Rational.PlusInfinity),
                        new Segment(0, 3, Rational.PlusInfinity, Rational.PlusInfinity),
                        new Point(3, 7),
                        new Segment(3, 8, 7, 2),
                        new Point(8, 17)
                    }),
                    pseudoPeriodStart: 3,
                    pseudoPeriodLength: 5,
                    pseudoPeriodHeight: 2
                ),
                expectedFirstFiniteTime: 3,
                expectedFirstFiniteTimeExceptOrigin: 3
            ),
            (
                curve: Curve.PlusInfinite(),
                expectedFirstFiniteTime: Rational.PlusInfinity,
                expectedFirstFiniteTimeExceptOrigin: Rational.PlusInfinity
            ),
            (
                curve: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        new Point(0, 5),
                        new Segment(0, 5, Rational.PlusInfinity, Rational.PlusInfinity)
                    }),
                    pseudoPeriodStart: 0,
                    pseudoPeriodLength: 5,
                    pseudoPeriodHeight: 1
                ),
                expectedFirstFiniteTime: 0,
                expectedFirstFiniteTimeExceptOrigin: 5
            ),
            (
                curve: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        new Point(0, 0),
                        new Segment(0, 5, Rational.PlusInfinity, Rational.PlusInfinity)
                    }),
                    pseudoPeriodStart: 0,
                    pseudoPeriodLength: 5,
                    pseudoPeriodHeight: 1
                ),
                expectedFirstFiniteTime: 0,
                expectedFirstFiniteTimeExceptOrigin: 5
            ),
            (
                curve: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        new Point(0, Rational.PlusInfinity),
                        new Segment(0, 2, Rational.PlusInfinity, Rational.PlusInfinity),
                        new Point(2, 4),
                        new Segment(2, 7, 4, 1),
                        new Point(7, 9)
                    }),
                    pseudoPeriodStart: 3,
                    pseudoPeriodLength: 4,
                    pseudoPeriodHeight: 2
                ),
                expectedFirstFiniteTime: 2,
                expectedFirstFiniteTimeExceptOrigin: 2
            ),
            (
                curve: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        new Point(0, 0),
                        new Segment(0, 2, Rational.PlusInfinity, Rational.PlusInfinity),
                        new Point(2, 4),
                        new Segment(2, 7, 4, 1),
                        new Point(7, 9)
                    }),
                    pseudoPeriodStart: 3,
                    pseudoPeriodLength: 4,
                    pseudoPeriodHeight: 2
                ),
                expectedFirstFiniteTime: 0,
                expectedFirstFiniteTimeExceptOrigin: 2
            ),
        };

    #pragma warning disable xUnit1026
    public static IEnumerable<object[]> KnownCasesTestCases =
        KnownCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(KnownCasesTestCases))]
    public void FirstFiniteTimeTest(Curve curve, Rational expectedFirstFiniteTime, Rational expectedFirstFiniteTimeExceptOrigin)
    {
        Assert.Equal(expectedFirstFiniteTime, curve.FirstFiniteTime);
        
        if(curve.FirstFiniteTime.IsFinite) {
            var valueAtFFT = curve.ValueAt(curve.FirstFiniteTime);
            var valueAfterFFT = curve.RightLimitAt(curve.FirstFiniteTime);
            Assert.True(valueAtFFT.IsFinite || valueAfterFFT.IsFinite);
        }
    }

    [Theory]
    [MemberData(nameof(KnownCasesTestCases))]
    public void expectedFirstFiniteTimeExceptOriginTest(Curve curve, Rational expectedFirstFiniteTime, Rational expectedFirstFiniteTimeExceptOrigin)
    {
        Assert.Equal(expectedFirstFiniteTimeExceptOrigin, curve.FirstFiniteTimeExceptOrigin);

        if(curve.FirstFiniteTimeExceptOrigin.IsFinite) {
            var valueAtFFTEO = curve.ValueAt(curve.FirstFiniteTimeExceptOrigin);
            var valueAfterFFTEO = curve.RightLimitAt(curve.FirstFiniteTimeExceptOrigin);
            if(curve.FirstFiniteTimeExceptOrigin == 0)
                Assert.True(valueAfterFFTEO.IsFinite);
            else
                Assert.True(valueAtFFTEO.IsFinite || valueAfterFFTEO.IsFinite);
        }
    }
    #pragma warning restore xUnit1026
}
