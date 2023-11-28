using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="ConvexCurve"/>.
/// </summary>
public class ConvexCurveSystemJsonConverter : JsonConverter<ConvexCurve>
{
    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override ConvexCurve Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var plain = JsonSerializer.Deserialize<GenericCurveSystemJsonConverter.PlainCurve>(ref reader);
        if (plain?.type != ConvexCurve.TypeCode)
            throw new JsonException();
        return new ConvexCurve(plain.baseSequence, plain.pseudoPeriodStart, plain.pseudoPeriodLength, plain.pseudoPeriodHeight);
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        ConvexCurve value,
        JsonSerializerOptions options)
    {
        var plain = new GenericCurveSystemJsonConverter.PlainCurve(
            ConvexCurve.TypeCode, 
            value.BaseSequence, 
            value.PseudoPeriodStart, 
            value.PseudoPeriodLength, 
            value.PseudoPeriodHeight
        );
        JsonSerializer.Serialize(writer, plain, options);
    }
}