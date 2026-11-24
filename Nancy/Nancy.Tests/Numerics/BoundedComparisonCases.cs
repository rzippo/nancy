using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Unipi.Nancy.Tests.Numerics;

internal static class BoundedComparisonCases
{
    public static IEnumerable<(BigInteger LeftNumerator, BigInteger LeftDenominator,
        BigInteger RightNumerator, BigInteger RightDenominator)> Operands()
        => SignedOperands().Distinct();

    private static IEnumerable<(BigInteger, BigInteger, BigInteger, BigInteger)> SignedOperands()
    {
        foreach (var (a, b, c, d) in PositiveOperands())
        foreach (var sign in new[] { -1, 1 })
        {
            yield return (sign * a, b, sign * c, d);
            yield return (sign * c, d, sign * a, b);
        }
    }

    private static IEnumerable<(BigInteger, BigInteger, BigInteger, BigInteger)> PositiveOperands()
    {
        var p31 = BigInteger.One << 31;
        var p32 = BigInteger.One << 32;
        var p63 = BigInteger.One << 63;
        var p64 = BigInteger.One << 64;

        // Close fractions exercise products around the signed Int64 boundary.
        yield return (p31 - 1, p31 - 2, p31 - 3, p31 - 4);
        yield return (p31, p31 - 1, p31 - 2, p31 - 3);
        yield return (p32 - 1, p31 - 4, p32 - 3, p31 - 6);
        yield return (p32 - 1, p31 - 4, p32 - 3, p32 - 4);
        yield return (p32 - 1, p32 - 2, p32 - 3, p32 - 4);

        // Individual components also exceed signed Int64, without a large ratio gap.
        yield return (p63, p63 - 1, p63 - 2, p63 - 3);
        yield return (p64, p64 - 1, p64 - 2, p64 - 3);

        // Odd numerators keep these powers-of-two denominators canonical.
        // Nearby powers straddle both directions of the logarithmic separation bound.
        foreach (var exponent in new[] { 5, 31, 128 })
        foreach (var gap in new[] { 0, 1, 2, 3, 4 })
        foreach (var offset in new[] { -1, 1 })
            yield return ((BigInteger.One << exponent) + offset, 2,
                (BigInteger.One << (exponent + gap)) - offset, 8);
    }
}
