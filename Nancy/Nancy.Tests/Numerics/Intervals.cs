using System;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics;

public class IntervalTests
{
    [Fact]
    public void ClosedInterval_Contains_Endpoints()
    {
        var interval = Interval.Closed(1, 5);

        Assert.True(interval.Contains(1));
        Assert.True(interval.Contains(5));
        Assert.True(interval.Contains(3));

        Assert.False(interval.Contains(0));
        Assert.False(interval.Contains(6));
    }

    [Fact]
    public void OpenInterval_DoesNotContain_Endpoints()
    {
        var interval = Interval.Open(1, 5);

        Assert.False(interval.Contains(1));
        Assert.False(interval.Contains(5));
        Assert.True(interval.Contains(3));
    }

    [Fact]
    public void EmptyInterval_WhenSameBoundsButExclusive()
    {
        var open = Interval.Open(2, 2);
        var leftClosed = Interval.ClosedOpen(2, 2);
        var rightClosed = Interval.OpenClosed(2, 2);
        var closed = Interval.Closed(2, 2);

        Assert.True(open.IsEmpty);
        Assert.True(leftClosed.IsEmpty);
        Assert.True(rightClosed.IsEmpty);
        Assert.False(closed.IsEmpty); // [2,2] contains exactly one point
    }

    [Fact]
    public void Intersection_OverlappingIntervals()
    {
        var a = Interval.Closed(1, 5);
        var b = Interval.Closed(3, 7);

        var intersection = Interval.Intersection(a, b);

        Assert.NotNull(intersection);
        Assert.Equal(Interval.Closed(3, 5), intersection.Value);
    }

    [Fact]
    public void Intersection_NonOverlappingIntervals_IsNull()
    {
        var a = Interval.Closed(1, 2);
        var b = Interval.Closed(3, 4);

        var intersection = Interval.Intersection(a, b);

        Assert.Null(intersection);
    }

    [Fact]
    public void Intersection_TouchingAtPoint_BothInclusive_IncludesPoint()
    {
        var a = Interval.Closed(1, 2);     // [1,2]
        var b = Interval.Closed(2, 3);     // [2,3]

        var intersection = Interval.Intersection(a, b);

        Assert.NotNull(intersection);
        Assert.Equal(Interval.Closed(2, 2), intersection.Value);
    }

    [Fact]
    public void Intersection_TouchingAtPoint_OneExclusive_IsEmpty()
    {
        var a = Interval.Closed(1, 2);     // [1,2]
        var b = Interval.Open(2, 3);       // (2,3)

        var intersection = Interval.Intersection(a, b);

        Assert.Null(intersection);
    }

    [Fact]
    public void Union_OverlappingIntervals_ReturnsBoundingInterval()
    {
        var a = Interval.Closed(1, 5);
        var b = Interval.Closed(3, 7);

        var union = Interval.Union(a, b);

        Assert.NotNull(union);
        Assert.Equal(Interval.Closed(1, 7), union.Value);
    }

    [Fact]
    public void Union_TouchingWithInclusivePoint_ReturnsSingleInterval()
    {
        var a = Interval.Closed(1, 2);     // [1,2]
        var b = Interval.Open(2, 4);       // (2,4)

        var union = Interval.Union(a, b);

        Assert.NotNull(union);
        // union should be [1,4) because 2 is included on at least one side
        Assert.Equal(Interval.ClosedOpen(1, 4), union.Value);
    }

    [Fact]
    public void Union_TouchingButBothExclusive_IsNull()
    {
        var a = Interval.Open(1, 2);       // (1,2)
        var b = Interval.Open(2, 3);       // (2,3)

        var union = Interval.Union(a, b);

        Assert.Null(union);
    }

    [Fact]
    public void Union_DisjointIntervals_IsNull()
    {
        var a = Interval.Closed(1, 2);
        var b = Interval.Closed(4, 5);

        var union = Interval.Union(a, b);

        Assert.Null(union);
    }

