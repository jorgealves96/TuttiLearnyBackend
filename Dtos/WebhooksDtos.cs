using System.Text.Json.Serialization;
namespace LearningAppNetCoreApi.Dtos
{
    public class RevenueCatWebhookDto
    {
        [JsonPropertyName("event")]
        public RevenueCatEventDto Event { get; set; }
    }

    public class RevenueCatEventDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("app_user_id")]
        public string AppUserId { get; set; } // This is the Firebase UID

        [JsonPropertyName("product_id")]
        public string ProductId { get; set; } // e.g., "pro_monthly"

        [JsonPropertyName("expiration_at_ms")]
        public long ExpirationAtMs { get; set; } // Expiration date in milliseconds
    }
}
