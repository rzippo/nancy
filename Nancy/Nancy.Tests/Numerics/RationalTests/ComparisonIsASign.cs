using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

/// <summary>
/// Pins the guarantee documented on <see cref="Rational.Compare"/> and its <c>CompareTo</c> alternatives:
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
        var values = new Rational[]
        {
            Rational.Zero,
            Rational.One,
            -Rational.One,
            new Rational(1, 2),
            new Rational(-1, 2),
            new Rational(1, 3),
            new Rational(2, 3),
            new Rational(271616, 11),          // the multiplication fallback path
            new Rational(30734848, 1243),
            new Rational(1388768, 113),
            12292,
            Rational.PlusInfinity,
            Rational.MinusInfinity,
        };

        return from a in values from b in values select new object[] { a, b };
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void CompareReturnsASign(Rational a, Rational b)
    {
        Assert.Contains(Rational.Compare(a, b), new[] { -1, 0, 1 });
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void CompareToReturnsASign(Rational a, Rational b)
    {
        Assert.Contains(a.CompareTo(b), new[] { -1, 0, 1 });
        Assert.Contains(((IComparable)a).CompareTo(b), new[] { -1, 0, 1 });
    }

    [Theory]
    [MemberData(nameof(Pairs))]
    public void TheSignStillAgreesWithTheOrdering(Rational a, Rational b)
    {
        // the pinning must not have changed which way the comparison goes
        var sign = Rational.Compare(a, b);
        Assert.Equal(a < b, sign < 0);
        Assert.Equal(a > b, sign > 0);
        Assert.Equal(a == b, sign == 0);
    }
}