    [Fact]
    public void IsContiguousWith_TouchingWithIncludedPoint_ReturnsTrue()
    {
        var a = Interval.Closed(1, 2);     // [1,2]
        var b = Interval.Open(2, 3);       // (2,3)

        Assert.True(a.IsContiguousWith(b));
        Assert.True(b.IsContiguousWith(a));
    }

    [Fact]
    public void IsContiguousWith_Overlapping_ReturnsFalse()
    {
        var a = Interval.Closed(1, 3);
        var b = Interval.Closed(2, 4);

        Assert.False(a.IsContiguousWith(b));
    }

    [Fact]
    public void IsContiguousWith_Separated_ReturnsFalse()
    {
        var a = Interval.Closed(1, 2);
        var b = Interval.Closed(3, 4);

        Assert.False(a.IsContiguousWith(b));
    }

    [Fact]
    public void Subset_Superset_BasicCases()
    {
        var outer = Interval.Closed(1, 10);
        var inner = Interval.Open(2, 5);
        var disjoint = Interval.Closed(11, 12);

        Assert.True(inner.IsSubsetOf(outer));
        Assert.True(outer.IsSupersetOf(inner));

        Assert.False(outer.IsSubsetOf(inner));
        Assert.False(inner.IsSupersetOf(outer));

        Assert.False(disjoint.IsSubsetOf(outer));
        Assert.False(outer.IsSubsetOf(disjoint));
    }

    [Fact]
    public void Subset_RespectsInclusivityOnBoundaries()
    {
        var a = Interval.Closed(1, 5);       // [1,5]
        var b = Interval.OpenClosed(1, 5);   // (1,5]

        // b is subset of a (loses 1)
        Assert.True(b.IsSubsetOf(a));
        // a is not subset of b (because 1 is missing in b)
        Assert.False(a.IsSubsetOf(b));
    }
    
        [Fact]
    public void Interior_OfClosedInterval_IsOpen()
    {
        var interval = Interval.Closed(1, 5);   // [1,5]
        var interior = interval.Interior();           // (1,5)

        Assert.False(interior.IsEmpty);
        Assert.Equal(Interval.Open(1, 5), interior);
    }

    [Fact]
    public void Interior_OfOpenInterval_IsItself()
    {
        var interval = Interval.Open(1, 5);     // (1,5)
        var interior = interval.Interior();

        Assert.Equal(interval, interior);
    }

    [Fact]
    public void Interior_OfHalfOpen_IsOpen()
    {
        var leftClosed = Interval.ClosedOpen(1, 5);  // [1,5)
        var rightClosed = Interval.OpenClosed(1, 5); // (1,5]

        Assert.Equal(Interval.Open(1, 5), leftClosed.Interior());
        Assert.Equal(Interval.Open(1, 5), rightClosed.Interior());
    }

    [Fact]
    public void Interior_OfSingletonClosed_IsEmpty()
    {
        var singleton = Interval.Closed(2, 2);      // [2,2]
        var interior = singleton.Interior();              // (2,2) empty

        Assert.True(interior.IsEmpty);
        Assert.Equal(Interval.Open(2, 2), interior);
    }

    [Fact]
    public void Interior_OfAlreadyEmpty_Throws()
    {
        var empty = Interval.Open(3, 3);            // (3,3) empty
        Assert.Throws<EmptyIntervalException>(() => empty.Interior());
    }

    [Fact]
    public void Closure_OfOpenInterval_IsClosed()
    {
        var interval = Interval.Open(1, 5);         // (1,5)
        var closure = interval.Closure();                 // [1,5]

        Assert.False(closure.IsEmpty);
        Assert.Equal(Interval.Closed(1, 5), closure);
    }

    [Fact]
    public void Closure_OfHalfOpen_IsClosed()
    {
        var leftClosed = Interval.ClosedOpen(1, 5);  // [1,5)
        var rightClosed = Interval.OpenClosed(1, 5); // (1,5]

        Assert.Equal(Interval.Closed(1, 5), leftClosed.Closure());
        Assert.Equal(Interval.Closed(1, 5), rightClosed.Closure());
    }

