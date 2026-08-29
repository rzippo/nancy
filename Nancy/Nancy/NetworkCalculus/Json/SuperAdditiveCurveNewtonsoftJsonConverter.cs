using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.MinPlusAlgebra.Json;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom Newtonsoft.Json JsonConverter for <see cref="SuperAdditiveCurve"/>.
/// </summary>
public class SuperAdditiveCurveNewtonsoftJsonConverter : JsonConverter
{
    private const string TypeName = "type";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(SuperAdditiveCurve));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        Sequence sequence = jo.RequireNonNull<Sequence>(CurveNewtonsoftJsonConverter.BaseSequenceName, "SuperAdditiveCurve", serializer);
        Rational periodStart = jo.RequireNonNull<Rational>(CurveNewtonsoftJsonConverter.PseudoPeriodStartName, "SuperAdditiveCurve", serializer);
        Rational periodLength = jo.RequireNonNull<Rational>(CurveNewtonsoftJsonConverter.PseudoPeriodLengthName, "SuperAdditiveCurve", serializer);
        Rational periodHeight = jo.RequireNonNull<Rational>(CurveNewtonsoftJsonConverter.PseudoPeriodHeightName, "SuperAdditiveCurve", serializer);

        SuperAdditiveCurve curve = new SuperAdditiveCurve(
            baseSequence: sequence,
            pseudoPeriodStart: periodStart,
            pseudoPeriodLength: periodLength,
            pseudoPeriodHeight: periodHeight
        );
        return curve;
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        SuperAdditiveCurve curve = (SuperAdditiveCurve) value;

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(SuperAdditiveCurve.TypeCode, serializer) },
            { CurveNewtonsoftJsonConverter.BaseSequenceName, JToken.FromObject(curve.BaseSequence, serializer) },
            { CurveNewtonsoftJsonConverter.PseudoPeriodStartName, JToken.FromObject(curve.PseudoPeriodStart, serializer) },
            { CurveNewtonsoftJsonConverter.PseudoPeriodLengthName, JToken.FromObject(curve.PseudoPeriodLength, serializer) },
            { CurveNewtonsoftJsonConverter.PseudoPeriodHeightName, JToken.FromObject(curve.PseudoPeriodHeight, serializer) }
        };

        jo.WriteTo(writer);
    }
}