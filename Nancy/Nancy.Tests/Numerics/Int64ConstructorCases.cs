using System.Collections.Generic;

namespace Unipi.Nancy.Tests.Numerics;

internal static class Int64ConstructorCases
{
    public static IEnumerable<(long numerator, long denominator)> SignedPairs()
    {
        yield return (0, 1);
        yield return (0, -13);
        yield return (0, long.MinValue);
        yield return (7, 0);
        yield return (-7, 0);
        yield return (12, 18);
        yield return (-12, 18);
        yield return (12, -18);
        yield return (-12, -18);
        yield return (42, 1);
        yield return (long.MinValue, 1);
        yield return (long.MinValue, 2);
        yield return (long.MinValue, long.MinValue);
        yield return (long.MinValue, -1);
        yield return (1, long.MinValue);
        yield return (2, long.MinValue);
        yield return (long.MaxValue, long.MaxValue);
        yield return (long.MinValue + 1, long.MaxValue);
    }

    public static IEnumerable<(long numerator, long denominator)> CanonicalPairs()
    {
        yield return (0, 1);
        yield return (1, 0);
        yield return (-1, 0);
        yield return (1, 1);
        yield return (-1, 1);
        yield return (-7, 3);
        yield return (long.MinValue, 1);
        yield return (long.MaxValue, 1);
    }
}
