using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Checks the pseudo-inverses of a sequence at a height, $f^{-1}_\downarrow(y) = \inf \{ t \in D \mid f(t) \ge y \}$ and $f^{-1}_\uparrow(y) = \sup \{ t \in D \mid f(t) \le y \}$ over its domain $D$,
/// against values computed by hand, and that a sequence that is not non-decreasing is rejected.
/// </summary>
public class PseudoInverseAt
{
    private static readonly Rational Inf = Rational.PlusInfinity;
    private static readonly Rational MinusInf = Rational.MinusInfinity;

    /// <summary>$t$ over $[1, 3]$, or with either end excluded.</summary>
    private static Sequence Ramp(bool isStartIncluded, bool isEndIncluded)
    {
        var elements = new List<Element>();
        if (isStartIncluded)
            elements.Add(new Point(1, 1));
        elements.Add(new Segment(1, 3, 1, 1));
        if (isEndIncluded)
            elements.Add(new Point(3, 3));
        return new Sequence(elements);
    }

    /// <summary>1 over $[1, 2]$, then rising from 3 to 4 over $]2, 3[$, excluded.</summary>
    private static readonly Sequence PlateauAndJump = new Sequence([
        new Point(1, 1), Segment.Constant(1, 2, 1), new Point(2, 1), new Segment(2, 3, 3, 1)
    ]);

    /// <summary>$t$ over $[0, 2[$, 3 at 2, and $+\infty$ over $]2, 3]$.</summary>
    private static readonly Sequence SlopeIntoPlusInfinity = new Sequence([
        Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, 3), Segment.PlusInfinite(2, 3), Point.PlusInfinite(3)
    ]);

    public static List<(string name, Sequence f, Rational y, Rational lower, Rational upper)> Cases =
    [
        ("closed, below the image", Ramp(true, true), 0, 1, MinusInf),
        ("closed, inside the image", Ramp(true, true), 2, 2, 2),
        ("closed, at the top of the image", Ramp(true, true), 3, 3, 3),
        ("closed, above the image", Ramp(true, true), 4, Inf, 3),
        // the start is excluded: the domain still starts at 1, and no instant is at or below 1
        ("open start, below the image", Ramp(false, true), 0, 1, MinusInf),
        ("open start, at the right limit at the start", Ramp(false, true), 1, 1, MinusInf),
        ("open start, inside the image", Ramp(false, true), 2, 2, 2),
        // the end is excluded: no instant reaches 3, and every instant is below it
        ("open end, at the left limit at the end", Ramp(true, false), 3, Inf, 3),
        ("open end, above the image", Ramp(true, false), 4, Inf, 3),
        ("plateau and jump, at the plateau", PlateauAndJump, 1, 1, 2),
        ("plateau and jump, inside the jump", PlateauAndJump, 2, 2, 2),
        ("plateau and jump, after the jump", PlateauAndJump, new Rational(7, 2), new Rational(5, 2), new Rational(5, 2)),
        ("plateau and jump, at the left limit at the open end", PlateauAndJump, 4, Inf, 3),
        ("single point, below it", new Sequence([new Point(2, 5)]), 4, 2, MinusInf),
        ("single point, at it", new Sequence([new Point(2, 5)]), 5, 2, 2),
        ("single point, above it", new Sequence([new Point(2, 5)]), 6, Inf, 2),
        ("into +inf, at its value at 2", SlopeIntoPlusInfinity, 3, 2, 2),
        ("into +inf, above its value at 2", SlopeIntoPlusInfinity, 4, 2, 2),
        ("into +inf, at +inf", SlopeIntoPlusInfinity, Inf, 2, 3),
    ];

    public static IEnumerable<object[]> CaseData() => Cases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(CaseData))]
    public void LowerPseudoInverseAtKnownValues(string name, Sequence f, Rational y, Rational lower, Rational upper)
    {
        _ = upper;

        Assert.False(string.IsNullOrEmpty(name));
        Assert.Equal(lower, f.LowerPseudoInverseAt(y));
    }

    [Theory]
    [MemberData(nameof(CaseData))]
    public void UpperPseudoInverseAtKnownValues(string name, Sequence f, Rational y, Rational lower, Rational upper)
    {
        _ = lower;

        Assert.False(string.IsNullOrEmpty(name));
        Assert.Equal(upper, f.UpperPseudoInverseAt(y));
    }

    [Fact]
    public void ThrowsForASequenceThatIsNotNonDecreasing()
    {
        var decreasing = new Sequence([new Point(0, 1), new Segment(0, 1, 1, -1), new Point(1, 0)]);
        Assert.Throws<ArgumentException>(() => decreasing.LowerPseudoInverseAt(1));
        Assert.Throws<ArgumentException>(() => decreasing.UpperPseudoInverseAt(1));
    }
}
