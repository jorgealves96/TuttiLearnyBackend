using System.ComponentModel.DataAnnotations;

namespace LearningAppNetCoreApi.Dtos
{
    public class CreatePathRequestDto
    {
        [Required]
        public string Prompt { get; set; }
    }

    // Represents a detailed path, combining template data with user progress
    public class LearningPathResponseDto
    {
        public int UserPathId { get; set; } // The ID of the user's specific instance of the path
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PathItemResponseDto> PathItems { get; set; }
    }

    public class PathItemResponseDto
    {
        public int Id { get; set; } // The ID of the PathItemTemplate
        public string Title { get; set; }
        public int Order { get; set; }
        public bool IsCompleted { get; set; }
        public List<ResourceResponseDto> Resources { get; set; }
    }

    public class ResourceResponseDto
    {
        public int Id { get; set; } // The ID of the ResourceTemplate
        public string Title { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public bool IsCompleted { get; set; }
    }

    // Represents a summary of a path a user is taking
    public class MyPathSummaryDto
    {
        public int UserPathId { get; set; } // The ID of the user's specific instance
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public double Progress { get; set; }
    }

    public class PathTemplateSummaryDto
    {
        public int Id { get; set; } // The ID of the PathTemplate
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }
}
