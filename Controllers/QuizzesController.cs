using LearningAppNetCoreApi.Dtos;
using LearningAppNetCoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningAppNetCoreApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizzesController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        [HttpGet("history/{pathTemplateId}")]
        public async Task<IActionResult> GetQuizHistory(int pathTemplateId)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            var results = await _quizService.GetQuizHistoryAsync(pathTemplateId, firebaseUid);
            return Ok(results);
        }

        [HttpGet("results/{quizResultId}")]
        public async Task<IActionResult> GetQuizResult(int quizResultId)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            var result = await _quizService.GetQuizResultDetailsAsync(quizResultId, firebaseUid);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpGet("{quizResultId}/resume")]
        public async Task<IActionResult> ResumeQuiz(int quizResultId)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            var quizToResume = await _quizService.GetQuizForResumeAsync(quizResultId, firebaseUid);

            if (quizToResume == null)
            {
                return NotFound(new { message = "No active quiz found to resume." });
            }

            return Ok(quizToResume);
        }

        [HttpPost("generate/{pathTemplateId}")]
        public async Task<IActionResult> GenerateQuiz(int pathTemplateId)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            try
            {
                var quiz = await _quizService.CreateQuizAsync(pathTemplateId, firebaseUid);
                return Ok(quiz);
            }
            catch (Exception e)
            {
                // Return a BadRequest or another appropriate error if quiz generation fails
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpPost("{quizId}/submit")]
        public async Task<IActionResult> SubmitQuiz(int quizId, [FromBody] SubmitQuizAnswersDto dto, [FromQuery] bool isFinalSubmission = true)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            // The service method will now also need the 'isFinalSubmission' flag
            var result = await _quizService.CalculateAndSaveQuizResultAsync(quizId, dto.Answers, firebaseUid, isFinalSubmission);

            return Ok(new QuizResultResponseDto
            {
                QuizResultId = result.Id,
                Score = result.Score,
                TotalQuestions = result.TotalQuestions
            });
        }

        [HttpPost("{quizId}/feedback")]
        public async Task<IActionResult> SubmitFeedback(int quizId, [FromBody] SubmitQuizFeedbackDto dto)
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (firebaseUid == null) return Unauthorized();

            await _quizService.SubmitQuizFeedbackAsync(quizId, dto, firebaseUid);
            return Ok(new { message = "Feedback submitted successfully." });
        }
    }
}