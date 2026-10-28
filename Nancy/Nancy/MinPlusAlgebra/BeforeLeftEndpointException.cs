using System;

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>A cursor lookup precedes the left endpoint of the support, accounting for its inclusion and the requested side.</summary>
public sealed class BeforeLeftEndpointException : ArgumentException
{
    /// <summary>Initializes an exception for a lookup before the left endpoint.</summary>
    public BeforeLeftEndpointException() : base("The request precedes the left endpoint of the support.") { }
}
