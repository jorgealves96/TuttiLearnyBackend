using Newtonsoft.Json;

namespace LearningAppNetCoreApi.Dtos
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

    public class GeminiPathItemDto
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("resources")]
        public List<GeminiResourceDto> Resources { get; set; }
    }

    public class GeminiResourceDto
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("searchQuery")]
        public string? SearchQuery { get; set; }
    }
}
