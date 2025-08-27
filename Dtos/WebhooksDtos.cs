using Newtonsoft.Json;

namespace LearningAppNetCoreApi.Dtos
{
    public class RevenueCatWebhookDto
    {
        [JsonProperty("event")]
        public RevenueCatEventDto Event { get; set; }
    }

    public class RevenueCatEventDto
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("app_user_id")]
        public string AppUserId { get; set; } // This is the Firebase UID

        [JsonProperty("product_id")]
        public string ProductId { get; set; } // e.g., "pro_monthly"

        [JsonProperty("expiration_at_ms")]
        public long ExpirationAtMs { get; set; } // Expiration date in milliseconds
    }
}
