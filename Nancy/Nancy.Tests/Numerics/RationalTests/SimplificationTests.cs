using System;
using System.Collections.Generic;
using System.Text;

using Unipi.Nancy.Numerics;

using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class SimplificationTests
{
    public static IEnumerable<object[]> GetTestCases()
    {
        var testCases = new (long numerator, long denominator, long expectedNumerator, long expectedDenominator)[]
        {
            (
                numerator: 290928896,
                denominator: 140459,
                expectedNumerator: 22784,
                expectedDenominator: 11
            ),
            (
                numerator: -290928896,
                denominator: 140459,
                expectedNumerator: -22784,
                expectedDenominator: 11
            ),
            (
                numerator: 290928896,
                denominator: -140459,
                expectedNumerator: -22784,
                expectedDenominator: 11
            )
        };

        foreach (var testCase in testCases)
            yield return new object[]
            {
                testCase.numerator,
                testCase.denominator,
                testCase.expectedNumerator,
                testCase.expectedDenominator
            };
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Simplify(long numerator, long denominator, long expectedNumerator, long expectedDenominator)
    {
        var rational = new Rational(numerator, denominator);

        Assert.Equal(expectedNumerator, rational.Numerator);
        Assert.Equal(expectedDenominator, rational.Denominator);
    }
}