    [Fact]
    public void Closure_OfClosedInterval_IsItself()
    {
        var interval = Interval.Closed(1, 5);       // [1,5]
        var closure = interval.Closure();

        Assert.Equal(interval, closure);
    }

    [Fact]
    public void Closure_OfEmptyInterval_Throws()
    {
        var empty = Interval.Open(2, 2);            // (2,2) empty
        Assert.Throws<EmptyIntervalException>(() => empty.Closure());
    }

    [Fact]
    public void InteriorAndClosure_Relationships()
    {
        var interval = Interval.OpenClosed(1, 5);   // (1,5]
        var interior = interval.Interior();               // (1,5)
        var closure = interval.Closure();                 // [1,5]

        Assert.True(interior.IsSubsetOf(interval));
        Assert.True(interval.IsSubsetOf(closure));
        Assert.True(interior.IsSubsetOf(closure));
    }

    #region Unbounded intervals

    [Fact]
    public void UnboundedAbove_RunsFromItsBoundForever()
    {
        var interval = Interval.UnboundedAbove(5);            // [5, +inf[

        Assert.Equal(5, interval.Lower);
        Assert.True(interval.IsLowerIncluded);
        Assert.True(interval.IsUnboundedAbove);
        Assert.False(interval.IsUnboundedBelow);
    }

    [Fact]
    public void UnboundedBelow_RunsToItsBoundFromForever()
    {
        var interval = Interval.UnboundedBelow(5);            // ]-inf, 5]

        Assert.Equal(5, interval.Upper);
        Assert.True(interval.IsUpperIncluded);
        Assert.True(interval.IsUnboundedBelow);
        Assert.False(interval.IsUnboundedAbove);
    }

    [Fact]
    public void IsBounded_SeparatesFiniteStretchesFromUnboundedOnes()
    {
        Assert.True(Interval.Closed(1, 5).IsBounded);
        Assert.True(Interval.Open(1, 5).IsBounded);
        Assert.False(Interval.UnboundedAbove(5).IsBounded);
        Assert.False(Interval.UnboundedBelow(5).IsBounded);
        Assert.False(new Interval(Rational.MinusInfinity, Rational.PlusInfinity, false, false).IsBounded);
    }

    [Fact]
    public void Length_MeasuresTheSpanRegardlessOfInclusion()
    {
        Assert.Equal(4, Interval.Closed(1, 5).Length);
        Assert.Equal(4, Interval.Open(1, 5).Length);
        Assert.Equal(0, Interval.Closed(3, 3).Length);
        Assert.True(Interval.UnboundedAbove(5).Length.IsPlusInfinite);
        Assert.True(Interval.UnboundedBelow(5).Length.IsPlusInfinite);
    }

    [Fact]
    public void ADegenerateIntervalHasNoLengthEvenAtAnInfinity()
    {
        // the endpoints coincide, so the interval holds at most the one point they name and the
        // distance between them is 0; subtracting would be undetermined at an infinity
        Assert.Equal(0, Interval.Closed(5, 5).Length);
        Assert.Equal(0, new Interval(Rational.PlusInfinity, Rational.PlusInfinity, true, true).Length);
        Assert.Equal(0, new Interval(Rational.MinusInfinity, Rational.MinusInfinity, true, true).Length);
        Assert.Equal(0, Interval.UnboundedAbove(Rational.PlusInfinity).Length);
    }

    [Fact]
    public void AnIntervalReachingAnInfinityHasInfiniteLength()
    {
        Assert.True(Interval.UnboundedAbove(5).Length.IsPlusInfinite);
        Assert.True(Interval.UnboundedBelow(5).Length.IsPlusInfinite);
        Assert.True(new Interval(Rational.MinusInfinity, Rational.PlusInfinity, false, false).Length.IsPlusInfinite);
        Assert.True(Interval.Closed(0, Rational.PlusInfinity).Length.IsPlusInfinite);
    }

