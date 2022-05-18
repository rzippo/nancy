using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus;

public class ConcaveCurveConvolutions
{
    public static IEnumerable<object[]> GetEquivalenceTestCases()
    {
        var testcases = new (Curve a, Curve b)[]
        {
            (
                a: Curve.FromJson("{\"type\":\"rateLatencyServiceCurve\",\"rate\":{\"num\":100,\"den\":1},\"latency\":{\"num\":0,\"den\":1}}"),
                b: Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":{\"num\":0,\"den\":1},\"value\":{\"num\":83,\"den\":80},\"type\":\"point\"},{\"startTime\":{\"num\":0,\"den\":1},\"endTime\":{\"num\":1,\"den\":1},\"slope\":{\"num\":15,\"den\":2},\"rightLimit\":{\"num\":83,\"den\":80},\"type\":\"segment\"},{\"time\":{\"num\":1,\"den\":1},\"value\":{\"num\":683,\"den\":80},\"type\":\"point\"},{\"startTime\":{\"num\":1,\"den\":1},\"endTime\":{\"num\":2,\"den\":1},\"slope\":{\"num\":15,\"den\":2},\"rightLimit\":{\"num\":683,\"den\":80},\"type\":\"segment\"}]},\"periodStart\":{\"num\":1,\"den\":1},\"periodLength\":{\"num\":1,\"den\":1},\"periodHeight\":{\"num\":15,\"den\":2}}")
            )        
        };

        foreach (var (a, b) in testcases)
        {
            yield return new object[] { a, b };
        }
    }

    [Theory]
    [MemberData(nameof(GetEquivalenceTestCases))]
    public void EquivalenceTest(Curve a, Curve b)
    {
        Assert.True(a.IsConcave);
        Assert.True(b.IsConcave);

        
        var conv_asIs = Curve.Convolution(a, b);
        var min_asIs = Curve.Minimum(a, b);
        if (a.ValueAt(0) == 0 && b.ValueAt(0) == 0)
        {
            Assert.True(Curve.Equivalent(conv_asIs, min_asIs));
        }
        else
        {
            Assert.False(Curve.Equivalent(conv_asIs, min_asIs));
        }

        var ca = new ConcaveCurve(a.WithZeroOrigin());
        var cb = new ConcaveCurve(b.WithZeroOrigin());
        var conv_conc = Curve.Convolution(ca, cb);
        var min_conc = Curve.Minimum(ca, cb);
        Assert.Equal(conv_conc, min_conc);
    }
}