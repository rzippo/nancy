using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus;

public class ConcaveCurveTests
{
    public static IEnumerable<object[]> GetPairTestCases()
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

    public static IEnumerable<object[]> GetSingleTestCases()
    {

        foreach (var pair in GetPairTestCases())
        {
            yield return new object[] { pair[0] }; // Curve a
            yield return new object[] { pair[1] }; // Curve b
        }
    }

    [Theory]
    [MemberData(nameof(GetSingleTestCases))]
    public void PropertiesTest(Curve c)
    {
        Assert.True(c.IsConcave);

        Assert.True(c.WithZeroOrigin().IsConcave);
        Assert.True(c.WithZeroOrigin().IsRegularConcave);

        var c_rc = c.Maximum(new Point(0, c.RightLimitAt(0)));
        Assert.True(c_rc.IsConcave);
        Assert.Equal(c.RightLimitAt(0) == 0, c_rc.IsRegularConcave);

        var c_rc_1 = c.Maximum(new Point(0, c.RightLimitAt(0) + 1));
        Assert.False(c_rc_1.IsConcave);
        Assert.False(c_rc_1.IsRegularConcave);
    }

    [Theory]
    [MemberData(nameof(GetPairTestCases))]
    public void ConvolutionEquivalenceTest(Curve a, Curve b)
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