    [Fact]
    public void TheInteriorOfASinglePointIsEmptyEvenAtAnInfinity()
    {
        Assert.True(Interval.Closed(5, 5).Interior().IsEmpty);
        Assert.True(new Interval(Rational.PlusInfinity, Rational.PlusInfinity, true, true).Interior().IsEmpty);
    }

    [Fact]
    public void TheClosureOfAnUnboundedIntervalReachesItsInfinity()
    {
        // in the extended rationals the infinity is a point of the space, so it belongs to the closure
        var closure = Interval.UnboundedAbove(0).Closure();

        Assert.True(closure.IsUpperIncluded);
        Assert.True(closure.Contains(Rational.PlusInfinity));
    }

    [Fact]
    public void UnboundedFactoryIntervalsDoNotContainTheirInfinity()
    {
        // the unbounded factories model an open infinite endpoint, as time windows usually need
        Assert.False(Interval.UnboundedAbove(5).Contains(Rational.PlusInfinity));
        Assert.True(Interval.UnboundedAbove(5).Contains(1000000));
        Assert.False(Interval.UnboundedBelow(5).Contains(Rational.MinusInfinity));
        Assert.True(Interval.UnboundedBelow(5).Contains(-1000000));
    }

    [Fact]
    public void TheConstructorCanIncludeAnInfiniteUpperEndpoint()
    {
        // Interval is generic over Rational, so value intervals can include an attained infinity.
        var interval = new Interval(5, Rational.PlusInfinity, true, true);

        Assert.True(interval.IsUnboundedAbove);
        Assert.True(interval.IsUpperIncluded);
        Assert.True(interval.IsLowerIncluded);
        Assert.Equal(5, interval.Lower);
        Assert.True(interval.Contains(Rational.PlusInfinity));
    }

    [Fact]
    public void TheConstructorCanIncludeAnInfiniteLowerEndpoint()
    {
        var interval = new Interval(Rational.MinusInfinity, 5, true, true);

        Assert.True(interval.IsUnboundedBelow);
        Assert.True(interval.IsLowerIncluded);
        Assert.True(interval.IsUpperIncluded);
        Assert.True(interval.Contains(Rational.MinusInfinity));
    }

    [Fact]
    public void TheConstructorLeavesFiniteEndpointsAlone()
    {
        var interval = new Interval(1, 5, true, true);

        Assert.True(interval.IsLowerIncluded);
        Assert.True(interval.IsUpperIncluded);
    }

    [Fact]
    public void FactoriesAndMutationPreserveTheirOwnInfinityInclusion()
    {
        // closed intervals keep infinities as values; the unbounded factories leave them open by choice
        Assert.True(Interval.Closed(0, Rational.PlusInfinity).IsUpperIncluded);
        Assert.False(Interval.UnboundedAbove(0).IsUpperIncluded);
        Assert.True(Interval.Closed(0, 5).WithUpper(Rational.PlusInfinity).IsUpperIncluded);
        Assert.True(Interval.Open(0, Rational.PlusInfinity).WithIsUpperIncluded(true).IsUpperIncluded);
    }

    [Fact]
    public void UnboundedIntervalsJoinLikeAnyOther()
    {
        // the case the intersection API relies on: a bounded stretch running into an unbounded one
        var joined = Interval.Union(Interval.ClosedOpen(0, 5), Interval.UnboundedAbove(5));

        Assert.NotNull(joined);
        Assert.Equal(0, joined!.Value.Lower);
        Assert.True(joined.Value.IsUnboundedAbove);
    }

    [Fact]
    public void UnboundedIntervalsWithAGapDoNotJoin()
    {
        // 5 belongs to neither, so the union is not an interval
        Assert.Null(Interval.Union(Interval.ClosedOpen(0, 5), Interval.UnboundedAbove(5, isLowerIncluded: false)));
    }

    #endregion
}
