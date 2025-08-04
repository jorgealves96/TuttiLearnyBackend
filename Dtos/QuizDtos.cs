using System.ComponentModel.DataAnnotations;

namespace LearningAppNetCoreApi.Dtos
{
    public class QuizResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<QuizQuestionDto> Questions { get; set; } = new();
    }

    public class QuizQuestionDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public List<string> Options { get; set; } = new();
        public int CorrectAnswerIndex { get; set; }
    }

    public class SubmitQuizResultDto
    {
        [Required]
        public int Score { get; set; }

        [Required]
        public int TotalQuestions { get; set; }
    }

    public class SubmitQuizFeedbackDto
    {
        [Required]
        public bool WasHelpful { get; set; }
    }
}
