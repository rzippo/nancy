using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class FillBounds
{
    public static IEnumerable<(Element[] Input, Rational From, Rational To, Element[] Expected)> BoundedCases()
    {
        yield return ([Segment.Constant(5, 6, 2)], 0, 1,
            [Point.PlusInfinite(0), Segment.PlusInfinite(0, 1), Segment.Constant(5, 6, 2)]);
        yield return ([new Point(0, 2), new Point(10, 3)], 3, 5,
            [new Point(0, 2), Point.PlusInfinite(3), Segment.PlusInfinite(3, 5), new Point(10, 3)]);
    }

    public static IEnumerable<object[]> Cases() => BoundedCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void MissingValuesAreInsertedOnlyInsideTheRequestedInterval(
        Element[] input, Rational from, Rational to, Element[] expected)
        => Assert.Equal(expected, input.Fill(from, to)
            .ToArray());
}
