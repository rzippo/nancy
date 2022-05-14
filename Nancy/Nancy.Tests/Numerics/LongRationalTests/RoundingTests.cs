using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class RoundingTests
{
    public static IEnumerable<object[]> FloorTestCases()
    {
        var testCases = new (LongRational Value, long Expected)[]
        {
            (new LongRational(3, 4), 0),
            (new LongRational(7, 4), 1),
            (new LongRational(13, 4), 3),
            (new LongRational(4, 4), 1),
            (new LongRational(0, 4), 0),
            (new LongRational(-3, 4), -1),
            (new LongRational(-7, 4), -2),
            (new LongRational(-13, 4), -4),
            (new LongRational(-4, 4), -1)
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.Value, testCase.Expected };
    }

    [Theory]
    [MemberData(nameof(FloorTestCases))]
    public void Floor(LongRational value, long expected)
    {
        long floor = value.Floor();
        Assert.Equal(expected, floor);
    }

    public static IEnumerable<object[]> CeilTestCases()
    {
        var testCases = new (LongRational Value, long Expected)[]
        {
            (new LongRational(3, 4), 1),
            (new LongRational(7, 4), 2),
            (new LongRational(13, 4), 4),
            (new LongRational(4, 4), 1),
            (new LongRational(0, 4), 0),
            (new LongRational(-3, 4), 0),
            (new LongRational(-7, 4), -1),
            (new LongRational(-13, 4), -3),
            (new LongRational(-4, 4), -1)
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.Value, testCase.Expected };
    }

    [Theory]
    [MemberData(nameof(CeilTestCases))]
    public void Ceil(LongRational value, long expected)
    {
        long ceil = value.Ceil();
        Assert.Equal(expected, ceil);
    }
}