using System.Text.Json.Serialization;

namespace BikeRentalApi.Models;

public record WebhookPayload(
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("entry")] List<Entry> Entry
);

public record Entry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("changes")] List<Change> Changes
);

public record Change(
    [property: JsonPropertyName("value")] ChangeValue Value,
    [property: JsonPropertyName("field")] string Field
);

public record ChangeValue(
    [property: JsonPropertyName("messaging_product")] string MessagingProduct,
    [property: JsonPropertyName("messages")] List<WhatsAppMessage>? Messages,
    [property: JsonPropertyName("contacts")] List<Contact>? Contacts
);

public record WhatsAppMessage(
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("timestamp")] string Timestamp,
    [property: JsonPropertyName("text")] TextContent? Text,
    [property: JsonPropertyName("type")] string Type
);

public record TextContent(
    [property: JsonPropertyName("body")] string Body
);

public record Contact(
    [property: JsonPropertyName("profile")] Profile Profile,
    [property: JsonPropertyName("wa_id")] string WaId
);

public record Profile(
    [property: JsonPropertyName("name")] string Name
);
