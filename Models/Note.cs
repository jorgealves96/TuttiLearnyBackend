namespace LearningAppNetCoreApi.Models
{
    public class Note
    {
        public int Id { get; set; } // Primary Key
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key relationship to the PathItem
        public int PathItemId { get; set; }
        public PathItem PathItem { get; set; }
    }
}
