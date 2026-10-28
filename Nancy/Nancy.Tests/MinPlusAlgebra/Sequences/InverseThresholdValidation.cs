using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class InverseThresholdValidation
{
    public static IEnumerable<Element[]> DecreasingAcrossZero()
    {
        yield return [new Point(0, -1), Segment.Constant(0, 1, -1),
            new Point(1, -2), Segment.Constant(1, 2, -2), new Point(2, 1)];
        yield return [Point.Origin(), Segment.Zero(0, 1),
            new Point(1, -1), Segment.Constant(1, 2, -1), new Point(2, 1)];
    }

    public static IEnumerable<object[]> DecreasingAcrossZeroCases() => DecreasingAcrossZero().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DecreasingAcrossZeroCases))]
    public void LowerInverseChecksDecreasesAcrossTheZeroThreshold(Element[] elements)
        => Assert.Throws<ArgumentException>(() => elements
            .LowerPseudoInverse(startFromZero: true)
            .ToList());

    [Theory]
    [MemberData(nameof(DecreasingAcrossZeroCases))]
    public void UpperInverseChecksDecreasesAcrossTheZeroThreshold(Element[] elements)
        => Assert.Throws<ArgumentException>(() => elements
            .UpperPseudoInverse(startFromZero: true)
            .ToList());

}
