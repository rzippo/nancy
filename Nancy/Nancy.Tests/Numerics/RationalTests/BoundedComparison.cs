using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

#if BIG_RATIONAL
public class BoundedComparison
{
    public static IEnumerable<object[]> Cases()
        => BoundedComparisonCases.Operands().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void ComparisonMatchesExactCrossProducts(
        BigInteger leftNumerator, BigInteger leftDenominator,
        BigInteger rightNumerator, BigInteger rightDenominator)
    {
        var left = new Rational(leftNumerator, leftDenominator);
        var right = new Rational(rightNumerator, rightDenominator);

        // Constructor normalization must preserve the intended boundary operands.
        Assert.Equal(leftNumerator, left.Numerator);
        Assert.Equal(leftDenominator, left.Denominator);
        Assert.Equal(rightNumerator, right.Numerator);
        Assert.Equal(rightDenominator, right.Denominator);
        Assert.NotEqual(left.Denominator, right.Denominator);

        var expected = (leftNumerator * rightDenominator).CompareTo(
            rightNumerator * leftDenominator);
        Assert.Equal(System.Math.Sign(expected), Rational.Compare(left, right));
    }
}
#endif
