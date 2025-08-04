using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace LearningAppNetCoreApi.Dtos
{
    public class QuizResponseDto
    {
        public int Id { get; set; }

        [JsonProperty("quizTitle")]
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

    public class UserAnswerDto
    {
        public int QuestionId { get; set; }
        public int SelectedAnswerIndex { get; set; }
    }

    public class SubmitQuizAnswersDto
    {
        [Required]
        public List<UserAnswerDto> Answers { get; set; }
    }

    public class QuizResultResponseDto
    {
        public int QuizResultId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
    }

    public class QuizResultDto
    {
        public int Id { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime CompletedAt { get; set; }
        public bool IsComplete { get; set; }
    }

    public class SubmitQuizResultDto
    {
        [Required]
        public int Score { get; set; }
        [Required]
        public int TotalQuestions { get; set; }
        [Required]
        public List<UserAnswerDto> Answers { get; set; }
    }

    public class SubmitQuizFeedbackDto
    {
        [Required]
        public bool WasHelpful { get; set; }
    }

    public class UserQuizAnswerDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public List<string> Options { get; set; } = new();
        public int CorrectAnswerIndex { get; set; }
        public int SelectedAnswerIndex { get; set; }
    }

    public class QuizReviewDto
    {
        public int QuizResultId { get; set; }
        public string QuizTitle { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime CompletedAt { get; set; }
        public List<UserQuizAnswerDto> Answers { get; set; } = new();
    }

    public class QuizResumeDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; }
        public List<QuizQuestionDto> Questions { get; set; } = new();
        public List<UserAnswerDto> SavedAnswers { get; set; } = new();
    }
}
