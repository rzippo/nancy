using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Unipi.Nancy.Numerics;

internal static class JsonExtensions
{
    /// <summary>
    /// The token for <paramref name="field"/> within <paramref name="token"/>.
    /// </summary>
    /// <exception cref="JsonSerializationException">
    /// <paramref name="field"/> is missing from <paramref name="token"/>, or is JSON <see langword="null"/>.
    /// </exception>
    public static JToken RequireNonNull(this JToken token, string field, string typeName)
    {
        var value = token[field];
        if (value is null || value.Type == JTokenType.Null)
            throw new JsonSerializationException($"{typeName} cannot be deserialized: {field} cannot be null.");
        return value;
    }

    /// <summary>
    /// The value of <paramref name="field"/> within <paramref name="token"/>, deserialized as <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="JToken.ToObject{T}()"/> returns <c>T?</c> for every <typeparamref name="T"/>, so deserializing at the call site leaves a reference-typed result nullable there.
    /// Deserializing here keeps it non-nullable, and the one case a call site could not have ruled out is answered once, below.
    /// </remarks>
    /// <exception cref="JsonSerializationException">
    /// <paramref name="field"/> is missing from <paramref name="token"/>, is JSON <see langword="null"/>,
    /// or does not deserialize to a <typeparamref name="T"/>.
    /// </exception>
    public static T RequireNonNull<T>(this JToken token, string field, string typeName, JsonSerializer? serializer = null)
    {
        var value = token.RequireNonNull(field, typeName);
        var deserialized = serializer is null ? value.ToObject<T>() : value.ToObject<T>(serializer);
        if (deserialized is null)
            throw new JsonSerializationException(
                $"{typeName} cannot be deserialized: {field} is not a {typeof(T).Name}.");
        return deserialized;
    }
}
