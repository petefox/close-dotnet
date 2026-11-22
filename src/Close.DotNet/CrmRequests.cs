using System.Text.Json;
using System.Text.Json.Serialization;

namespace Close.DotNet;

/// <summary>Fields to create a lead.</summary>
public sealed class CreateLeadRequest : CloseWriteRequest
{
    /// <summary>Name.</summary>
    public string? Name { get; init; }

    /// <summary>Description.</summary>
    public string? Description { get; init; }

    /// <summary>Url.</summary>
    public string? Url { get; init; }

    /// <summary>Close status identifier.</summary>
    public string? StatusId { get; init; }

    /// <summary>Status.</summary>
    public string? Status { get; init; }

    /// <summary>Addresses.</summary>
    public IReadOnlyList<CloseAddress>? Addresses { get; init; }

    /// <summary>Contacts.</summary>
    public IReadOnlyList<CreateLeadContactRequest>? Contacts { get; init; }
}

/// <summary>Fields to create a contact inside a new lead.</summary>
public class CreateLeadContactRequest : CloseWriteRequest
{
    /// <summary>Contact name.</summary>
    public string? Name { get; init; }

    /// <summary>Job title.</summary>
    public string? Title { get; init; }

    /// <summary>IANA time zone name.</summary>
    public string? Timezone { get; init; }

    /// <summary>Contact email addresses.</summary>
    public IReadOnlyList<CloseEmail>? Emails { get; init; }

    /// <summary>Contact phone numbers.</summary>
    public IReadOnlyList<ClosePhone>? Phones { get; init; }

    /// <summary>Contact links.</summary>
    public IReadOnlyList<CloseUrl>? Urls { get; init; }
}

/// <summary>Fields to create a contact under an existing lead.</summary>
public sealed class CreateContactRequest : CreateLeadContactRequest
{
    /// <summary>Identifier of the associated lead.</summary>
    public required string LeadId { get; init; }

    /// <summary>Identifier of the creator.</summary>
    public string? CreatedBy { get; init; }

    /// <summary>Creation time for imported records.</summary>
    public DateTimeOffset? DateCreated { get; init; }
}

/// <summary>Contact fields in a lead update. Without an ID, Close creates a new contact.</summary>
public sealed class UpdateLeadContactRequest : UpdateContactFields
{
    /// <summary>Identifier of the contact to update.</summary>
    public string? Id { get; init; }
}

/// <summary>Fields to create an opportunity.</summary>
public sealed class CreateOpportunityRequest : CloseWriteRequest
{
    /// <summary>Identifier of the associated contact.</summary>
    public string? ContactId { get; init; }

    /// <summary>Close status identifier.</summary>
    public string? StatusId { get; init; }

    /// <summary>User Id.</summary>
    public string? UserId { get; init; }

    /// <summary>Estimated win probability from 0 to 100.</summary>
    public int? Confidence { get; init; }

    /// <summary>Opportunity amount in the currency's minor units.</summary>
    public long? Value { get; init; }

    /// <summary>Close value period, such as one_time, monthly, or annual.</summary>
    public string? ValuePeriod { get; init; }

    /// <summary>Date Won.</summary>
    public DateOnly? DateWon { get; init; }

    /// <summary>Note.</summary>
    public string? Note { get; init; }

    /// <summary>HTML body of the opportunity note.</summary>
    public string? NoteHtml { get; init; }

    /// <summary>Attachments.</summary>
    public IReadOnlyList<CloseAttachment>? Attachments { get; init; }

    /// <summary>Identifier of the associated lead.</summary>
    public required string LeadId { get; init; }

    /// <summary>Pipeline Id.</summary>
    public string? PipelineId { get; init; }

    /// <summary>Created By.</summary>
    public string? CreatedBy { get; init; }

    /// <summary>Date Created.</summary>
    public DateTimeOffset? DateCreated { get; init; }
}

/// <summary>Fields to create a task.</summary>
public sealed class CreateTaskRequest : CloseWriteRequest
{
    /// <summary>Close object type discriminator.</summary>
    [JsonPropertyName("_type")]
    public string? Type { get; init; }

    /// <summary>Identifier of the associated lead.</summary>
    public required string LeadId { get; init; }

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

    /// <summary>Close task priority, such as high or medium.</summary>
    public string? Priority { get; init; }

    /// <summary>Agent configuration for an outgoing call task.</summary>
    public string? AgentConfigId { get; init; }

    /// <summary>Identifier of the task creator.</summary>
    public string? CreatedBy { get; init; }

    /// <summary>Suppresses the assignment notification when true.</summary>
    public bool? DisableNotification { get; init; }

    /// <summary>Date Created.</summary>
    public DateTimeOffset? DateCreated { get; init; }
}

/// <summary>Fields to create a note.</summary>
public sealed class CreateNoteRequest : CloseWriteRequest
{
    /// <summary>Identifier of the associated lead.</summary>
    public required string LeadId { get; init; }

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

    /// <summary>Pinned.</summary>
    public bool? Pinned { get; init; }

    /// <summary>Time of the activity.</summary>
    public DateTimeOffset? ActivityAt { get; init; }

    /// <summary>Attachments.</summary>
    public IReadOnlyList<CloseAttachment>? Attachments { get; init; }

    /// <summary>Created By.</summary>
    public string? CreatedBy { get; init; }

