using Newtonsoft.Json;

namespace LearningAppNetCoreApi.Dtos
{
    // DTO for Google Custom Search API
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

    // DTOs for YouTube Data API
    public class YouTubeSearchResponseDto
    {
        [JsonProperty("items")]
        public List<YouTubeSearchItemDto> Items { get; set; }
    }

    public class YouTubeSearchItemDto
    {
        [JsonProperty("id")]
        public YouTubeVideoIdDto Id { get; set; }
    }

    public class YouTubeVideoIdDto
    {
        [JsonProperty("videoId")]
        public string VideoId { get; set; }
    }
}
