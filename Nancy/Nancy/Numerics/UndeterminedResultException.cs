using System;
using System.Runtime.Serialization;

namespace Unipi.Nancy.Numerics;

/// <summary>
/// Used for undetermined operations with <see cref="Rational"/>, e.g. $\infty - \infty$ 
/// </summary>
public class UndeterminedResultException : Exception
{
    /// <inheritdoc />
    public UndeterminedResultException()
    {
    }

    /// <inheritdoc />
    public UndeterminedResultException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public UndeterminedResultException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected UndeterminedResultException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}