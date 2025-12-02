using System.Linq;
using System.Text;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Numerics;

using System;

/// <summary>
/// Represents an interval of Rational numbers, endpoints may or may not be included.
/// </summary>
public readonly struct Interval : IToCodeString
{
    /// <summary>
    /// Lower (or left) endpoint of the interval.
    /// </summary>
    public Rational Lower { get; }
    
    /// <summary>
    /// Upper (or right) endpoint of the interval.
    /// </summary>
    public Rational Upper { get; }
    
    /// <summary>
    /// True if the lower endpoint is included in the interval.
    /// </summary>
    public bool IsLowerIncluded { get; }
    
    /// <summary>
    /// True if the upper endpoint is included in the interval.
    /// </summary>
    public bool IsUpperIncluded { get; }

    /// <summary>
    /// Constructs a new interval with the given endpoints.
    /// </summary>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <param name="isLowerIncluded"></param>
    /// <param name="isUpperIncluded"></param>
    /// <exception cref="ArgumentException"></exception>
    public Interval(
        Rational lower, 
        Rational upper,
        bool isLowerIncluded = true,
        bool isUpperIncluded = true
    )
    {
        if (lower.CompareTo(upper) > 0)
            throw new ArgumentException("Lower bound must be <= upper bound.");

        // Allow degenerate intervals [a,a], (a,a] etc., even if they might be empty.
        // Emptiness is handled by methods that operate on intervals, not by ctor.
        Lower = lower;
        Upper = upper;
        IsLowerIncluded = isLowerIncluded;
        IsUpperIncluded = isUpperIncluded;
    }

    #region Factory methods

    /// <summary>
    /// Shorthand constructor for a closed interval $[a, b]$.
    /// </summary>
    public static Interval Closed(Rational a, Rational b) =>
        new Interval(a, b, isLowerIncluded: true, isUpperIncluded: true);

    /// <summary>
    /// Shorthand constructor for an open interval $]a, b[$.
    /// </summary>
    public static Interval Open(Rational a, Rational b) =>
        new Interval(a, b, isLowerIncluded: false, isUpperIncluded: false);

    /// <summary>
    /// Shorthand constructor for a half-open interval $]a, b]$.
    /// </summary>
    public static Interval OpenClosed(Rational a, Rational b) =>
        new Interval(a, b, isLowerIncluded: false, isUpperIncluded: true);

    /// <summary>
    /// Shorthand constructor for a half-open interval $[a, b[$.
    /// </summary>
    public static Interval ClosedOpen(Rational a, Rational b) =>
        new Interval(a, b, isLowerIncluded: true, isUpperIncluded: false);

    /// <summary>
    /// Returns a new interval with the specified lower bound.
    /// </summary>
    /// <param name="lower">The new lower bound.</param>
    /// <returns>
    /// A new <see cref="Interval"/> with the updated lower bound, or
    /// <c>this</c> if the value is unchanged.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="lower"/> is greater than the current upper bound.
    /// </exception>
    public Interval WithLower(Rational lower)
    {
        if (lower == Lower)
            return this;
        if(lower > Upper)
            throw new ArgumentException("Lower bound must be <= upper bound.");
        return new Interval(
            lower,
            Upper,
            IsLowerIncluded,
            IsUpperIncluded
        );
    }

    /// <summary>
    /// Returns a new interval with the specified upper bound.
    /// </summary>
    /// <param name="upper">The new upper bound.</param>
    /// <returns>
    /// A new <see cref="Interval"/> with the updated upper bound, or
    /// <c>this</c> if the value is unchanged.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="upper"/> is less than the current lower bound.
    /// </exception>
    public Interval WithUpper(Rational upper)
    {
        if (upper == Upper)
            return this;
        if(Lower > upper)
            throw new ArgumentException("Lower bound must be <= upper bound.");
        return new Interval(
            Lower,
            upper,
            IsLowerIncluded,
            IsUpperIncluded
        );
    }

    /// <summary>
    /// Returns a new interval with the specified lower-bound inclusivity.
    /// </summary>
    /// <param name="isLowerIncluded">Whether the lower bound is included.</param>
    /// <returns>
    /// A new <see cref="Interval"/> with the updated lower-bound inclusion flag, or
    /// <c>this</c> if the value is unchanged.
    /// </returns>
    public Interval WithIsLowerIncluded(bool isLowerIncluded)
    {
        if (isLowerIncluded == IsLowerIncluded)
            return this;
        return new Interval(
            Lower,
            Upper,
            isLowerIncluded,
            IsUpperIncluded
        );
    }

    /// <summary>
    /// Returns a new interval with the specified upper-bound inclusivity.
    /// </summary>
    /// <param name="isUpperIncluded">Whether the upper bound is included.</param>
    /// <returns>
    /// A new <see cref="Interval"/> with the updated upper-bound inclusion flag, or
    /// <c>this</c> if the value is unchanged.
    /// </returns>
    public Interval WithIsUpperIncluded(bool isUpperIncluded)
    {
        if (isUpperIncluded == IsUpperIncluded)
            return this;
        return new Interval(
            Lower,
            Upper,
            IsLowerIncluded,
            isUpperIncluded
        );
    }

    #endregion

    #region Basic properties

    /// <summary>
    /// True if this interval does not contain any Rational,
    /// i.e., if the endpoints match and at least one of them is not included.
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            if(Lower < Upper)
                // non-degenerate
                return false;
            else if (IsLowerIncluded && IsUpperIncluded)
                // degenerates to a single point, but not empty
                return false;
            else
                return true;
        }
    }

    #endregion

    #region Containment

    /// <summary>
    /// True if <paramref name="x"/> is included in the interval.
    /// </summary>
    public bool Contains(Rational x)
    {
        // Left side
        int cmpL = x.CompareTo(Lower);
        if (cmpL < 0) return false;
        if (cmpL == 0 && !IsLowerIncluded) return false;

        // Right side
        int cmpU = x.CompareTo(Upper);
        if (cmpU > 0) return false;
        if (cmpU == 0 && !IsUpperIncluded) return false;

        return true;
    }

    /// <summary>
    /// Returns true if every point of this interval is inside <paramref name="other"/>.
    /// </summary>
    public bool IsSubsetOf(Interval other)
    {
        if (this.IsEmpty) 
            throw new EmptyIntervalException(this.ToString());
        if (other.IsEmpty) 
            throw new EmptyIntervalException(other.ToString());

        // Left side: other.Lower <= this.Lower
        int cmpL = Lower.CompareTo(other.Lower);
        bool leftOk = cmpL > 0 ||
                      (cmpL == 0 && (!IsLowerIncluded || other.IsLowerIncluded));

        // Right side: this.Upper <= other.Upper
        int cmpU = Upper.CompareTo(other.Upper);
        bool rightOk = cmpU < 0 ||
                       (cmpU == 0 && (!IsUpperIncluded || other.IsUpperIncluded));

        return leftOk && rightOk;
    }

    /// <summary>
    /// Returns true if every point of <paramref name="other"/> is inside this interval.
    /// </summary>
    public bool IsSupersetOf(Interval other) => other.IsSubsetOf(this);

    #endregion
    
    #region Topological operations

    /// <summary>
    /// Interior of this interval (largest open subset interval).
    /// For any non-empty interval, this is always (Lower, Upper).
    /// </summary>
    /// <exception cref="EmptyIntervalException">
    /// If the interval is empty.
    /// </exception>
    public Interval Interior()
    {
        if (IsEmpty) 
            throw new EmptyIntervalException(this.ToString());        
        return new Interval(Lower, Upper, false, false);
    }

    /// <summary>
    /// Closure of this interval (smallest closed superset interval).
    /// For any non-empty interval, this is always [Lower, Upper].
    /// </summary>
    /// <exception cref="EmptyIntervalException">
    /// If the interval is empty.
    /// </exception>
    public Interval Closure()
    {
        if (IsEmpty) 
            throw new EmptyIntervalException(this.ToString());
        return new Interval(Lower, Upper, true, true);
    }

    #endregion

    #region Overlap / intersection

    /// <summary>
    /// Returns true if the two intervals have a non-empty intersection.
    /// </summary>
    public bool Overlaps(Interval other) => Intersection(this, other) is not null;

    /// <summary>
    /// Intersection of two intervals.
    /// Returns null if the intersection is empty.
    /// </summary>
    public static Interval? Intersection(Interval a, Interval b)
    {
        // Determine lower bound = max(a.Lower, b.Lower)
        Rational lower;
        bool lowerInclusive;

        int cmpLower = a.Lower.CompareTo(b.Lower);
        if (cmpLower > 0)
        {
            lower = a.Lower;
            lowerInclusive = a.IsLowerIncluded;
        }
        else if (cmpLower < 0)
        {
            lower = b.Lower;
            lowerInclusive = b.IsLowerIncluded;
        }
        else
        {
            // same point, intersection keeps it only if both are inclusive
            lower = a.Lower;
            lowerInclusive = a.IsLowerIncluded && b.IsLowerIncluded;
        }

        // Determine upper bound = min(a.Upper, b.Upper)
        Rational upper;
        bool upperInclusive;

        int cmpUpper = a.Upper.CompareTo(b.Upper);
        if (cmpUpper < 0)
        {
            upper = a.Upper;
            upperInclusive = a.IsUpperIncluded;
        }
        else if (cmpUpper > 0)
        {
            upper = b.Upper;
            upperInclusive = b.IsUpperIncluded;
        }
        else
        {
            upper = a.Upper;
            upperInclusive = a.IsUpperIncluded && b.IsUpperIncluded;
        }

        int cmp = lower.CompareTo(upper);
        if (cmp > 0)
        {
            // Empty intersection
            return null;
        }

        if (cmp == 0 && !(lowerInclusive && upperInclusive))
        {
            // Same single point but not included on at least one side => empty
            return null;
        }

        return new Interval(lower, upper, lowerInclusive, upperInclusive);
    }

    #endregion

    #region Union / contiguity

    /// <summary>
    /// Returns the union of two intervals if it is itself a single interval;
    /// otherwise returns null.
    /// </summary>
    public static Interval? Union(Interval a, Interval b)
    {
        // First, check if they overlap (including boundary intersections).
        var inter = Intersection(a, b);
        if (inter is not null)
        {
            // The union is just the minimal bounding interval.
            return BoundingInterval(a, b);
        }

        // They don't overlap. Check if they are "contiguous":
        // they touch at a point which is included by at least one.
        if (AreTouchingWithIncludedPoint(a, b))
        {
            return BoundingInterval(a, b);
        }

        // Otherwise union would be two disjoint intervals => not representable as single Interval
        return null;
    }

    /// <summary>
    /// Returns true if the intervals are just touching (their union is a single
    /// interval but their intersection is empty).
    /// </summary>
    public bool IsContiguousWith(Interval other)
    {
        if (Intersection(this, other) is not null)
            return false;

        return AreTouchingWithIncludedPoint(this, other);
    }

    private static Interval BoundingInterval(Interval a, Interval b)
    {
        // Lower bound = min
        Rational lower;
        bool lowerInclusive;
        int cmpL = a.Lower.CompareTo(b.Lower);
        if (cmpL < 0)
        {
            lower = a.Lower;
            lowerInclusive = a.IsLowerIncluded;
        }
        else if (cmpL > 0)
        {
            lower = b.Lower;
            lowerInclusive = b.IsLowerIncluded;
        }
        else
        {
            lower = a.Lower;
            // union: include boundary if any side includes it
            lowerInclusive = a.IsLowerIncluded || b.IsLowerIncluded;
        }

        // Upper bound = max
        Rational upper;
        bool upperInclusive;
        int cmpU = a.Upper.CompareTo(b.Upper);
        if (cmpU > 0)
        {
            upper = a.Upper;
            upperInclusive = a.IsUpperIncluded;
        }
        else if (cmpU < 0)
        {
            upper = b.Upper;
            upperInclusive = b.IsUpperIncluded;
        }
        else
        {
            upper = a.Upper;
            upperInclusive = a.IsUpperIncluded || b.IsUpperIncluded;
        }

        return new Interval(lower, upper, lowerInclusive, upperInclusive);
    }

    /// <summary>
    /// Returns true if the two intervals touch at an endpoint and at least one of them includes that endpoint.
    /// </summary>
    private static bool AreTouchingWithIncludedPoint(Interval a, Interval b)
    {
        // a's upper == b's lower and at least one side includes that point
        if (a.Upper.CompareTo(b.Lower) == 0 &&
            (a.IsUpperIncluded || b.IsLowerIncluded))
        {
            return true;
        }

        // symmetric
        if (b.Upper.CompareTo(a.Lower) == 0 &&
            (b.IsUpperIncluded || a.IsLowerIncluded))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Utility

    /// <summary>
    /// True if two intervals are equal as sets of rationals.
    /// (Record's default equality already does this, but this can be more explicit.)
    /// </summary>
    public bool SetEquals(Interval other) =>
        Lower.Equals(other.Lower)
        && Upper.Equals(other.Upper)
        && IsLowerIncluded == other.IsLowerIncluded
        && IsUpperIncluded == other.IsUpperIncluded;

    /// <inheritdoc />
    public override string ToString()
    {
        var left = IsLowerIncluded ? "[" : "(";
        var right = IsUpperIncluded ? "]" : ")";
        return $"{left}{Lower}, {Upper}{right}";
    }

    /// <inheritdoc />
    public string ToCodeString(bool formatted = false, int indentation = 0)
    {
        var sb = new StringBuilder("new Interval(");
        sb.Append(Lower.ToCodeString());
        sb.Append(", ");
        sb.Append(Upper.ToCodeString());
        sb.Append(", ");
        sb.Append(IsLowerIncluded);
        sb.Append(", ");
        sb.Append(IsUpperIncluded);
        sb.Append(')');
        return sb.ToString();
    }

    #endregion
}
