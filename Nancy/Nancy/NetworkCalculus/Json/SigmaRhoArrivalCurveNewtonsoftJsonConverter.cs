using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom Newtonsoft.Json JsonConverter for <see cref="SigmaRhoArrivalCurve"/>.
/// </summary>
public class SigmaRhoArrivalCurveNewtonsoftJsonConverter : JsonConverter
{
    private const string TypeName = "type";

    private static readonly string SigmaName = "sigma";
    private static readonly string RhoName = "rho";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(SigmaRhoArrivalCurve));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        Rational sigma = jo[SigmaName]!.ToObject<Rational>(serializer);
        Rational rho = jo[RhoName]!.ToObject<Rational>(serializer);

        SigmaRhoArrivalCurve curve = new SigmaRhoArrivalCurve(
            sigma: sigma,
            rho: rho
        );
        return curve;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        SigmaRhoArrivalCurve curve = (SigmaRhoArrivalCurve) value;

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(SigmaRhoArrivalCurve.TypeCode, serializer) },
            { SigmaName, JToken.FromObject(curve.Sigma, serializer) },
            { RhoName, JToken.FromObject(curve.Rho, serializer) }
        };

        jo.WriteTo(writer);
    }
}