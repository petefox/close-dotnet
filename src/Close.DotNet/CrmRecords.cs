using System.Text.Json;
using System.Text.Json.Serialization;

namespace Close.DotNet;

/// <summary>A Close lead.</summary>
public sealed class Lead : CloseRecord
{
    /// <summary>Name.</summary>
    public string? Name { get; init; }

    /// <summary>Display Name.</summary>
    public string? DisplayName { get; init; }

    /// <summary>Description.</summary>
    public string? Description { get; init; }

    /// <summary>Url.</summary>
    public string? Url { get; init; }

    /// <summary>Html Url.</summary>
    public string? HtmlUrl { get; init; }

    /// <summary>Close status identifier.</summary>
    public string? StatusId { get; init; }

    /// <summary>Status Label.</summary>
    public string? StatusLabel { get; init; }

    /// <summary>Contact Ids.</summary>
    public IReadOnlyList<string>? ContactIds { get; init; }

    /// <summary>Contacts.</summary>
    public IReadOnlyList<Contact>? Contacts { get; init; }

    /// <summary>Addresses.</summary>
    public IReadOnlyList<CloseAddress>? Addresses { get; init; }

    /// <summary>Opportunities.</summary>
    public IReadOnlyList<Opportunity>? Opportunities { get; init; }

    /// <summary>Tasks.</summary>
    public IReadOnlyList<CloseTask>? Tasks { get; init; }

    /// <summary>Primary Email.</summary>
    public string? PrimaryEmail { get; init; }

    /// <summary>Primary Phone.</summary>
    public string? PrimaryPhone { get; init; }
}

/// <summary>A Close contact.</summary>
public sealed class Contact : CloseRecord
{
    /// <summary>Identifier of the associated lead.</summary>
    public string? LeadId { get; init; }

    /// <summary>Name.</summary>
    public string? Name { get; init; }

    /// <summary>Display Name.</summary>
    public string? DisplayName { get; init; }

    /// <summary>Title.</summary>
    public string? Title { get; init; }

    /// <summary>Timezone.</summary>
    public string? Timezone { get; init; }

    /// <summary>Emails.</summary>
    public IReadOnlyList<CloseEmail>? Emails { get; init; }

    /// <summary>Phones.</summary>
    public IReadOnlyList<ClosePhone>? Phones { get; init; }

    /// <summary>Urls.</summary>
    public IReadOnlyList<CloseUrl>? Urls { get; init; }
}

/// <summary>A Close opportunity.</summary>
public sealed class Opportunity : CloseRecord
{
    /// <summary>Identifier of the associated lead.</summary>
    public string? LeadId { get; init; }

    /// <summary>Identifier of the associated contact.</summary>
    public string? ContactId { get; init; }

    /// <summary>Pipeline Id.</summary>
    public string? PipelineId { get; init; }

    /// <summary>Close status identifier.</summary>
    public string? StatusId { get; init; }

    /// <summary>Status Label.</summary>
    public string? StatusLabel { get; init; }

    /// <summary>Status Type.</summary>
    public string? StatusType { get; init; }

    /// <summary>User Id.</summary>
    public string? UserId { get; init; }

    /// <summary>Estimated win probability from 0 to 100.</summary>
    public int? Confidence { get; init; }

    /// <summary>Opportunity amount in the currency's minor units.</summary>
    public long? Value { get; init; }

    /// <summary>Value Currency.</summary>
    public string? ValueCurrency { get; init; }

    /// <summary>Close value period, such as one_time, monthly, or annual.</summary>
    public string? ValuePeriod { get; init; }

    /// <summary>Note.</summary>
    public string? Note { get; init; }

    /// <summary>HTML body of the opportunity note.</summary>
    public string? NoteHtml { get; init; }

    /// <summary>Date Won.</summary>
    public DateOnly? DateWon { get; init; }

    /// <summary>Date Lost.</summary>
    public DateTimeOffset? DateLost { get; init; }

    /// <summary>Attachments.</summary>
    public IReadOnlyList<CloseAttachment>? Attachments { get; init; }
}

/// <summary>A Close task.</summary>
public sealed class CloseTask : CloseRecord
{
    /// <summary>Close object type discriminator.</summary>
    [JsonPropertyName("_type")]
    public string? Type { get; init; }

    /// <summary>Identifier of the associated lead.</summary>
    public string? LeadId { get; init; }

    /// <summary>Identifier of the associated contact.</summary>
    public string? ContactId { get; init; }

    /// <summary>Assigned To.</summary>
    public string? AssignedTo { get; init; }

    /// <summary>Plain-text contents.</summary>
    public string? Text { get; init; }

    /// <summary>Is Complete.</summary>
    public bool? IsComplete { get; init; }

