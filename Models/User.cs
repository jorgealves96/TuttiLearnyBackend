namespace LearningAppNetCoreApi.Models
{
    public class User
    {
        public int Id { get; set; } // Primary Key
        public string Auth0Id { get; set; } // The unique ID from Auth0
        public string Email { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property: A user can have many learning paths
        public ICollection<LearningPath> LearningPaths { get; set; }
    }
}
