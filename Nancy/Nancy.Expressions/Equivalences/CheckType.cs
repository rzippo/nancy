namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// Which side of an equivalence is matched against the expression.
/// The other side is what replaces it, so the choice is the direction in which the equivalence is read.
/// </summary>
public enum CheckType
{
    /// <summary>
    /// Match the left side and substitute the right one.
    /// </summary>
    CheckLeftOnly,
    /// <summary>
    /// Match the right side and substitute the left one.
    /// </summary>
    CheckRightOnly,
    /// <summary>
    /// Try the left side first, then the right one, and use whichever matches.
    /// </summary>
    CheckBothSides
}