    /// <summary>Is Dateless.</summary>
    public bool? IsDateless { get; init; }

    /// <summary>Task date in the format expected by Close.</summary>
    public string? Date { get; init; }

    /// <summary>Task due date in the format expected by Close.</summary>
    public string? DueDate { get; init; }

    /// <summary>Object Id.</summary>
    public string? ObjectId { get; init; }

    /// <summary>Object Type.</summary>
    public string? ObjectType { get; init; }

    /// <summary>Priority.</summary>
    public string? Priority { get; init; }

    /// <summary>Resolution.</summary>
    public string? Resolution { get; init; }
}

/// <summary>A Close note.</summary>
public sealed class Note : CloseRecord
{
    /// <summary>Close object type discriminator.</summary>
    [JsonPropertyName("_type")]
    public string? Type { get; init; }

    /// <summary>Identifier of the associated lead.</summary>
    public string? LeadId { get; init; }

    /// <summary>Identifier of the associated contact.</summary>
    public string? ContactId { get; init; }

    /// <summary>Plain-text contents.</summary>
    [JsonPropertyName("note")]
    public string? Text { get; init; }

    /// <summary>HTML body of the note.</summary>
    [JsonPropertyName("note_html")]
    public string? Html { get; init; }

    /// <summary>Title.</summary>
    public string? Title { get; init; }

    /// <summary>Time of the activity.</summary>
    public DateTimeOffset? ActivityAt { get; init; }

    /// <summary>Pinned.</summary>
    public bool? Pinned { get; init; }

    /// <summary>Attachments.</summary>
    public IReadOnlyList<CloseAttachment>? Attachments { get; init; }
}

/// <summary>A Close user.</summary>
public sealed class CloseUser : CloseRecord
{
    /// <summary>Email.</summary>
    public string? Email { get; init; }

    /// <summary>First Name.</summary>
    public string? FirstName { get; init; }

    /// <summary>Last Name.</summary>
    public string? LastName { get; init; }

    /// <summary>Last Used Timezone.</summary>
    public string? LastUsedTimezone { get; init; }

    /// <summary>Organizations.</summary>
    public IReadOnlyList<CloseOrganization>? Organizations { get; init; }

    /// <summary>Memberships.</summary>
    public IReadOnlyList<CloseMembership>? Memberships { get; init; }
}

/// <summary>A Close organization.</summary>
public sealed class CloseOrganization : CloseRecord
{
    /// <summary>Name.</summary>
    public string? Name { get; init; }

    /// <summary>Timezone.</summary>
    public string? Timezone { get; init; }
}

/// <summary>A Close membership.</summary>
public sealed class CloseMembership : CloseRecord
{
    /// <summary>Role Id.</summary>
    public string? RoleId { get; init; }

    /// <summary>User Id.</summary>
    public string? UserId { get; init; }

    /// <summary>Permissions Granted.</summary>
    public IReadOnlyList<string>? PermissionsGranted { get; init; }
}

/// <summary>A Close event.</summary>
public sealed class CloseEvent : CloseRecord
{
    /// <summary>Action.</summary>
    public string? Action { get; init; }

    /// <summary>Object Type.</summary>
    public string? ObjectType { get; init; }

    /// <summary>Object Id.</summary>
    public string? ObjectId { get; init; }

    /// <summary>Identifier of the associated lead.</summary>
    public string? LeadId { get; init; }

    /// <summary>User Id.</summary>
    public string? UserId { get; init; }

    /// <summary>Request Id.</summary>
    public string? RequestId { get; init; }

    /// <summary>Changed Fields.</summary>
    public IReadOnlyList<string>? ChangedFields { get; init; }

    /// <summary>Current event object data, in its resource-specific shape.</summary>
    public JsonElement? Data { get; init; }

    /// <summary>Previous values supplied for this event.</summary>
    public JsonElement? PreviousData { get; init; }

    /// <summary>Additional event metadata.</summary>
    public JsonElement? Meta { get; init; }
}

/// <summary>A Close webhook subscription.</summary>
public sealed class WebhookSubscription : CloseRecord
{
    /// <summary>Url.</summary>
    public string? Url { get; init; }

    /// <summary>Status.</summary>
    public string? Status { get; init; }

    /// <summary>Whether Close verifies the delivery endpoint’s TLS certificate.</summary>
    public bool? VerifySsl { get; init; }

    /// <summary>Webhook event subscriptions.</summary>
    public IReadOnlyList<WebhookEventFilter>? Events { get; init; }

    /// <summary>Hex-encoded signing secret returned by Close.</summary>
    public string? SignatureKey { get; init; }

    /// <summary>Counters.</summary>
    public JsonElement? Counters { get; init; }
}
