using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom JsonConverter for <see cref="FlowControlCurve"/>.
/// </summary>
public class FlowControlCurveConverter : JsonConverter
{
    private const string TypeName = "type";

    /// <summary>
    /// Code used in JSON output to distinguish this type 
    /// </summary>
    public const string TypeCode = "flowControlCurve";

    private static readonly string DelayName = "delay";
    private static readonly string RateName = "rate";
    private static readonly string HeightName = "height";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(FlowControlCurve));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        serializer.Converters.Add(new RationalConverter());

        Rational delay = jo[DelayName]!.ToObject<Rational>(serializer);
        Rational rate = jo[RateName]!.ToObject<Rational>(serializer);
        Rational height = jo[HeightName]!.ToObject<Rational>(serializer);

        FlowControlCurve curve = new FlowControlCurve(
            latency: delay,
            rate: rate,
            height: height
        );
        return curve;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        FlowControlCurve curve = (FlowControlCurve) value;

        serializer.Converters.Add(new RationalConverter());

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(TypeCode, serializer) },
            { DelayName, JToken.FromObject(curve.Latency, serializer) },
            { RateName, JToken.FromObject(curve.Rate, serializer) },
            { HeightName, JToken.FromObject(curve.Height, serializer) }
        };

        jo.WriteTo(writer);
    }
}