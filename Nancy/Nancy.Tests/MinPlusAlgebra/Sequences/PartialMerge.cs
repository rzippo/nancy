using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class PartialMerge
{
    public static IEnumerable<(Element[] Input, Element[] Expected)> PartialCases()
    {
        yield return ([new Point(0, 1), new Point(2, 1)], [new Point(0, 1), new Point(2, 1)]);
        yield return ([Segment.Constant(0, 1, 1), Segment.Constant(1, 2, 1)],
            [Segment.Constant(0, 1, 1), Segment.Constant(1, 2, 1)]);
        yield return ([Segment.Constant(0, 1, 1), new Point(2, 1), Segment.Constant(2, 3, 1)],
            [Segment.Constant(0, 1, 1), new Point(2, 1), Segment.Constant(2, 3, 1)]);
        yield return ([Segment.Constant(0, 1, 1), new Point(1, 1), Segment.Constant(1, 2, 1), new Point(4, 2)],
            [Segment.Constant(0, 2, 1), new Point(4, 2)]);
    }

    public static IEnumerable<object[]> Cases() => PartialCases().ToXUnitTestCases();
    public static IEnumerable<object[]> InvalidCases() => Fill.OverlappingInputs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void StreamingMergePreservesGapsAndMissingBoundaryPoints(Element[] input, Element[] expected)
        => Assert.Equal(expected, ValidationCases.SingleUse(input).MergeAsEnumerable()
            .ToArray());

    [Theory]
    [MemberData(nameof(Cases))]
    public void MaterializedMergePreservesGapsAndMissingBoundaryPoints(Element[] input, Element[] expected)
        => Assert.Equal(expected, input.Merge());

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void StreamingMergeRejectsOverlappingOrUnorderedInput(Element[] input)
        => Assert.Throws<ArgumentException>(() => input.MergeAsEnumerable()
            .ToList());

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void MaterializedMergeRejectsOverlappingOrUnorderedInput(Element[] input)
        => Assert.Throws<ArgumentException>(() => input.Merge());

    [Fact]
    public void StreamingMergeRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().MergeAsEnumerable()
            .ToList());

    [Fact]
    public void MaterializedMergeRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().Merge());
}
