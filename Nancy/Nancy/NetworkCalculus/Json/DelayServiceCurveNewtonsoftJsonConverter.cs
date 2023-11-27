using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom Newtonsoft.Json JsonConverter for <see cref="DelayServiceCurve"/>.
/// </summary>
public class DelayServiceCurveNewtonsoftJsonConverter : JsonConverter
{
    public const string TypeName = "type";

    public static readonly string DelayName = nameof(DelayServiceCurve.Delay).ToLower();

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(DelayServiceCurve));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        Rational delay = jo[DelayName]!.ToObject<Rational>(serializer);

        DelayServiceCurve curve = new DelayServiceCurve(
            delay: delay
        );
        return curve;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        DelayServiceCurve curve = (DelayServiceCurve) value;

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(DelayServiceCurve.TypeCode, serializer) },
            { DelayName, JToken.FromObject(curve.Delay, serializer) }
        };

        jo.WriteTo(writer);
    }
}