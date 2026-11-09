using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Checks selection without copying and translation of stored pseudo-period elements.
/// </summary>
public class PeriodElements
{
    private static Curve Example(int height = 3) => new(new Sequence([
        Point.Origin(), Segment.Zero(0, 2),
        new Point(2, 10), new Segment(2, 4, 10, 1),
        new Point(4, 20), new Segment(4, 6, 30, -2),
        new Point(6, 50), new Segment(6, 8, 60, 0)
    ]), 2, 6, height);

    public static IEnumerable<object[]> Selections()
    {
        // The second translated period occupies [14, 20[; both internal endpoints coincide with stored points.
        yield return new object[] { 16, 18, false, false, 3, 1 };
        yield return new object[] { 16, 18, false, true, 3, 2 };
        yield return new object[] { 16, 18, true, false, 2, 2 };
        yield return new object[] { 16, 18, true, true, 2, 3 };

        // The cut starts inside this period and extends beyond it; only the lower inclusion flag affects the selection.
        yield return new object[] { 16, 22, false, false, 3, 3 };
        yield return new object[] { 16, 22, true, true, 2, 4 };

        // The cut starts before this period and ends inside it; only the upper inclusion flag affects the selection.
        yield return new object[] { 12, 18, false, false, 0, 4 };
        yield return new object[] { 12, 18, true, true, 0, 5 };
        yield return new object[] { 12, 22, false, false, 0, 6 };

        // The stored period has no point at its excluded end, even when the cut includes that time.
        yield return new object[] { 14, 20, false, false, 1, 5 };
        yield return new object[] { 14, 20, false, true, 1, 5 };
        yield return new object[] { 14, 20, true, false, 0, 6 };
        yield return new object[] { 14, 20, true, true, 0, 6 };

        // Boundaries inside segments retain those whole segments for the caller to clip.
        yield return new object[] { 15, 19, true, true, 1, 5 };
    }

    [Theory]
    [MemberData(nameof(Selections))]
    public void SelectionReturnsTheRequiredStoredView(
        int start, int end, bool includeStart, bool includeEnd, int first, int count)
    {
        var curve = Example();
        var period = curve.PseudoPeriodicSequence;
        var selected = curve.GetPeriodElementsForCut(period, 2, new Interval(start, end, includeStart, includeEnd));

        Assert.Equal(count, selected.Count);
        Assert.True(period.AsSpan().Overlaps(selected.AsSpan(), out int offset));
        Assert.Equal(first, offset);
        for (int i = 0; i < count; i++)
            Assert.Same(period.Elements[first + i], selected[i]);
    }

    public static IEnumerable<object[]> Translations()
    {
        foreach (bool parallel in new[] { false, true })
        {
            yield return new object[] { parallel, 2, 3, new Element[] {
                new Segment(14, 16, 16, 1), new Point(16, 26),
                new Segment(16, 18, 36, -2), new Point(18, 56)
            } };
            yield return new object[] { parallel, 3, -2, new Element[] {
                new Segment(20, 22, 4, 1), new Point(22, 14),
                new Segment(22, 24, 24, -2), new Point(24, 44)
            } };
            yield return new object[] { parallel, 0, 3, new Element[] {
                new Segment(2, 4, 10, 1), new Point(4, 20),
                new Segment(4, 6, 30, -2), new Point(6, 50)
            } };
        }
    }

    [Theory]
    [MemberData(nameof(Translations))]
    public void TranslationPreservesOrderAndSlopesWithoutChangingTheSource(
        bool parallel, int periodIndex, int height, Element[] expected)
    {
        var curve = Example(height);
        var selected = curve.PseudoPeriodicSequence.ElementList.Slice(1, 4);
        var source = selected.ToArray();
        var settings = new ComputationSettings
        {
            UseParallelism = parallel,
            ComputeExtensionSequencesParallelizationThreshold = 0,
            ExtensionMinimumElementsPerPartition = 1
        };
        var translated = curve.TranslatePeriodElements(selected, periodIndex, settings,
            maximumParallelism: 4);

        Assert.Equal(expected, translated.ToArray());
        Assert.Equal(expected, translated.ToArray());
        Assert.Equal(source, selected.ToArray());
    }
}
