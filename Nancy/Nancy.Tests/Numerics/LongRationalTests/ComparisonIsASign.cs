using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

/// <summary>
/// Pins the guarantee documented on <see cref="LongRational.Compare"/> and its <c>CompareTo</c> alternatives:
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
        var values = new LongRational[]
        {
            LongRational.Zero,
            LongRational.One,
            -LongRational.One,
            new LongRational(1, 2),
            new LongRational(-1, 2),
            new LongRational(1, 3),
            new LongRational(2, 3),
            new LongRational(271616, 11),          // the multiplication fallback path
            new LongRational(30734848, 1243),
            new LongRational(1388768, 113),
            12292,
            LongRational.PlusInfinity,
            LongRational.MinusInfinity,
        };

        return from a in values from b in values select new object[] { a, b };
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void CompareReturnsASign(LongRational a, LongRational b)
    {
        Assert.Contains(LongRational.Compare(a, b), new[] { -1, 0, 1 });
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void CompareToReturnsASign(LongRational a, LongRational b)
    {
        Assert.Contains(a.CompareTo(b), new[] { -1, 0, 1 });
        Assert.Contains(((IComparable)a).CompareTo(b), new[] { -1, 0, 1 });
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void TheSignStillAgreesWithTheOrdering(LongRational a, LongRational b)
    {
        // the pinning must not have changed which way the comparison goes
        var sign = LongRational.Compare(a, b);
        Assert.Equal(a < b, sign < 0);
        Assert.Equal(a > b, sign > 0);
        Assert.Equal(a == b, sign == 0);
    }
}
