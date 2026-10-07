using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Unipi.Nancy.Numerics;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="LongRational"/>, writing it as <see cref="RationalSystemJsonConverter"/> writes a <see cref="Rational"/>.
/// </summary>
public class LongRationalSystemJsonConverter : JsonConverter<LongRational>
{
    /// <summary>
    /// Proxy record for the serialization of <see cref="LongRational"/>.
    /// </summary>
    public record PlainLongRational(long num, long den);

    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override LongRational Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
            {
                if (reader.TryGetInt32(out int i))
                    return new LongRational(i);
                else if (reader.TryGetInt64(out long l))
                    return new LongRational(l);
                else if (reader.TryGetDecimal(out decimal d))
                    return new LongRational(d);
                else
                    throw new JsonException($"Could not parse LongRational from: {Encoding.UTF8.GetString(reader.ValueSpan)}");
            }

            default:
            {
                var plain = JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.PlainLongRational);
                if (plain == null)
                    throw new JsonException("LongRational cannot be deserialized: JSON value is null.");

                return new LongRational(plain.num, plain.den);
            }
        }
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        LongRational value,
        JsonSerializerOptions options)
    {
        if (value.Denominator == 1)
            writer.WriteNumberValue(value.Numerator);
        else
        {
            var plain = new PlainLongRational(value.Numerator, value.Denominator);
            JsonSerializer.Serialize(writer, plain, NancyJsonSerializerContext.Default.PlainLongRational);
        }
    }
}
