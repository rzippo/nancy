namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// Interface for classes having the ToMppgString() method.
/// </summary>
public interface IToMppgString
{
    /// <summary>
    /// Return a string containing code to create an equivalent of this object in a (min,+) playground.
    /// </summary>
    public string ToMppgString();
}