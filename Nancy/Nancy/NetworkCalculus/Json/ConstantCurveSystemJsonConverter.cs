using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="ConstantCurve"/>.
/// </summary>
public class ConstantCurveSystemJsonConverter : JsonConverter<ConstantCurve>
{
    // ugly hack?
    record PlainConstantCurve(string type, Rational value);
    
    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override ConstantCurve Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var plain = JsonSerializer.Deserialize<PlainConstantCurve>(ref reader);
        if (plain.type != ConstantCurve.TypeCode)
            throw new JsonException();
        return new ConstantCurve(plain.value);
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        ConstantCurve value,
        JsonSerializerOptions options)
    {
        var plain = new PlainConstantCurve(ConstantCurve.TypeCode, value.Value);
        JsonSerializer.Serialize(writer, plain, options);
    }
}