using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class RoundingTests
{
    public static IEnumerable<object[]> FloorTestCases()
    {
        var testCases = new (Rational Value, long Expected)[]
        {
            (new Rational(3, 4), 0),
            (new Rational(7, 4), 1),
            (new Rational(13, 4), 3),
            (new Rational(4, 4), 1),
            (new Rational(0, 4), 0),
            (new Rational(-3, 4), -1),
            (new Rational(-7, 4), -2),
            (new Rational(-13, 4), -4),
            (new Rational(-4, 4), -1)
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.Value, testCase.Expected };
    }

    [Theory]
    [MemberData(nameof(FloorTestCases))]
    public void Floor(Rational value, long expected)
    {
        var floor = value.Floor();
        Assert.Equal(expected, floor);
    }

    public static IEnumerable<object[]> CeilTestCases()
    {
        var testCases = new (Rational Value, long Expected)[]
        {
            (new Rational(3, 4), 1),
            (new Rational(7, 4), 2),
            (new Rational(13, 4), 4),
            (new Rational(4, 4), 1),
            (new Rational(0, 4), 0),
            (new Rational(-3, 4), 0),
            (new Rational(-7, 4), -1),
            (new Rational(-13, 4), -3),
            (new Rational(-4, 4), -1)
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.Value, testCase.Expected };
    }

    [Theory]
    [MemberData(nameof(CeilTestCases))]
    public void Ceil(Rational value, long expected)
    {
        var ceil = value.Ceil();
        Assert.Equal(expected, ceil);
    }
}