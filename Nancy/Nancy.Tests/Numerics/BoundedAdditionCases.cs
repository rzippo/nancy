using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace Unipi.Nancy.Tests.Numerics;

internal static class BoundedAdditionCases
{
    public static IEnumerable<(BigInteger LeftNumerator, BigInteger LeftDenominator,
        BigInteger RightNumerator, BigInteger RightDenominator)> Operands()
        => SignedOperands().Distinct();

    private static IEnumerable<(BigInteger LeftNumerator, BigInteger LeftDenominator,
        BigInteger RightNumerator, BigInteger RightDenominator)> SignedOperands()
    {
        var p30 = BigInteger.One << 30;
        var p31 = BigInteger.One << 31;
        var p32 = BigInteger.One << 32;
        var p62 = BigInteger.One << 62;
        var p128 = BigInteger.One << 128;

        var positivePairs = new (BigInteger A, BigInteger B, BigInteger C, BigInteger D)[]
        {
            (1, 4, 1, 6),
            (1, 2, 1, 2),
            (p31 - 1, p31 - 2, p31 - 1, p31 - 2),
            (p30 - 1, 1, 1, p31),
            (p30 + 1, 1, 1, p31),
            (p31 - 1, p31 - 2, p32 - 1, p32 - 2),
            (p62 - 1, 1, 1, 3),
            (1, p32, 1, p32 + 1),
            (1, p30, 1, p31 - 1),
            (1, p31, 1, p31 - 1),
            (1, p31, 1, p32 - 1),
            (p128, 3, p128 + 1, 5),
            (p128, 3, p128, 3),
            (1, p128, 3, p128),
            (p128 + 1, p128 + 3, p128 + 5, p128 + 3),
            (p128, 3, 1, 6),
        };

        foreach (var (a, b, c, d) in positivePairs)
        foreach (var leftSign in new[] { -1, 1 })
        foreach (var rightSign in new[] { -1, 1 })
        {
            yield return (leftSign * a, b, rightSign * c, d);
            yield return (rightSign * c, d, leftSign * a, b);
        }

        yield return (0, 1, 0, 1);
        yield return (0, 1, 7, 3);
        yield return (7, 3, 0, 1);
        yield return (0, 1, -p128, 3);
        yield return (-p128, 3, 0, 1);
    }
}
