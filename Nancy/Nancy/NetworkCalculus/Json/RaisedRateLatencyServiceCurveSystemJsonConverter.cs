using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="RaisedRateLatencyServiceCurve"/>.
/// </summary>
public class RaisedRateLatencyServiceCurveSystemJsonConverter : JsonConverter<RaisedRateLatencyServiceCurve>
{
    // ugly hack?
    record PlainRaisedRateLatencyServiceCurve(string type, Rational rate, Rational latency, Rational bufferShift);
    
    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override RaisedRateLatencyServiceCurve Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var plain = JsonSerializer.Deserialize<PlainRaisedRateLatencyServiceCurve>(ref reader);
        if (plain?.type != RaisedRateLatencyServiceCurve.TypeCode)
            throw new JsonException();
        return new RaisedRateLatencyServiceCurve(plain.rate, plain.latency, plain.bufferShift);
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        RaisedRateLatencyServiceCurve value,
        JsonSerializerOptions options)
    {
        var plain = new PlainRaisedRateLatencyServiceCurve(RaisedRateLatencyServiceCurve.TypeCode, value.Rate, value.Latency, value.BufferShift);
        JsonSerializer.Serialize(writer, plain, options);
    }
}