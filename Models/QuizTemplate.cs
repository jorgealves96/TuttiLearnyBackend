namespace LearningAppNetCoreApi.Models
{
    public class QuizTemplate
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int PathTemplateId { get; set; } // Foreign key
        public PathTemplate PathTemplate { get; set; }
        public List<QuizQuestionTemplate> Questions { get; set; } = new();
    }
}
