using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Close.DotNet;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = [typeof(CloseTimestampConverter),
        typeof(CloseFieldConverter<string?>),
        typeof(CloseFieldConverter<IReadOnlyList<CloseAddress>?>),
        typeof(CloseFieldConverter<IReadOnlyList<UpdateLeadContactRequest>?>),
        typeof(CloseFieldConverter<IReadOnlyList<CloseEmail>?>),
        typeof(CloseFieldConverter<IReadOnlyList<ClosePhone>?>),
        typeof(CloseFieldConverter<IReadOnlyList<CloseUrl>?>),
        typeof(CloseFieldConverter<int?>),
        typeof(CloseFieldConverter<long?>),
        typeof(CloseFieldConverter<DateOnly?>),
        typeof(CloseFieldConverter<IReadOnlyList<CloseAttachment>?>),
        typeof(CloseFieldConverter<bool?>),
        typeof(CloseFieldConverter<DateTimeOffset?>),
        typeof(CloseFieldConverter<IReadOnlyList<WebhookEventFilter>?>)])]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(CloseUser))]
[JsonSerializable(typeof(CloseOrganization))]
[JsonSerializable(typeof(CloseMembership))]
[JsonSerializable(typeof(CloseEvent))]
[JsonSerializable(typeof(CloseEventPage))]
[JsonSerializable(typeof(CloseSearchPage))]
[JsonSerializable(typeof(CloseSearchRequest))]
[JsonSerializable(typeof(CloseWebhookDelivery))]
[JsonSerializable(typeof(CloseOAuthTokens))]
[JsonSerializable(typeof(CloseUploadRequest))]
[JsonSerializable(typeof(CloseUploadTicket))]
[JsonSerializable(typeof(Lead))]
[JsonSerializable(typeof(ClosePage<Lead>))]
[JsonSerializable(typeof(Contact))]
[JsonSerializable(typeof(ClosePage<Contact>))]
[JsonSerializable(typeof(Opportunity))]
[JsonSerializable(typeof(ClosePage<Opportunity>))]
[JsonSerializable(typeof(CloseTask))]
[JsonSerializable(typeof(ClosePage<CloseTask>))]
[JsonSerializable(typeof(Note))]
[JsonSerializable(typeof(ClosePage<Note>))]
[JsonSerializable(typeof(WebhookSubscription))]
[JsonSerializable(typeof(ClosePage<WebhookSubscription>))]
[JsonSerializable(typeof(CreateLeadRequest))]
[JsonSerializable(typeof(CreateContactRequest))]
[JsonSerializable(typeof(CreateOpportunityRequest))]
[JsonSerializable(typeof(CreateTaskRequest))]
[JsonSerializable(typeof(CreateNoteRequest))]
[JsonSerializable(typeof(CreateWebhookRequest))]
[JsonSerializable(typeof(UpdateLeadRequest))]
[JsonSerializable(typeof(UpdateContactRequest))]
[JsonSerializable(typeof(UpdateOpportunityRequest))]
[JsonSerializable(typeof(UpdateTaskRequest))]
[JsonSerializable(typeof(UpdateNoteRequest))]
[JsonSerializable(typeof(UpdateWebhookRequest))]
[JsonSerializable(typeof(IReadOnlyList<CloseAddress>))]
[JsonSerializable(typeof(IReadOnlyList<UpdateLeadContactRequest>))]
[JsonSerializable(typeof(IReadOnlyList<CloseEmail>))]
[JsonSerializable(typeof(IReadOnlyList<ClosePhone>))]
[JsonSerializable(typeof(IReadOnlyList<CloseUrl>))]
[JsonSerializable(typeof(int?))]
[JsonSerializable(typeof(long?))]
[JsonSerializable(typeof(DateOnly?))]
[JsonSerializable(typeof(IReadOnlyList<CloseAttachment>))]
[JsonSerializable(typeof(bool?))]
[JsonSerializable(typeof(DateTimeOffset?))]
[JsonSerializable(typeof(IReadOnlyList<WebhookEventFilter>))]
[JsonSerializable(typeof(string))]
internal sealed partial class CloseJson : JsonSerializerContext
{
    internal static JsonTypeInfo<T> Type<T>() => (JsonTypeInfo<T>)Default.Options.GetTypeInfo(typeof(T));
}
