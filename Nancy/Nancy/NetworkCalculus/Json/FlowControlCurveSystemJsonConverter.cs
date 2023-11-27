using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="FlowControlCurve"/>.
/// </summary>
public class FlowControlCurveSystemJsonConverter : JsonConverter<FlowControlCurve>
{
    // ugly hack?
    record PlainFlowControlCurve(string type, Rational latency, Rational rate, Rational height);
    
    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override FlowControlCurve Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var plain = JsonSerializer.Deserialize<PlainFlowControlCurve>(ref reader);
        if (plain.type != FlowControlCurve.TypeCode)
            throw new JsonException();
        return new FlowControlCurve(plain.latency, plain.rate, plain.height);
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        FlowControlCurve value,
        JsonSerializerOptions options)
    {
        var plain = new PlainFlowControlCurve(FlowControlCurve.TypeCode, value.Latency, value.Rate, value.Height);
        JsonSerializer.Serialize(writer, plain, options);
    }
}