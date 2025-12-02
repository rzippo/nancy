using System;

namespace Unipi.Nancy.Numerics;

/// <summary>
/// Trying to use an empty interval.
/// </summary>
public class EmptyIntervalException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyIntervalException"/> class with the specified error message.
    /// </summary>
    public EmptyIntervalException(string message)
        : base(message)
    {}

    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyIntervalException"/> class with a default error message constructed with the given interval.
    /// </summary>
    public EmptyIntervalException(Interval interval)
        : base($"Interval {interval} is empty")
    {}
}