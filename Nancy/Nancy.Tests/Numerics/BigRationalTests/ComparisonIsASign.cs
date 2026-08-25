using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

/// <summary>
/// Pins the guarantee documented on <see cref="BigRational.Compare"/> and its <c>CompareTo</c> alternatives:
/// the result is exactly $-1$, $0$ or $1$, not merely a value of the right sign.
/// </summary>
/// <remarks>
/// The interface contract alone allows any value of the right sign, and several return paths delegate
/// to a <c>CompareTo</c> that only promises that much. Callers reading the result as a sign depend on
/// the stronger promise, so it is checked here rather than assumed.
/// </remarks>
public class ComparisonIsASign
{
    public static IEnumerable<object[]> Pairs()
    {
        var values = new BigRational[]
        {
            BigRational.Zero,
            BigRational.One,
            -BigRational.One,
            new BigRational(1, 2),
            new BigRational(-1, 2),
            new BigRational(1, 3),
            new BigRational(2, 3),
            new BigRational(271616, 11),          // the multiplication fallback path
            new BigRational(30734848, 1243),
            new BigRational(1388768, 113),
            12292,
            BigRational.PlusInfinity,
            BigRational.MinusInfinity,
        };

        return from a in values from b in values select new object[] { a, b };
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void CompareReturnsASign(BigRational a, BigRational b)
    {
        Assert.Contains(BigRational.Compare(a, b), new[] { -1, 0, 1 });
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void CompareToReturnsASign(BigRational a, BigRational b)
    {
        Assert.Contains(a.CompareTo(b), new[] { -1, 0, 1 });
        Assert.Contains(((IComparable)a).CompareTo(b), new[] { -1, 0, 1 });
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void TheSignStillAgreesWithTheOrdering(BigRational a, BigRational b)
    {
        // the pinning must not have changed which way the comparison goes
        var sign = BigRational.Compare(a, b);
        Assert.Equal(a < b, sign < 0);
        Assert.Equal(a > b, sign > 0);
        Assert.Equal(a == b, sign == 0);
    }
}
