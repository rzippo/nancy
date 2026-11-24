using System.Collections.Generic;
using System.Numerics;

namespace Unipi.Nancy.Tests.Numerics;

/// <summary>
/// Reduced fractions p/q and r/s whose quotients reach each tier of the division and its boundaries.
/// </summary>
public static class DivisionTierCases
{
    /// <summary>
    /// Each multiplication tier case a/b times c/d as a/b divided by the reciprocal of c/d, in both orders and with every combination of numerator signs, with the quotient reduced independently.
    /// </summary>
    /// <remarks>
    /// The division multiplies by the reciprocal of its divisor, so each case reaches the tier and the boundary of the product it comes from.
    /// </remarks>
    public static IEnumerable<object[]> Values()
    {
        foreach (var (a, b, c, d) in MultiplicationTierCases.Fractions())
        {
            foreach (var (xs, ys) in new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) })
            {
                yield return Quotient(xs * a, b, ys * c, d);
                yield return Quotient(ys * c, d, xs * a, b);
            }
        }
    }

    /// <summary>
    /// p/q divided by the reciprocal of c/d, which is ±d/|c|, with the quotient p·|c| / (q·d), its sign that of p·c, reduced.
    /// </summary>
    static object[] Quotient(BigInteger p, BigInteger q, BigInteger c, BigInteger d)
    {
        var r = c.Sign * d;
        var s = BigInteger.Abs(c);
        var numerator = p * s * r.Sign;
        var denominator = q * BigInteger.Abs(r);
        var divisor = BigInteger.GreatestCommonDivisor(numerator, denominator);
        return new object[] { p, q, r, s, numerator / divisor, denominator / divisor };
    }
}
