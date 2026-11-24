using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

#if BIG_RATIONAL
public class BoundedAddition
{
    public static IEnumerable<object[]> Cases()
        => BoundedAdditionCases.Operands().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void AdditionReturnsTheReducedExactSum(
        BigInteger leftNumerator, BigInteger leftDenominator,
        BigInteger rightNumerator, BigInteger rightDenominator)
    {
        var left = new Rational(leftNumerator, leftDenominator);
        var right = new Rational(rightNumerator, rightDenominator);

        // Ensure constructor normalization has not changed the intended boundary operands.
        Assert.Equal(leftNumerator, left.Numerator);
        Assert.Equal(leftDenominator, left.Denominator);
        Assert.Equal(rightNumerator, right.Numerator);
        Assert.Equal(rightDenominator, right.Denominator);

        var expectedNumerator = leftNumerator * rightDenominator + rightNumerator * leftDenominator;
        var expectedDenominator = leftDenominator * rightDenominator;
        var divisor = BigInteger.GreatestCommonDivisor(expectedNumerator, expectedDenominator);
        expectedNumerator /= divisor;
        expectedDenominator /= divisor;

        var actual = left + right;

        Assert.Equal(expectedNumerator, actual.Numerator);
        Assert.Equal(expectedDenominator, actual.Denominator);
    }
}
#endif
