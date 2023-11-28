using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="StepCurve"/>.
/// </summary>
public class StepCurveSystemJsonConverter : JsonConverter<StepCurve>
{
    // ugly hack?
    record PlainStepCurve(string type, Rational value, Rational stepTime);
    
    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override StepCurve Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var plain = JsonSerializer.Deserialize<PlainStepCurve>(ref reader);
        if (plain?.type != StepCurve.TypeCode)
            throw new JsonException();
        return new StepCurve(plain.value, plain.stepTime);
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        StepCurve value,
        JsonSerializerOptions options)
    {
        var plain = new PlainStepCurve(StepCurve.TypeCode, value.Value, value.StepTime);
        JsonSerializer.Serialize(writer, plain, options);
    }
}