namespace LearningAppNetCoreApi.Models
{
    public class PathItem
    {
        public int Id { get; set; } // Primary Key
        public string Title { get; set; }
        public string? Url { get; set; } // The link to the resource
        public ItemType Type { get; set; } // e.g., Video, Article, Book
        public int Order { get; set; } // Defines the sequence
        public bool IsCompleted { get; set; } = false;

        // Foreign Key relationship to the LearningPath
        public int LearningPathId { get; set; }
        public LearningPath LearningPath { get; set; }

        // Navigation property: A path item can have many notes
        public ICollection<Note> Notes { get; set; }
    }

    public enum ItemType
    {
        Article,
        Video,
        Book,
        Project,
        Documentation
    }
}
