namespace LearningAppNetCoreApi.DTOs
{
    public class LearningPathResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PathItemResponseDto> PathItems { get; set; }
    }

    public class PathItemResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public int Order { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class MyPathSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } // New property for the category
        public double Progress { get; set; }
    }
}
