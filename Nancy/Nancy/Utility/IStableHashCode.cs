namespace Unipi.Nancy.Utility;

/// <summary>
/// Interface that provides a stable hash code, useful for identification and caching.
/// </summary>
public interface IStableHashCode
{
    /// <summary>
    /// A stable hash code. 
    /// </summary>
    public int GetStableHashCode();
}