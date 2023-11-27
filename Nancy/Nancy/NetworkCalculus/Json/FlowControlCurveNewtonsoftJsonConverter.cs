using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom JsonConverter for <see cref="FlowControlCurve"/>.
/// </summary>
public class FlowControlCurveNewtonsoftJsonConverter : JsonConverter
{
    private const string TypeName = "type";

    private static readonly string LatencyName = nameof(FlowControlCurve.Latency).ToLower();
    private static readonly string RateName = nameof(FlowControlCurve.Rate).ToLower();
    private static readonly string HeightName = nameof(FlowControlCurve.Height).ToLower();

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(FlowControlCurve));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        Rational latency = jo[LatencyName]!.ToObject<Rational>(serializer);
        Rational rate = jo[RateName]!.ToObject<Rational>(serializer);
        Rational height = jo[HeightName]!.ToObject<Rational>(serializer);

        FlowControlCurve curve = new FlowControlCurve(
            latency,
            rate,
            height
        );
        return curve;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        FlowControlCurve curve = (FlowControlCurve) value;

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(FlowControlCurve.TypeCode, serializer) },
            { LatencyName, JToken.FromObject(curve.Latency, serializer) },
            { RateName, JToken.FromObject(curve.Rate, serializer) },
            { HeightName, JToken.FromObject(curve.Height, serializer) }
        };

        jo.WriteTo(writer);
    }
}