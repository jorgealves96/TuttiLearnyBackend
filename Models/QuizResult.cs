namespace LearningAppNetCoreApi.Models
{
    public class QuizResult
    {
        public int Id { get; set; }
        public int Score { get; set; } // e.g., 4 (out of 5)
        public int TotalQuestions { get; set; } // e.g., 5
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime CompletedAt { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public int QuizTemplateId { get; set; }
        public QuizTemplate QuizTemplate { get; set; }
        public List<UserQuizAnswer> UserAnswers { get; set; } = new();
        public bool IsComplete { get; set; } = false;
    }
}
