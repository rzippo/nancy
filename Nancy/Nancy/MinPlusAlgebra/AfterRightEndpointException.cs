using System;

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>A cursor lookup follows the right endpoint of the support, accounting for its inclusion and the requested side.</summary>
public sealed class AfterRightEndpointException : ArgumentException
{
    /// <summary>Initializes an exception for a lookup beyond the right endpoint.</summary>
    public AfterRightEndpointException() : base("The request follows the right endpoint of the support.") { }
}
