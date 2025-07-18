using System.ComponentModel.DataAnnotations;

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
        public int Order { get; set; }
        public bool IsCompleted { get; set; }
        public List<ResourceDto> Resources { get; set; }
    }

    public class ResourceDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public bool IsCompleted { get; set; } // isCompleted is now here
    }

    public class MyPathSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } // New property for the category
        public double Progress { get; set; }
    }

    public class CreatePathRequestDto
    {
        [Required]
        public string Prompt { get; set; }
    }
}
