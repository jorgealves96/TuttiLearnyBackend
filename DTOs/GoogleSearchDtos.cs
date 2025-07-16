using Newtonsoft.Json;

namespace LearningAppNetCoreApi.DTOs
{
    public class GoogleSearchResponseDto
    {
        [JsonProperty("items")]
        public List<SearchResultItemDto> Items { get; set; }
    }

    public class SearchResultItemDto
    {
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
