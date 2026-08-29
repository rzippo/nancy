using System.Text.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests;

// NancyJsonSerializerContext went from internal to public.
// These pin that it is reachable from outside the assembly, and that going through it round-trips correctly.
public class PublicSerializerContextTests
{
    [Fact]
    public void RationalRoundTripsThroughTheContext()
    {
        var value = new Rational(3, 4);

        var serialization = JsonSerializer.Serialize(value, NancyJsonSerializerContext.Default.Rational);
        var deserialized = JsonSerializer.Deserialize(serialization, NancyJsonSerializerContext.Default.Rational);

        Assert.Equal(value, deserialized);
    }

    [Fact]
    public void CurveRoundTripsThroughTheContext()
    {
        Curve value = new RateLatencyServiceCurve(rate: 20, latency: 10);

        var serialization = JsonSerializer.Serialize(value, NancyJsonSerializerContext.Default.Curve);
        var deserialized = JsonSerializer.Deserialize(serialization, NancyJsonSerializerContext.Default.Curve);

        Assert.Equal(value, deserialized);
    }

    [Fact]
    public void RateLatencyServiceCurveRoundTripsThroughItsOwnContextEntry()
    {
        var value = new RateLatencyServiceCurve(rate: 20, latency: 10);

        var serialization = JsonSerializer.Serialize(value, NancyJsonSerializerContext.Default.RateLatencyServiceCurve);
        var deserialized = JsonSerializer.Deserialize(serialization, NancyJsonSerializerContext.Default.RateLatencyServiceCurve);

        Assert.Equal(value, deserialized);
    }
}
