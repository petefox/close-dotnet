using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Close.DotNet;

/// <summary>Distinguishes an omitted update field from a field explicitly set to a value or null.</summary>
/// <typeparam name="T">The field value type.</typeparam>
public readonly struct CloseField<T>
{
    private readonly T? value;

    private CloseField(T? value)
    {
        this.value = value;
        IsSpecified = true;
    }

    /// <summary>Whether to include this field in the request.</summary>
    public bool IsSpecified { get; }

    /// <summary>The supplied value, or the default value if unspecified.</summary>
    public T? Value => value;

    /// <summary>Includes the supplied value in the request, even when it is null.</summary>
    public static CloseField<T> Set(T? value) => new(value);

    /// <summary>Includes the supplied value in the request, even when it is null.</summary>
    public static implicit operator CloseField<T>(T? value) => Set(value);
}

internal sealed class CloseFieldConverter<T> : JsonConverter<CloseField<T>>
{
    public override CloseField<T> Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) =>
        CloseField<T>.Set(JsonSerializer.Deserialize(ref reader, TypeInfo(options)));

    public override void Write(Utf8JsonWriter writer, CloseField<T> value, JsonSerializerOptions options)
    {
        if (value.Value is { } included)
            JsonSerializer.Serialize<T>(writer, included, TypeInfo(options));
        else
            writer.WriteNullValue();
    }

    private static JsonTypeInfo<T> TypeInfo(JsonSerializerOptions options) =>
        (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T));
}
