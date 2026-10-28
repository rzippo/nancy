using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class DelaySupport
{
    public static IEnumerable<Sequence> OpenInputs()
    {
        yield return new Sequence([Segment.Constant(-2, 0, 5)]);
        yield return new Sequence([Segment.Constant(0, 2, 5)]);
        yield return new Sequence([Segment.Constant(3, 5, 5)]);
    }

    public static IEnumerable<object[]> OpenInputCases() => OpenInputs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(OpenInputCases))]
    public void AZeroPrefixSuppliesTheJoinOfAnOpenInput(Sequence sequence)
    {
        var result = sequence.Delay(2);
        Assert.True(result.IsLeftOpen);
        Assert.Equal(sequence.DefinedFrom, result.DefinedFrom);
        Assert.Equal(sequence.DefinedUntil + 2, result.DefinedUntil);
        Assert.Equal(0, result.ValueAt(sequence.DefinedFrom + 2));
        Assert.Equal(5, result.RightLimitAt(sequence.DefinedFrom + 2));
    }
}
