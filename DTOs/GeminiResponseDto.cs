using Newtonsoft.Json;

namespace LearningAppNetCoreApi.DTOs
{
    public class GeminiResponseDto
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("category")] // New property for the category
        public string Category { get; set; }

        [JsonProperty("items")]
        public List<GeminiPathItemDto> Items { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public class GeminiPathItemDto
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
