using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.MinPlusAlgebra.Json;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus.Json;

/// <exclude />
/// <summary>
/// Custom Newtonsoft.Json JsonConverter for <see cref="ConcaveCurve"/>.
/// </summary>
public class ConcaveCurveNewtonsoftJsonConverter : JsonConverter
{
    private const string TypeName = "type";

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(ConcaveCurve));
    }

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        Sequence sequence = jo.RequireNonNull<Sequence>(CurveNewtonsoftJsonConverter.BaseSequenceName, "ConcaveCurve");
        Rational periodStart = jo.RequireNonNull<Rational>(CurveNewtonsoftJsonConverter.PseudoPeriodStartName, "ConcaveCurve");
        Rational periodLength = jo.RequireNonNull<Rational>(CurveNewtonsoftJsonConverter.PseudoPeriodLengthName, "ConcaveCurve");
        Rational periodHeight = jo.RequireNonNull<Rational>(CurveNewtonsoftJsonConverter.PseudoPeriodHeightName, "ConcaveCurve");

        ConcaveCurve curve = new ConcaveCurve(
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
        ConcaveCurve curve = (ConcaveCurve) value;

        serializer.Converters.Add(new RationalNewtonsoftJsonConverter());

        JObject jo = new JObject
        {
            { TypeName, JToken.FromObject(ConcaveCurve.TypeCode, serializer) },
            { CurveNewtonsoftJsonConverter.BaseSequenceName, JToken.FromObject(curve.BaseSequence, serializer) },
            { CurveNewtonsoftJsonConverter.PseudoPeriodStartName, JToken.FromObject(curve.PseudoPeriodStart, serializer) },
            { CurveNewtonsoftJsonConverter.PseudoPeriodLengthName, JToken.FromObject(curve.PseudoPeriodLength, serializer) },
            { CurveNewtonsoftJsonConverter.PseudoPeriodHeightName, JToken.FromObject(curve.PseudoPeriodHeight, serializer) }
        };

        jo.WriteTo(writer);
    }
}