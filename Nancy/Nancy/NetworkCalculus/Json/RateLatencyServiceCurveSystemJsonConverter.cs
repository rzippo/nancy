using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="RateLatencyServiceCurve"/>.
/// </summary>
public class RateLatencyServiceCurveSystemJsonConverter : JsonConverter<RateLatencyServiceCurve>
{
    // ugly hack?
    internal record PlainRateLatencyServiceCurve(string type, Rational rate, Rational latency);

    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override RateLatencyServiceCurve Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var plain = JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.PlainRateLatencyServiceCurve);
        if (plain?.type != RateLatencyServiceCurve.TypeCode)
            throw new JsonException();
        return new RateLatencyServiceCurve(plain.rate, plain.latency);
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        RateLatencyServiceCurve value,
        JsonSerializerOptions options)
    {
        var plain = new PlainRateLatencyServiceCurve(RateLatencyServiceCurve.TypeCode, value.Rate, value.Latency);
        JsonSerializer.Serialize(writer, plain, NancyJsonSerializerContext.Default.PlainRateLatencyServiceCurve);
    }
}