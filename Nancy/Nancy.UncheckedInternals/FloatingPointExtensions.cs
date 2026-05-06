using System;
using System.Globalization;
using System.Numerics;

namespace Unipi.Nancy.UncheckedInternals;

/// <summary>
/// Extension methods for floating-point rational-part extraction.
/// </summary>
public static class FloatingPointExtensions
{
    /// <summary>
    /// Returns a numerator/denominator representation of the shortest round-trippable decimal representation of this double.
    /// </summary>
    public static (BigInteger numerator, BigInteger denominator) GetRationalParts(this double value)
    {
        if (double.IsNaN(value))
            throw new ArgumentException("Argument is not a number", nameof(value));
        if (double.IsPositiveInfinity(value))
            return (BigInteger.One, BigInteger.Zero);
        if (double.IsNegativeInfinity(value))
            return (BigInteger.MinusOne, BigInteger.Zero);

        return GetRationalParts(value.ToString("G", CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Returns a numerator/denominator representation of the shortest round-trippable decimal representation of this float.
    /// </summary>
    public static (BigInteger numerator, BigInteger denominator) GetRationalParts(this float value)
    {
        if (float.IsNaN(value))
            throw new ArgumentException("Argument is not a number", nameof(value));
        if (float.IsPositiveInfinity(value))
            return (BigInteger.One, BigInteger.Zero);
        if (float.IsNegativeInfinity(value))
            return (BigInteger.MinusOne, BigInteger.Zero);

        return GetRationalParts(value.ToString("G", CultureInfo.InvariantCulture));
    }

    private static (BigInteger numerator, BigInteger denominator) GetRationalParts(string value)
    {
        var exponentSeparator = value.IndexOf('E');
        if (exponentSeparator < 0)
            exponentSeparator = value.IndexOf('e');

        var significandText = exponentSeparator < 0 ? value : value[..exponentSeparator];
        var exponent = exponentSeparator < 0
            ? 0
            : int.Parse(value[(exponentSeparator + 1)..], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);

        var isNegative = significandText.StartsWith("-", StringComparison.Ordinal);
        if (isNegative || significandText.StartsWith("+", StringComparison.Ordinal))
            significandText = significandText[1..];

        var decimalPoint = significandText.IndexOf('.');
        if (decimalPoint >= 0)
        {
            exponent -= significandText.Length - decimalPoint - 1;
            significandText = significandText.Remove(decimalPoint, 1);
        }

        var numerator = BigInteger.Parse(significandText, NumberStyles.None, CultureInfo.InvariantCulture);
        var denominator = BigInteger.One;
        if (exponent >= 0)
            numerator *= BigInteger.Pow(10, exponent);
        else
            denominator = BigInteger.Pow(10, -exponent);

        if (isNegative)
            numerator = -numerator;

        return (numerator, denominator);
    }
}
