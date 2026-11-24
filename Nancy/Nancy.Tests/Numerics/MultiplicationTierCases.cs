using System.Collections.Generic;
using System.Numerics;

namespace Unipi.Nancy.Tests.Numerics;

/// <summary>
/// Reduced fractions a/b and c/d whose products reach each multiplication tier and its boundaries.
/// </summary>
public static class MultiplicationTierCases
{
    static BigInteger P(int k) => BigInteger.One << k;

    public static IEnumerable<(BigInteger A, BigInteger B, BigInteger C, BigInteger D)> Fractions()
    {
        // Integers: two Int32 values, products at the Int64 edge, and Int32.MinValue, which BigInteger keeps in an array.
        yield return (6, 1, 35, 1);
        yield return (int.MaxValue, 1, int.MaxValue, 1);
        yield return (int.MinValue, 1, int.MaxValue, 1);
        yield return (int.MinValue, 1, int.MinValue, 1);
        yield return (P(40) + 1, 1, 3, 1);
        // Units on either side, of every width.
        yield return (1, 1, 5, 7);
        yield return (-1, 1, 5, 7);
        yield return (1, 1, P(100) + 1, 3);
        yield return (-1, 1, int.MinValue, 1);
        // Int32 fractions: both cross-pairs cancelling, an equal cross-pair, equal denominators and equal numerator magnitudes.
        yield return (4, 9, 3, 8);
        yield return (7, 6, 6, 7);
        yield return (1, 7, 3, 7);
        yield return (3, 5, 3, 7);
        yield return (int.MaxValue, int.MaxValue - 1, int.MaxValue - 1, int.MaxValue);
        yield return (int.MaxValue, 2, 3, int.MaxValue - 2);
        // A component of magnitude 2^31, Int32.MinValue's, leaves the Int32 tier.
        yield return (int.MinValue, 3, 9, 4);
        yield return (3, P(31), P(31) - 1, 5);
        // ±1/b times an integer: below and beyond 63 bits, a hit and a miss.
        yield return (1, 6, P(40) * 3, 1);
        yield return (1, P(40) + 1, 5, 1);
        yield return (-1, 6, P(70) * 3, 1);
        yield return (1, P(70) + 1, 7, 1);
        // Int64 fractions: cross hits, an equal cross-pair, equal denominators, and products beyond 64 bits.
        yield return (P(40) + 1, P(20), P(19), 3 * (P(40) + 1));
        yield return (P(40) + 1, 3, 3, P(40) + 1);
        yield return (5, P(40) + 1, 7, P(40) + 1);
        yield return (P(62) + 1, 3, P(62) - 1, 5);
        // Products at the Int64 edges: a numerator of −2^63, and a denominator of 2^63, which does not fit.
        yield return (-P(32), 3, P(31), 5);
        yield return (3, P(32), 5, P(31));
        // Components at the 63-bit edge, and Int64.MinValue, which goes to BigInteger.
        yield return (long.MaxValue, 2, 3, long.MaxValue - 2);
        yield return (long.MinValue, 3, 9, 4);
        yield return (7, P(63) + 1, P(63) + 1, 5);
        // BigInteger fractions: cross hits, an equal cross-pair, equal denominators and equal numerator magnitudes.
        yield return ((P(100) + 1) * 3, P(90) + 7, (P(90) + 7) * 5, (P(100) + 1) * 11);
        yield return (P(100) + 1, 3, 3, P(100) + 1);
        yield return (5, P(100) + 1, 7, P(100) + 1);
        yield return (P(100) + 1, 5, P(100) + 1, 7);
        yield return (P(100) + 1, P(100) + 3, 2, 7);
    }

    /// <summary>
    /// Each case in both orders and with every combination of numerator signs, with the product reduced independently.
    /// </summary>
    public static IEnumerable<object[]> Values()
    {
        foreach (var (a, b, c, d) in Fractions())
        {
            foreach (var (xs, ys) in new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) })
            {
                var numerator = xs * a * ys * c;
                var denominator = b * d;
                var divisor = BigInteger.GreatestCommonDivisor(numerator, denominator);
                yield return new object[] { xs * a, b, ys * c, d, numerator / divisor, denominator / divisor };
                yield return new object[] { ys * c, d, xs * a, b, numerator / divisor, denominator / divisor };
            }
        }
    }
}
