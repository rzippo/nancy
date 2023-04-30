using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom JsonConverter for <see cref="StepCurve"/>.
/// </summary>
public class StepCurveConverter : JsonConverter
{
    private const string TypeName = "type";

    /// <summary>
    /// Code used in JSON output to distinguish this type 
    /// </summary>
    public const string TypeCode = "stepCurve";

    private static readonly string ValueName = "value";
    private static readonly string StepTimeName = "stepTime";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(StepCurve));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        serializer.Converters.Add(new RationalConverter());

        Rational value = jo[ValueName]!.ToObject<Rational>(serializer);
        Rational stepTime = jo[StepTimeName]!.ToObject<Rational>(serializer);

        StepCurve curve = new StepCurve(
            value: value,
            stepTime: stepTime
        );
        return curve;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        StepCurve curve = (StepCurve) value;

        serializer.Converters.Add(new RationalConverter());

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(TypeCode, serializer) },
            { ValueName, JToken.FromObject(curve.Value, serializer) },
            { StepTimeName, JToken.FromObject(curve.StepTime, serializer) }
        };

        jo.WriteTo(writer);
    }
}