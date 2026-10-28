using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class ExtremaLocations
{
    [Fact]
    public void SupremumTiesSelectTheEarliestTime()
    {
        Element[] elements = [new Point(1, 7), Segment.Constant(1, 5, 7), new Point(5, 7)];
        Assert.Equal(1, elements.SupArg());
    }

    [Fact]
    public void InfimumTiesSelectTheEarliestTime()
    {
        Element[] elements = [new Point(1, 7), Segment.Constant(1, 5, 7), new Point(5, 7)];
        Assert.Equal(1, elements.InfArg());
    }

    public static List<Segment> Plateaus =>
    [
        Segment.Constant(2, 4, 7),
        Segment.Constant(-4, -2, 7),
        Segment.Constant(-4, 0, 7),
        Segment.Constant(-4, 2, 7),
        Segment.Constant(-4, Rational.PlusInfinity, 7),
        Segment.Constant(Rational.MinusInfinity, -4, 7),
        Segment.Constant(2, Rational.PlusInfinity, 7),
        Segment.Constant(Rational.MinusInfinity, 4, 7),
        Segment.Constant(Rational.MinusInfinity, Rational.PlusInfinity, 7),
        Segment.PlusInfinite(2, 4),
        Segment.MinusInfinite(2, 4)
    ];

    public static IEnumerable<object[]> PlateauCases => Plateaus.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(PlateauCases))]
    public void MaximumArgumentBelongsToTheOpenPlateau(Segment segment)
    {
        Element[] elements = [segment];
        var argument = elements.MaxArg();
        Assert.NotNull(argument);
        Assert.True(segment.IsDefinedFor(argument.Value));
        Assert.Equal(segment.RightLimitAtStartTime, segment.ValueAt(argument.Value));
    }

    [Theory]
    [MemberData(nameof(PlateauCases))]
    public void MinimumArgumentBelongsToTheOpenPlateau(Segment segment)
    {
        Element[] elements = [segment];
        var argument = elements.MinArg();
        Assert.NotNull(argument);
        Assert.True(segment.IsDefinedFor(argument.Value));
        Assert.Equal(segment.RightLimitAtStartTime, segment.ValueAt(argument.Value));
    }
}
