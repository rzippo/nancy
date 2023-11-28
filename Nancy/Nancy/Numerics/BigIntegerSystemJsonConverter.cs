using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Unipi.Nancy.Numerics;

/// <exclude />
/// <summary>
/// Custom System.Text.Json JsonConverter for <see cref="Rational"/>.
/// </summary>
public class BigIntegerSystemJsonConverter : JsonConverter<BigInteger>
{
    /// <inheritdoc cref="JsonConverter{T}.Read"/>
    public override BigInteger Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.TokenType switch
        {
            JsonTokenType.Number when reader.TryGetInt64(out long l) => new BigInteger(l),
            JsonTokenType.String => BigInteger.Parse(reader.GetString()!),
            _ => throw new JsonException()
        };
        return value;
    }

    /// <inheritdoc cref="JsonConverter{T}.Write"/>
    public override void Write(
        Utf8JsonWriter writer,
        BigInteger value,
        JsonSerializerOptions options)
    {
        if(value.GetBitLength() < 64)
            writer.WriteNumberValue((long) value);
        else
            writer.WriteStringValue(value.ToString());
    }
}