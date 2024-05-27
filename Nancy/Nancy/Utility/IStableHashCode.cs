namespace Unipi.Nancy.Utility;

/// <summary>
/// Interface that provides a stable hash code, useful for identification and caching.
/// By "stable" we mean that the same object, constructed with the same values, will produce the same hash code
/// across different processes or executions.
/// By constrast, <see cref="object.GetHashCode"/> is not stable.
/// </summary>
public interface IStableHashCode
{
    /// <summary>
    /// A stable hash code. 
    /// </summary>
    public int GetStableHashCode();
}