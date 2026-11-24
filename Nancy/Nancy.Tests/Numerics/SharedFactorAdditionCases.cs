using System.Collections.Generic;
using System.Numerics;

namespace Unipi.Nancy.Tests.Numerics;

public static class SharedFactorAdditionCases
{
    public static IEnumerable<object[]> Values()
    {
        foreach (var width in new[] { 40, 256 })
        {
            var power = BigInteger.One << width;
            foreach (var sign in new[] { -1, 1 })
            {
                yield return new object[] { sign, 6 * (power + 1), sign, 6 * (power + 3), 6 };
                yield return new object[] { sign, 6 * (power + 1), 5 * sign, 6 * (power + 3), 2 };
                yield return new object[] { sign, 6 * power, sign, 6 * (power + 1), 3 };
                yield return new object[] { sign, 6 * power, 5 * sign, 6 * (power + 1), 1 };
                yield return new object[] { sign, 15 * power, sign, 21 * power, 12 };
                yield return new object[] { sign, 6 * (power + 1), -sign, 6 * (power + 3), 2 };
                yield return new object[] { sign, 6 * (power + 1), -5 * sign, 6 * (power + 3), 6 };
            }
        }
    }
}
