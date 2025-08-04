namespace LearningAppNetCoreApi.Models
{
    public class QuizFeedback
    {
        public int Id { get; set; }
        public bool WasHelpful { get; set; } // Thumbs up or down
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; }
        public int QuizTemplateId { get; set; }
        public QuizTemplate QuizTemplate { get; set; }
    }
}
