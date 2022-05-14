using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom JsonConverter for <see cref="DelayServiceCurve"/>.
/// </summary>
public class DelayServiceCurveConverter : JsonConverter
{
    private const string TypeName = "type";

    /// <summary>
    /// Code used in JSON output to distinguish this type 
    /// </summary>
    public const string TypeCode = "delayServiceCurve";

    private static readonly string DelayName = "delay";
        
    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(DelayServiceCurve));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        Rational delay = jo[DelayName]!.ToObject<Rational>();

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

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(TypeCode) },
            { DelayName, JToken.FromObject(curve.Delay) }
        };

        jo.WriteTo(writer);
    }
}