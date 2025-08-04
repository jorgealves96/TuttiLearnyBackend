namespace LearningAppNetCoreApi.Models
{
    public class QuizResult
    {
        public int Id { get; set; }
        public int Score { get; set; } // e.g., 4 (out of 5)
        public int TotalQuestions { get; set; } // e.g., 5
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User User { get; set; }
        public int QuizTemplateId { get; set; }
        public QuizTemplate QuizTemplate { get; set; }
    }
}
