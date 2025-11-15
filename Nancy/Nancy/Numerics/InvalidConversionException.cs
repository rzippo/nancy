using System;

namespace Unipi.Nancy.Numerics;

/// <summary>
/// Used for invalid conversions, e.g. from Rational.PlusInfinity to decimal.
/// </summary>
public class InvalidConversionException : Exception
{
    /// <inheritdoc />
    public InvalidConversionException()
    {
    }

    /// <inheritdoc />
    public InvalidConversionException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public InvalidConversionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}