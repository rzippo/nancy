using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom Newtonsoft.Json JsonConverter for <see cref="ConstantCurve"/>.
/// </summary>
public class ConstantCurveNewtonsoftJsonConverter : JsonConverter
{
    public const string TypeName = "type";

    public static readonly string ValueName = "value";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(ConstantCurve));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        Rational value = jo[ValueName]!.ToObject<Rational>(serializer);

        ConstantCurve curve = new ConstantCurve(
            value: value
        );
        return curve;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        ConstantCurve curve = (ConstantCurve) value;

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(ConstantCurve.TypeCode, serializer) },
            { ValueName, JToken.FromObject(curve.Value, serializer) }
        };

        jo.WriteTo(writer);
    }
}