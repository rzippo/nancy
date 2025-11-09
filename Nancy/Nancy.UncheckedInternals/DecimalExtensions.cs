using System.Numerics;
using System.Text;

namespace Unipi.Nancy.UncheckedInternals;

public static class DecimalExtensions
{
    private const int DecimalScaleMask = 0x00FF0000;
    private const int DecimalSignMask = unchecked((int)0x80000000);
    private const int DecimalMaxScale = 28;

    /// <inheritdoc cref="decimal.GetBits(decimal)"/>
    public static int[] GetSignedBits(this decimal d)
    {
        return Decimal.GetBits(d);
    }
    
    /// <summary>
    /// Similar to <see cref="GetSignedBits"/>, but the returned array is unsigned.
    /// </summary>
    public static uint[] GetUnsignedBits(this decimal d)
    {
        var sbits = Decimal.GetBits(d);
        var ubits = sbits.Select(b => (uint)b).ToArray();
        return ubits;
    }

    /// <summary>
    /// Returns an equivalent numerator/denominator representation of this decimal.
    /// </summary>
    /// <remarks>
    /// Moved out of Rational code to avoid overflow exceptions due to decimal operations based on signed bits representations.
    /// </remarks>
    public static (BigInteger numerator, BigInteger denominator) GetRationalParts(this decimal d)
    {
        if (d == Decimal.Zero)
            return (BigInteger.Zero, BigInteger.One);

        int[] bits = Decimal.GetBits(d);
        if (bits == null || bits.Length != 4 || (bits[3] & ~(DecimalSignMask | DecimalScaleMask)) != 0 || (bits[3] & DecimalScaleMask) > (28 << 16))
        {
            throw new ArgumentException("invalid Decimal", nameof(d));
        }

        // build up the numerator
        ulong ul = (((ulong)(uint)bits[2]) << 32) | ((ulong)(uint)bits[1]);   // (hi    << 32) | (mid)
        var numerator = (new BigInteger(ul) << 32) | unchecked((uint)bits[0]);             // (hiMid << 32) | (low)

        bool isNegative = (bits[3] & DecimalSignMask) != 0;
        if (isNegative)
        {
            numerator = -numerator;
        }

        // build up the denominator
        int scale = (bits[3] & DecimalScaleMask) >> 16;     // 0-28, power of 10 to divide numerator by
        var denominator = BigInteger.Pow(10, scale);
        
        return (numerator, denominator);
    }
}