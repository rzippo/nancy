using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom JsonConverter for <see cref="StairCurve"/>.
/// </summary>
public class StairCurveConverter : JsonConverter
{
    private const string TypeName = "type";

    /// <summary>
    /// Code used in JSON output to distinguish this type 
    /// </summary>
    public const string TypeCode = "stairCurve";

    private static readonly string AName = "a";
    private static readonly string BName = "b";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(StairCurve));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        serializer.Converters.Add(new RationalConverter());

        Rational a = jo[AName]!.ToObject<Rational>(serializer);
        Rational b = jo[BName]!.ToObject<Rational>(serializer);

        StairCurve curve = new StairCurve(
            a: a,
            b: b
        );
        return curve;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        StairCurve curve = (StairCurve) value;

        serializer.Converters.Add(new RationalConverter());

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(TypeCode, serializer) },
            { AName, JToken.FromObject(curve.A, serializer) },
            { BName, JToken.FromObject(curve.B, serializer) }
        };

        jo.WriteTo(writer);
    }
}