using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="SigmaRhoArrivalCurve"/>.
/// </summary>
public class SigmaRhoArrivalCurveSystemJsonConverter : JsonConverter<SigmaRhoArrivalCurve>
{
    // ugly hack?
    record PlainSigmaRhoArrivalCurve(string type, Rational sigma, Rational rho);
    
    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override SigmaRhoArrivalCurve Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var plain = JsonSerializer.Deserialize<PlainSigmaRhoArrivalCurve>(ref reader);
        if (plain?.type != SigmaRhoArrivalCurve.TypeCode)
            throw new JsonException();
        return new SigmaRhoArrivalCurve(plain.sigma, plain.rho);
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        SigmaRhoArrivalCurve value,
        JsonSerializerOptions options)
    {
        var plain = new PlainSigmaRhoArrivalCurve(SigmaRhoArrivalCurve.TypeCode, value.Sigma, value.Rho);
        JsonSerializer.Serialize(writer, plain, options);
    }
}