using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class DisconnectedPairEnvelopes
{
    public static IEnumerable<(Sequence Left, Sequence Right, Element[] Expected)> DisconnectedSupports()
    {
        var left = Segment.Constant(0, 1, 2);
        var separated = Segment.Constant(2, 3, 3);
        yield return (new Sequence([left]), new Sequence([separated]), [left, separated]);
        var adjoining = Segment.Constant(1, 2, 2);
        yield return (new Sequence([left]), new Sequence([adjoining]), [left, adjoining]);
        var point = new Point(0, 4);
        yield return (new Sequence([point]), new Sequence([left]), [point, left]);
        var laterPoint = new Point(2, 3);
        yield return (new Sequence([point]), new Sequence([laterPoint]), [point, laterPoint]);
    }

    public static IEnumerable<object[]> Cases() => DisconnectedSupports().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void LowerEnvelopePreservesTheActualSupportUnion(Sequence left, Sequence right, Element[] expected)
    {
        Assert.Equal(expected, Sequence.LowerEnvelope(left, right));
        Assert.Equal(expected, Sequence.LowerEnvelope(right, left));
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void UpperEnvelopePreservesTheActualSupportUnion(Sequence left, Sequence right, Element[] expected)
    {
        Assert.Equal(expected, Sequence.UpperEnvelope(left, right));
        Assert.Equal(expected, Sequence.UpperEnvelope(right, left));
    }
}
