using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom JsonConverter for <see cref="RateLatencyServiceCurve"/>.
/// </summary>
public class RateLatencyServiceCurveConverter : JsonConverter
{
    private const string TypeName = "type";

    /// <summary>
    /// Code used in JSON output to distinguish this type 
    /// </summary>
    public const string TypeCode = "rateLatencyServiceCurve";

    private static readonly string LatencyName = "latency";
    private static readonly string RateName = "rate";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(RateLatencyServiceCurve));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        serializer.Converters.Add(new RationalConverter());

        Rational latency = jo[LatencyName]!.ToObject<Rational>(serializer);
        Rational rate = jo[RateName]!.ToObject<Rational>(serializer);

        RateLatencyServiceCurve curve = new RateLatencyServiceCurve(rate: rate, latency: latency);
        return curve;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        RateLatencyServiceCurve curve = (RateLatencyServiceCurve) value;

        serializer.Converters.Add(new RationalConverter());

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(TypeCode, serializer) },
            { RateName, JToken.FromObject(curve.Rate, serializer) },
            { LatencyName, JToken.FromObject(curve.Latency, serializer) }
        };

        jo.WriteTo(writer);
    }
}