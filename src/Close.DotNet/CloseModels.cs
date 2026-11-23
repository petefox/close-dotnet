using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Close.DotNet;

/// <summary>A Close response object. Projected fields can be absent.</summary>
public abstract class CloseObject
{
    /// <summary>Close object identifier.</summary>
    public string? Id { get; init; }

    /// <summary>Server properties without a dedicated SDK member, including custom.cf_ fields.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalFields { get; set; }

    /// <summary>Gets the value of a custom field specified by its cf_ identifier.</summary>
    public bool TryGetCustomField(string fieldId, out JsonElement value)
        => TryReadCustomField(AdditionalFields, fieldId, out value);

    internal static bool TryReadCustomField(
        Dictionary<string, JsonElement>? fields, string fieldId, out JsonElement value)
    {
        var name = CustomFieldName(fieldId);
        value = default;
        return fields?.TryGetValue(name, out value) == true;
    }

    internal static string CustomFieldName(string fieldId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);
        if (!fieldId.StartsWith("cf_", StringComparison.Ordinal) || fieldId.Length <= 3)
            throw new ArgumentException("Supply a Close custom field ID starting with cf_.", nameof(fieldId));
        return "custom." + fieldId;
    }
}

/// <summary>A Close object belonging to an organization.</summary>
public abstract class CloseRecord : CloseObject
{
    /// <summary>Organization owning the object.</summary>
    public string? OrganizationId { get; init; }

    /// <summary>Creation time.</summary>
    public DateTimeOffset? DateCreated { get; init; }

    /// <summary>Last modification time.</summary>
    public DateTimeOffset? DateUpdated { get; init; }

    /// <summary>User that created this record.</summary>
    public string? CreatedBy { get; init; }

    /// <summary>User that last updated this record.</summary>
    public string? UpdatedBy { get; init; }
}

/// <summary>Base request for objects with extensible fields.</summary>
public abstract class CloseWriteRequest
{
    /// <summary>Additional wire fields, flattened at the root of the request.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalFields { get; set; } = new(StringComparer.Ordinal);

    /// <summary>Adds or clears a custom field using its cf_ identifier. Use a JSON null to clear it.</summary>
    public void SetCustomField(string fieldId, JsonElement value) =>
        AdditionalFields[CloseObject.CustomFieldName(fieldId)] = value.Clone();
}

/// <summary>A page of objects returned by an offset-paginated endpoint.</summary>
/// <typeparam name="T">Type of object in the page.</typeparam>
public sealed class ClosePage<T>
{
    /// <summary>Records in the current page.</summary>
    public IReadOnlyList<T> Data { get; init; } = [];

    /// <summary>Whether the API reports another page.</summary>
    public bool HasMore { get; init; }

    /// <summary>Other response fields, such as opportunity totals.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalFields { get; set; }
}

/// <summary>A contact email address.</summary>
/// <param name="Email">Email address.</param>
/// <param name="Type">Close email label, such as office.</param>
/// <param name="IsUnsubscribed">Whether the contact has unsubscribed, when supplied by Close.</param>
public sealed record CloseEmail(string Email, string? Type = null, bool? IsUnsubscribed = null);

/// <summary>A contact phone number.</summary>
/// <param name="Phone">Phone number.</param>
/// <param name="Type">Close phone label, such as mobile.</param>
/// <param name="Country">Close country code, when supplied.</param>
public sealed record ClosePhone(string Phone, string? Type = null, string? Country = null);

/// <summary>A contact URL.</summary>
/// <param name="Url">Link URL.</param>
/// <param name="Type">Close URL label.</param>
public sealed record CloseUrl(string Url, string? Type = null);

/// <summary>A postal address on a lead.</summary>
public sealed class CloseAddress
{
    /// <summary>Address label.</summary>
    public string? Label { get; init; }
    /// <summary>First street address line.</summary>
    [JsonPropertyName("address_1")] public string? Address1 { get; init; }
    /// <summary>Second street address line.</summary>
    [JsonPropertyName("address_2")] public string? Address2 { get; init; }
    /// <summary>City.</summary>
    public string? City { get; init; }
    /// <summary>State or region.</summary>
    public string? State { get; init; }
    /// <summary>Postal code.</summary>
    [JsonPropertyName("zipcode")] public string? ZipCode { get; init; }
    /// <summary>Country code.</summary>
    public string? Country { get; init; }
}

/// <summary>A file referenced by an activity or opportunity.</summary>
/// <param name="Url">Close file URL.</param>
/// <param name="Filename">File name.</param>
/// <param name="ContentType">Media type.</param>
/// <param name="Size">File size in bytes.</param>
/// <param name="ThumbnailUrl">Preview URL, if present.</param>
public sealed record CloseAttachment(string Url, string? Filename = null, string? ContentType = null,
    long? Size = null, string? ThumbnailUrl = null);

internal sealed class CloseTimestampConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.String &&
        DateTimeOffset.TryParse(reader.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var value)
            ? value : throw new JsonException("Expected a Close timestamp.");

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
