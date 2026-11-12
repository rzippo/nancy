using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;
namespace Unipi.Nancy.Tests.Numerics;

public class IntervalPairwiseSum
{
    public static IEnumerable<object[]> FiniteEndpoints()
    {
        for (int flags = 0; flags < 16; flags++)
            yield return [flags];
    }

    [Theory]
    [MemberData(nameof(FiniteEndpoints))]
    public void SumContainsExactlyThePairwiseSums(int flags)
    {
        var a = new Interval(-2, 1, (flags & 1) != 0, (flags & 2) != 0);
        var b = new Interval(3, 5, (flags & 4) != 0, (flags & 8) != 0);
        var result = Interval.PairwiseSum(a, b)!.Value;
        Assert.Equal((Rational)1, result.Lower);
        Assert.Equal((Rational)6, result.Upper);
        for (int twiceValue = 0; twiceValue <= 14; twiceValue++)
        {
            var value = new Rational(twiceValue, 2);
            bool exists = false;
            for (int quarterX = -8; quarterX <= 4; quarterX++)
            {
                var x = new Rational(quarterX, 4);
                exists |= a.Contains(x) && b.Contains(value - x);
            }
            Assert.Equal(exists, result.Contains(value));
        }
        Assert.Equal(result, Interval.PairwiseSum(b, a));
    }

    public static IEnumerable<object[]> OtherIntervals =>
    [
        new object[] { Interval.Open(1, 1), Interval.UnboundedAbove(0), null! },
        new object[] { Interval.Closed(2, 2), Interval.OpenClosed(3, 5), Interval.OpenClosed(5, 7) },
        new object[] { Interval.UnboundedBelow(2), Interval.UnboundedAbove(3), Interval.Open(Rational.MinusInfinity, Rational.PlusInfinity) },
        new object[] { Interval.Closed(0, Rational.PlusInfinity), Interval.Open(1, 2), new Interval(1, Rational.PlusInfinity, false, true) },
        new object[] { Interval.Closed(Rational.PlusInfinity, Rational.PlusInfinity), Interval.UnboundedBelow(0), Interval.Closed(Rational.PlusInfinity, Rational.PlusInfinity) },
        new object[] { Interval.Closed(Rational.MinusInfinity, Rational.MinusInfinity), Interval.UnboundedAbove(0), Interval.Closed(Rational.MinusInfinity, Rational.MinusInfinity) }
    ];

    [Theory]
    [MemberData(nameof(OtherIntervals))]
    public void SumHandlesEmptySingletonAndUnboundedIntervals(Interval a, Interval b, Interval? expected)
    {
        Assert.Equal(expected, Interval.PairwiseSum(a, b));
        Assert.Equal(expected, Interval.PairwiseSum(b, a));
    }

    [Fact]
    public void OppositeIncludedInfinitiesHaveNoSum()
    {
        Assert.Throws<ArgumentException>(() => Interval.PairwiseSum(
            Interval.Closed(Rational.MinusInfinity, 0), Interval.Closed(0, Rational.PlusInfinity)));
    }
}
