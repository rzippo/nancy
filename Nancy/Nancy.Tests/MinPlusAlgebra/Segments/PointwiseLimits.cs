using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class PointwiseLimits
{
    public static List<(Segment Left, Segment Right, Segment Sum, Segment Difference)> FiniteCases =>
    [
        (new(0, 4, 2, 3), new(0, 4, 7, -1), new(0, 4, 9, 2), new(0, 4, -5, 4)),
        (new(0, 4, 2, 3), new(1, 5, 7, -1), new(1, 4, 12, 2), new(1, 4, -2, 4)),
        (new(1, 5, 7, -1), new(0, 4, 2, 3), new(1, 4, 12, 2), new(1, 4, 2, -4)),
        (new(0, 5, 1, -2), new(2, 3, 4, 3), new(2, 3, 1, 1), new(2, 3, -7, -5)),
        (new(0, 2, 1, 0), new(0, 4, 3, 2), new(0, 2, 4, 2), new(0, 2, -2, -2)),
        (new(new Rational(1, 2), new Rational(5, 2), new Rational(3, 2), 2),
            new(new Rational(3, 2), new Rational(7, 2), new Rational(1, 2), -1),
            new(new Rational(3, 2), new Rational(5, 2), 4, 1),
            new(new Rational(3, 2), new Rational(5, 2), 3, 3)),
        (new(1, Rational.PlusInfinity, 2, 3), new(2, Rational.PlusInfinity, 5, -1),
            new(2, Rational.PlusInfinity, 10, 2), new(2, Rational.PlusInfinity, 0, 4))
    ];

    public static IEnumerable<object[]> AdditionOperands => FiniteCases
        .Select(c => (c.Left, c.Right, c.Sum))
        .ToXUnitTestCases();

    public static IEnumerable<object[]> SubtractionOperands => FiniteCases
        .Select(c => (c.Left, c.Right, c.Difference))
        .ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(AdditionOperands))]
    public void AdditionUsesTheLimitsAtTheOverlapStart(Segment left, Segment right, Segment sum)
    {
        Assert.Equal(sum, Segment.Addition(left, right));
    }

    [Theory]
    [MemberData(nameof(SubtractionOperands))]
    public void SubtractionUsesTheLimitsAtTheOverlapStart(Segment left, Segment right, Segment difference)
    {
        Assert.Equal(difference, Segment.Subtraction(left, right));
    }

    public static List<(Segment Left, Segment Right)> DisjointCases =>
    [
        (new(0, 2, 1, 3), new(2, 4, 5, -1)),
        (new(2, 4, 5, -1), new(0, 2, 1, 3)),
        (new(0, 1, 1, 3), new(3, 4, 5, -1))
    ];

    public static IEnumerable<object[]> DisjointOperands => DisjointCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DisjointOperands))]
    public void AdditionRejectsDisjointOpenSupports(Segment left, Segment right)
        => Assert.Throws<ArgumentException>(() => Segment.Addition(left, right));

    [Theory]
    [MemberData(nameof(DisjointOperands))]
    public void SubtractionRejectsDisjointOpenSupports(Segment left, Segment right)
        => Assert.Throws<ArgumentException>(() => Segment.Subtraction(left, right));

    public static List<(Segment Infinite, Segment Finite)> InfiniteCases =>
    [
        (Segment.PlusInfinite(0, 4), new(1, 5, 2, -3)),
        (Segment.MinusInfinite(1, 5), new(0, 4, 2, 3))
    ];

    public static IEnumerable<object[]> InfiniteOperands => InfiniteCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InfiniteOperands))]
    public void AdditionPreservesAnInfiniteValueOverTheOverlap(Segment infinite, Segment finite)
    {
        var expected = Segment.Constant(1, 4, infinite.RightLimitAtStartTime);
        Assert.Equal(expected, Segment.Addition(infinite, finite));
        Assert.Equal(expected, Segment.Addition(finite, infinite));
    }

    [Theory]
    [MemberData(nameof(InfiniteOperands))]
    public void SubtractionPreservesTheSignedInfiniteValue(Segment infinite, Segment finite)
    {
        Assert.Equal(Segment.Constant(1, 4, infinite.RightLimitAtStartTime), Segment.Subtraction(infinite, finite));
        Assert.Equal(Segment.Constant(1, 4, -infinite.RightLimitAtStartTime), Segment.Subtraction(finite, infinite));
    }

    public static IEnumerable<object[]> InfiniteSegments => InfiniteCases
        .Select(c => c.Infinite)
        .ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InfiniteSegments))]
    public void AdditionOfOppositeInfinitiesIsUndetermined(Segment infinite)
        => Assert.Throws<UndeterminedResultException>(() => Segment.Addition(infinite, -infinite));

    [Theory]
    [MemberData(nameof(InfiniteSegments))]
    public void SubtractionOfEqualInfinitiesIsUndetermined(Segment infinite)
        => Assert.Throws<UndeterminedResultException>(() => Segment.Subtraction(infinite, infinite));
}
