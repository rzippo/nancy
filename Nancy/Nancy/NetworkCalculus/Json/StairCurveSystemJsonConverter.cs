using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="StairCurve"/>.
/// </summary>
public class StairCurveSystemJsonConverter : JsonConverter<StairCurve>
{
    // ugly hack?
    record PlainStairCurve(string type, Rational a, Rational b);
    
    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override StairCurve Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var plain = JsonSerializer.Deserialize<PlainStairCurve>(ref reader);
        if (plain?.type != StairCurve.TypeCode)
            throw new JsonException();
        return new StairCurve(plain.a, plain.b);
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        StairCurve value,
        JsonSerializerOptions options)
    {
        var plain = new PlainStairCurve(StairCurve.TypeCode, value.A, value.B);
        JsonSerializer.Serialize(writer, plain, options);
    }
}