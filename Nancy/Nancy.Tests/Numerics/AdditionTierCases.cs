using System.Collections.Generic;
using System.Numerics;

namespace Unipi.Nancy.Tests.Numerics;

/// <summary>
/// Reduced fractions a/b and c/d whose sums and comparisons reach each addition and comparison tier and its boundaries.
/// </summary>
public static class AdditionTierCases
{
    static BigInteger P(int k) => BigInteger.One << k;

    public static IEnumerable<(BigInteger A, BigInteger B, BigInteger C, BigInteger D)> Fractions()
    {
        // Int32 integers, sums at the Int32 edge, and Int32.MinValue, which BigInteger keeps in an array.
        yield return (6, 1, 35, 1);
        yield return (int.MaxValue, 1, int.MaxValue, 1);
        yield return (int.MaxValue, 1, 1, 1);
        yield return (int.MinValue, 1, int.MaxValue, 1);
        yield return (int.MinValue, 1, int.MinValue, 1);
        // Int32 equal denominators: a sum cancelling with the denominator, one not cancelling, and one reaching zero.
        yield return (1, 6, 1, 6);
        yield return (1, 7, 2, 7);
        yield return (3, 10, 3, 10);
        yield return (int.MaxValue, int.MaxValue - 1, int.MaxValue, int.MaxValue - 1);
        // An Int32 integer and a fraction, either side, at the Int32 edge.
        yield return (5, 1, 3, 7);
        yield return (int.MaxValue, 1, int.MaxValue - 1, int.MaxValue);
        yield return (2, 3, int.MinValue + 1, 1);
        // Int32 fractions: coprime denominators, a shared factor with and without a second cancellation, and products at the Int64 edge.
        yield return (1, 6, 1, 35);
        yield return (1, 6, 1, 10);
        yield return (1, 6, 1, 3);
        yield return (5, 12, 7, 18);
        yield return (int.MaxValue, int.MaxValue - 1, int.MaxValue - 1, int.MaxValue);
        yield return (int.MaxValue, 2 * 3 * 5 * 7 * 11 * 13 * 17 * 19 * 23, int.MaxValue - 6, 2 * 3 * 5 * 7 * 11 * 13 * 17 * 19 * 29);
        // A component of magnitude 2^31, Int32.MinValue's, leaves the Int32 tier.
        yield return (int.MinValue, 3, 9, 4);
        yield return (3, P(31), P(31) - 1, 5);
        // Int64 integers: sums beyond Int64 in both signs, and Int64.MinValue, which needs more than 63 bits.
        yield return (P(40) + 1, 1, 3, 1);
        yield return (long.MaxValue, 1, long.MaxValue, 1);
        yield return (long.MaxValue, 1, 1, 1);
        yield return (long.MinValue + 1, 1, long.MinValue + 1, 1);
        yield return (long.MinValue, 1, 3, 1);
        // Int64 equal denominators: cancelling, not cancelling, a sum beyond Int64, and zero.
        yield return (P(40) + 1, P(41) + 2 + 1, P(40) + 3, P(41) + 2 + 1);
        yield return (P(50) + 1, 7, P(50) + 5, 7);
        yield return (long.MaxValue, P(40) + 1, long.MaxValue - 2, P(40) + 1);
        yield return (P(40) + 1, 3, P(40) + 1, 3);
        // An Int64 integer and a fraction: products beyond Int64.
        yield return (long.MaxValue, 1, 1, P(40) + 1);
        yield return (3, P(62) + 1, long.MaxValue - 1, 1);
        // Int64 fractions: coprime denominators, a shared factor with and without a second cancellation, and products beyond 64 bits.
        yield return (P(40) + 1, P(30) + 1, P(41) + 5, P(30) - 1);
        yield return (1, 6 * (P(40) + 1), 1, 10 * (P(40) + 1));
        yield return (1, 6 * (P(40) + 1), 1, 3 * (P(40) + 1));
        yield return (long.MaxValue, long.MaxValue - 1, long.MaxValue - 1, long.MaxValue);
        // Components at the 63-bit edge, and beyond, which go to BigInteger.
        yield return (long.MaxValue, 2, 3, long.MaxValue - 2);
        yield return (7, P(63) + 1, P(63) + 1, 5);
        yield return (7, P(63), 3, P(63));
        // BigInteger fractions: equal denominators, a shared factor, coprime, and a small operand against a wide one.
        yield return (5, P(100) + 1, 7, P(100) + 1);
        yield return (P(100) + 1, 3 * (P(90) + 7), 1, 5 * (P(90) + 7));
        yield return (P(100) + 1, P(100) + 3, 2, 7);
        yield return (1, 3, P(200) + 1, P(150) + 1);
    }

    static (BigInteger, BigInteger) Reduced(BigInteger n, BigInteger d)
    {
        if (n.IsZero)
            return (0, 1);
        var g = BigInteger.GreatestCommonDivisor(n, d);
        return (n / g, d / g);
    }

    /// <summary>
    /// Each case in both orders and with every combination of numerator signs, with the sum, the difference and the comparison computed independently.
    /// </summary>
    public static IEnumerable<object[]> Values()
    {
        foreach (var (a, b, c, d) in Fractions())
        {
            foreach (var (xs, ys) in new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) })
            {
                foreach (var (p, q, r, s) in new[] { (xs * a, b, ys * c, d), (ys * c, d, xs * a, b) })
                {
                    var (sumNumerator, sumDenominator) = Reduced(p * s + r * q, q * s);
                    var (differenceNumerator, differenceDenominator) = Reduced(p * s - r * q, q * s);
                    int comparison = (p * s).CompareTo(r * q);
                    yield return new object[] { p, q, r, s, sumNumerator, sumDenominator, differenceNumerator, differenceDenominator, comparison };
                }
            }
        }
    }
}
