using System;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Unipi.Nancy.Numerics;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="BigRational"/>, writing it as <see cref="RationalSystemJsonConverter"/> writes a <see cref="Rational"/>.
/// </summary>
public class BigRationalSystemJsonConverter : JsonConverter<BigRational>
{
    /// <summary>
    /// Proxy record for the serialization of <see cref="BigRational"/>.
    /// </summary>
    public record PlainBigRational(
        [property: JsonConverter(typeof(BigIntegerSystemJsonConverter))]
        BigInteger num,
        [property: JsonConverter(typeof(BigIntegerSystemJsonConverter))]
        BigInteger den
    );

    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override BigRational Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
            {
                if (reader.TryGetInt32(out int i))
                    return new BigRational(i);
                else if (reader.TryGetInt64(out long l))
                    return new BigRational(l);
                else if (reader.TryGetDecimal(out decimal d))
                    return new BigRational(d);
                else
                    throw new JsonException($"Could not parse BigRational from: {Encoding.UTF8.GetString(reader.ValueSpan)}");
            }

            case JsonTokenType.String:
            {
                if (BigInteger.TryParse(reader.GetString(), out BigInteger bi))
                    return new BigRational(bi);
                else
                    throw new JsonException($"Could not parse BigRational from: {reader.GetString()}");
            }

            default:
            {
                var plain = JsonSerializer.Deserialize(ref reader, NancyJsonSerializerContext.Default.PlainBigRational);
                if (plain == null)
                    throw new JsonException("BigRational cannot be deserialized: JSON value is null.");

                return new BigRational(plain.num, plain.den);
            }
        }
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        BigRational value,
        JsonSerializerOptions options)
    {
        if (value.Denominator == 1)
            new BigIntegerSystemJsonConverter().Write(writer, value.Numerator, options);
        else
        {
            var plain = new PlainBigRational(value.Numerator, value.Denominator);
            JsonSerializer.Serialize(writer, plain, NancyJsonSerializerContext.Default.PlainBigRational);
        }
    }
}
