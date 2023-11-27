using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom JsonConverter for <see cref="StairCurve"/>.
/// </summary>
public class StairCurveNewtonsoftJsonConverter : JsonConverter
{
    private const string TypeName = "type";

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

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

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

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(StairCurve.TypeCode, serializer) },
            { AName, JToken.FromObject(curve.A, serializer) },
            { BName, JToken.FromObject(curve.B, serializer) }
        };

        jo.WriteTo(writer);
    }
}