using Newtonsoft.Json;

namespace LearningAppNetCoreApi.DTOs
{
    public class GeminiResponseDto
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("items")]
        public List<GeminiPathItemDto> Items { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    // Represents a conceptual step from the AI
    public class GeminiPathItemDto
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("resources")]
        public List<GeminiResourceDto> Resources { get; set; }
    }

    // Represents a single resource suggestion from the AI
    public class GeminiResourceDto
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("searchQuery")]
        public string SearchQuery { get; set; }
    }
}
