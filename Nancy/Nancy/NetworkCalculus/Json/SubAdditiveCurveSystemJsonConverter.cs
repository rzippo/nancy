using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="SubAdditiveCurve"/>.
/// </summary>
public class SubAdditiveCurveSystemJsonConverter : JsonConverter<SubAdditiveCurve>
{
    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override SubAdditiveCurve Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var plain = JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.PlainCurve);
        if (plain?.type != SubAdditiveCurve.TypeCode)
            throw new JsonException();
        return new SubAdditiveCurve(plain.baseSequence, plain.pseudoPeriodStart, plain.pseudoPeriodLength, plain.pseudoPeriodHeight);
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        SubAdditiveCurve value,
        JsonSerializerOptions options)
    {
        var plain = new GenericCurveSystemJsonConverter.PlainCurve(
            SubAdditiveCurve.TypeCode, 
            value.BaseSequence, 
            value.PseudoPeriodStart, 
            value.PseudoPeriodLength, 
            value.PseudoPeriodHeight
        );
        JsonSerializer.Serialize(writer, plain, NancyJsonSerializerContext.Default.PlainCurve);
    }
}