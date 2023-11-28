using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="TwoRatesServiceCurve"/>.
/// </summary>
public class TwoRatesServiceCurveSystemJsonConverter : JsonConverter<TwoRatesServiceCurve>
{
    // ugly hack?
    record PlainTwoRatesServiceCurve(string type, Rational delay, Rational transientRate, Rational transientEnd, Rational steadyRate);
    
    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override TwoRatesServiceCurve Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var plain = JsonSerializer.Deserialize<PlainTwoRatesServiceCurve>(ref reader);
        if (plain?.type != TwoRatesServiceCurve.TypeCode)
            throw new JsonException();
        return new TwoRatesServiceCurve(plain.delay, plain.transientRate, plain.transientEnd, plain.steadyRate);
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        TwoRatesServiceCurve value,
        JsonSerializerOptions options)
    {
        var plain = new PlainTwoRatesServiceCurve(TwoRatesServiceCurve.TypeCode, value.Delay, value.TransientRate, value.TransientEnd, value.SteadyRate);
        JsonSerializer.Serialize(writer, plain, options);
    }
}