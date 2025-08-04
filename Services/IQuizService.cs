using LearningAppNetCoreApi.Dtos;
using LearningAppNetCoreApi.Models;

namespace LearningAppNetCoreApi.Services
{
    public interface IQuizService
    {
        Task<QuizResponseDto> CreateQuizAsync(int pathTemplateId, string firebaseUid);
        Task<List<QuizResultDto>> GetQuizHistoryAsync(int pathTemplateId, string firebaseUid);
        Task<QuizResumeDto?> GetQuizForResumeAsync(int quizResultId, string firebaseUid);
        Task<QuizReviewDto?> GetQuizResultDetailsAsync(int quizResultId, string firebaseUid);
        Task<QuizResult> CalculateAndSaveQuizResultAsync(int quizTemplateId, List<UserAnswerDto> userAnswers, string firebaseUid, bool isFinalSubmission);
        Task SubmitQuizFeedbackAsync(int quizTemplateId, SubmitQuizFeedbackDto feedbackDto, string firebaseUid);
    }
}
