namespace LearningAppNetCoreApi.Models
{
    public class PathTemplate
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string GeneratedFromPrompt { get; set; }
        public PathCategory Category { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<PathItemTemplate> PathItems { get; set; } = [];
        public ICollection<UserPath> UserPaths { get; set; } // A template can be taken by many users
    }

    public enum PathCategory
    {
        Technology,
        CreativeArts,
        Music,
        Business,
        Wellness,
        LifeSkills,
        Gaming,
        Academic,
        Other
    }
}