    /// <summary>Date Created.</summary>
    public DateTimeOffset? DateCreated { get; init; }

    /// <summary>User Id.</summary>
    public string? UserId { get; init; }
}

/// <summary>Fields to create a webhook.</summary>
public sealed class CreateWebhookRequest : CloseWriteRequest
{
    /// <summary>Url.</summary>
    public required string Url { get; init; }

    /// <summary>Webhook event subscriptions.</summary>
    public required IReadOnlyList<WebhookEventFilter> Events { get; init; }

    /// <summary>Whether Close verifies the delivery endpoint’s TLS certificate.</summary>
    public bool? VerifySsl { get; init; }
}

/// <summary>Fields to update on an existing lead.</summary>
public sealed class UpdateLeadRequest : CloseWriteRequest
{
    /// <summary>Name.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Name { get; init; }

    /// <summary>Description.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Description { get; init; }

    /// <summary>Url.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Url { get; init; }

    /// <summary>Close status identifier.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> StatusId { get; init; }

    /// <summary>Status.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Status { get; init; }

    /// <summary>Addresses.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<IReadOnlyList<CloseAddress>?> Addresses { get; init; }

    /// <summary>Contacts.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<IReadOnlyList<UpdateLeadContactRequest>?> Contacts { get; init; }
}

/// <summary>Updates to the contact fields shared by the lead and contact APIs.</summary>
public abstract class UpdateContactFields : CloseWriteRequest
{
    /// <summary>Name.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Name { get; init; }

    /// <summary>Title.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Title { get; init; }

    /// <summary>Timezone.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Timezone { get; init; }

    /// <summary>Emails.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<IReadOnlyList<CloseEmail>?> Emails { get; init; }

    /// <summary>Phones.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<IReadOnlyList<ClosePhone>?> Phones { get; init; }

    /// <summary>Urls.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<IReadOnlyList<CloseUrl>?> Urls { get; init; }
}

/// <summary>Fields to update through the standalone contact API.</summary>
public sealed class UpdateContactRequest : UpdateContactFields
{
    /// <summary>Identifier of the associated lead.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> LeadId { get; init; }
}

/// <summary>Fields to update on an existing opportunity.</summary>
public sealed class UpdateOpportunityRequest : CloseWriteRequest
{
    /// <summary>Identifier of the associated contact.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> ContactId { get; init; }

    /// <summary>Close status identifier.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> StatusId { get; init; }

    /// <summary>User Id.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> UserId { get; init; }

    /// <summary>Estimated win probability from 0 to 100.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<int?> Confidence { get; init; }

    /// <summary>Opportunity amount in the currency's minor units.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<long?> Value { get; init; }

    /// <summary>Close value period, such as one_time, monthly, or annual.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> ValuePeriod { get; init; }

    /// <summary>Date Won.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<DateOnly?> DateWon { get; init; }

    /// <summary>Note.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Note { get; init; }

    /// <summary>HTML body of the opportunity note.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> NoteHtml { get; init; }

    /// <summary>Attachments.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<IReadOnlyList<CloseAttachment>?> Attachments { get; init; }
}

/// <summary>Fields to update on an existing task.</summary>
public sealed class UpdateTaskRequest : CloseWriteRequest
{
    /// <summary>Identifier of the associated lead.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> LeadId { get; init; }

    /// <summary>Identifier of the associated contact.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> ContactId { get; init; }

    /// <summary>Assigned To.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> AssignedTo { get; init; }

    /// <summary>Is Complete.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<bool?> IsComplete { get; init; }

    /// <summary>Is Dateless.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<bool?> IsDateless { get; init; }

    /// <summary>Task date in the format expected by Close.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Date { get; init; }

    /// <summary>Task due date in the format expected by Close.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> DueDate { get; init; }

    /// <summary>Plain-text contents.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Text { get; init; }

    /// <summary>Priority.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Priority { get; init; }

    /// <summary>Resolution.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Resolution { get; init; }

    /// <summary>Agent Config Id.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> AgentConfigId { get; init; }
}

/// <summary>Fields to update on an existing note.</summary>
public sealed class UpdateNoteRequest : CloseWriteRequest
{
    /// <summary>Identifier of the associated contact.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> ContactId { get; init; }

    /// <summary>Plain-text contents.</summary>
    [JsonPropertyName("note")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Text { get; init; }

    /// <summary>HTML body of the note.</summary>
    [JsonPropertyName("note_html")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Html { get; init; }

    /// <summary>Title.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Title { get; init; }

    /// <summary>Pinned.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<bool?> Pinned { get; init; }

    /// <summary>Time of the activity.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<DateTimeOffset?> ActivityAt { get; init; }

    /// <summary>Attachments.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<IReadOnlyList<CloseAttachment>?> Attachments { get; init; }
}

/// <summary>Fields to update on an existing webhook.</summary>
public sealed class UpdateWebhookRequest : CloseWriteRequest
{
    /// <summary>Url.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Url { get; init; }

    /// <summary>Status.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<string?> Status { get; init; }

    /// <summary>Whether Close verifies the delivery endpoint’s TLS certificate.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<bool?> VerifySsl { get; init; }

    /// <summary>Webhook event subscriptions.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public CloseField<IReadOnlyList<WebhookEventFilter>?> Events { get; init; }
}
