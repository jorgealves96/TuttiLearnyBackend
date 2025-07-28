namespace LearningAppNetCoreApi.Models
{
    public class PathTemplateRating
    {
        public int Id { get; set; }
        public int PathTemplateId { get; set; } // Foreign key to the PathTemplate
        public string FirebaseUid { get; set; }  // To know which user voted
        public int Rating { get; set; }         // e.g., 1 to 5
        public DateTime RatedAt { get; set; } = DateTime.UtcNow;

        public PathTemplate PathTemplate { get; set; }
    }
}
