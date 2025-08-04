namespace LearningAppNetCoreApi.Models
{
    public class UserQuizAnswer
    {
        public int Id { get; set; }
        public int QuestionId { get; set; } // The ID of the QuizQuestionTemplate
        public int SelectedAnswerIndex { get; set; }
        public bool WasCorrect { get; set; }

        public int QuizResultId { get; set; } // Foreign key to the result
        public QuizResult QuizResult { get; set; }
    }
}
