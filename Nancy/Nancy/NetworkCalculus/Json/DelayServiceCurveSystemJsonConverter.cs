using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="DelayServiceCurve"/>.
/// </summary>
public class DelayServiceCurveSystemJsonConverter : JsonConverter<DelayServiceCurve>
{
    // ugly hack?
    internal record PlainDelayServiceCurve(string type, Rational delay);

    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override DelayServiceCurve Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var plain = JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.PlainDelayServiceCurve);
        if (plain?.type != DelayServiceCurve.TypeCode)
            throw new JsonException();
        return new DelayServiceCurve(plain.delay);
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        DelayServiceCurve value,
        JsonSerializerOptions options)
    {
        var plain = new PlainDelayServiceCurve(DelayServiceCurve.TypeCode, value.Delay);
        JsonSerializer.Serialize(writer, plain, NancyJsonSerializerContext.Default.PlainDelayServiceCurve);
    }
}