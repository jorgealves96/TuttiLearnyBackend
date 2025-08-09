using LearningAppNetCoreApi.Constants;
using LearningAppNetCoreApi.Dtos;
using LearningAppNetCoreApi.Exceptions;
using LearningAppNetCoreApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace LearningAppNetCoreApi.Services
{
    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<QuizService> _logger;

        public QuizService(ApplicationDbContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory, IWebHostEnvironment env, ILogger<QuizService> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _env = env;
            _logger = logger;
        }

        public async Task<QuizResponseDto> CreateQuizAsync(int pathTemplateId, string firebaseUid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid) ?? throw new Exception("User not found.");

            ResetMonthlyUsageCounters(user);

            var quizLimit = GetQuizLimitForTier(user.Tier);

            if (quizLimit.HasValue && user.QuizzesCreatedThisMonth >= quizLimit.Value)
            {
                _logger.LogInformation("User {FirebaseUid} reached their quiz creation limit", firebaseUid);
                throw new ApiQuotaExceededException("Quiz creation limit reached for this month.");
            }

            var pathTemplate = await _context.PathTemplates
                .Include(p => p.PathItems)
                .ThenInclude(pi => pi.Resources)
                .FirstOrDefaultAsync(p => p.Id == pathTemplateId) ?? throw new Exception("Path not found.");

            var pastQuestions = await _context.QuizQuestionTemplates
                .Where(q => q.QuizTemplate.PathTemplateId == pathTemplateId)
                .OrderByDescending(q => q.Id)
                .Take(50)
                .Select(q => q.QuestionText)
                .ToListAsync(); // Limiting it to the last max 50 questions the user got should be enough for context

            var geminiQuiz = await GetQuizFromGemini(pathTemplate, pastQuestions);

            var newQuizTemplate = new QuizTemplate
            {
                Title = geminiQuiz.Title,
                PathTemplateId = pathTemplateId,
                Questions = geminiQuiz.Questions.Select(q => new QuizQuestionTemplate
                {
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    CorrectAnswerIndex = q.CorrectAnswerIndex
                }).ToList()
            };

            _context.QuizTemplates.Add(newQuizTemplate);

            // --- Increment the user's usage counter ---
            user.QuizzesCreatedThisMonth++;

            await _context.SaveChangesAsync();

            return MapQuizToDto(newQuizTemplate);
        }

        public async Task<List<QuizResultDto>> GetQuizHistoryAsync(int pathTemplateId, string firebaseUid)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) throw new Exception("User not found.");

            var results = await _context.QuizResults
                .Where(r => r.UserId == user.Id && r.QuizTemplate.PathTemplateId == pathTemplateId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new QuizResultDto
                { // You'll need to create this DTO
                    Id = r.Id,
                    Score = r.Score,
                    TotalQuestions = r.TotalQuestions,
                    CompletedAt = r.CompletedAt,
                    IsComplete = r.IsComplete
                })
                .ToListAsync();

            return results;
        }

        public async Task<QuizResumeDto?> GetQuizForResumeAsync(int quizResultId, string firebaseUid)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) throw new Exception("User not found.");

            var quizResult = await _context.QuizResults
                .Include(r => r.QuizTemplate)
                    .ThenInclude(qt => qt.Questions)
                .Include(r => r.UserAnswers)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == quizResultId && r.UserId == user.Id && !r.IsComplete);

            if (quizResult == null)
            {
                return null; // No in-progress quiz found
            }

            return new QuizResumeDto
            {
                QuizId = quizResult.QuizTemplate.Id,
                Title = quizResult.QuizTemplate.Title,
                Questions = quizResult.QuizTemplate.Questions.Select(q => new QuizQuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    CorrectAnswerIndex = q.CorrectAnswerIndex
                }).ToList(),
                SavedAnswers = quizResult.UserAnswers.Select(a => new UserAnswerDto
                {
                    QuestionId = a.QuestionId,
                    SelectedAnswerIndex = a.SelectedAnswerIndex
                }).ToList()
            };
        }

        public async Task<QuizReviewDto?> GetQuizResultDetailsAsync(int quizResultId, string firebaseUid)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) throw new Exception("User not found.");

            // Step 1: Get the main quiz result and the user's answers
            var quizResult = await _context.QuizResults
                .Include(r => r.QuizTemplate)
                .Include(r => r.UserAnswers)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == quizResultId && r.UserId == user.Id);

            if (quizResult == null)
            {
                return null; // No result found
            }

            // Step 2: Get the details for all questions in this quiz
            var questionIds = quizResult.UserAnswers.Select(ua => ua.QuestionId).ToList();
            var questionDetails = await _context.QuizQuestionTemplates
                .Where(q => questionIds.Contains(q.Id))
                .AsNoTracking()
                .ToDictionaryAsync(q => q.Id);

            // Step 3: Manually build the final DTO
            var reviewDto = new QuizReviewDto
            {
                QuizResultId = quizResult.Id,
                QuizTitle = quizResult.QuizTemplate.Title,
                Score = quizResult.Score,
                TotalQuestions = quizResult.TotalQuestions,
                CompletedAt = quizResult.CompletedAt,
                Answers = quizResult.UserAnswers.Select(ua => new UserQuizAnswerDto
                {
                    QuestionId = ua.QuestionId,
                    SelectedAnswerIndex = ua.SelectedAnswerIndex,
                    // Populate details from the dictionary we fetched in Step 2
                    QuestionText = questionDetails[ua.QuestionId].QuestionText,
                    Options = questionDetails[ua.QuestionId].Options,
                    CorrectAnswerIndex = questionDetails[ua.QuestionId].CorrectAnswerIndex
                }).ToList()
            };

            return reviewDto;
        }

        public async Task<QuizResult> CalculateAndSaveQuizResultAsync(int quizTemplateId, List<UserAnswerDto> userAnswers, string firebaseUid, bool isFinalSubmission)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid) ?? throw new Exception("User not found");

            // Check for an existing INCOMPLETE result for this quiz template
            var existingResult = await _context.QuizResults
                .Include(r => r.UserAnswers)
                .FirstOrDefaultAsync(r => r.UserId == user.Id && r.QuizTemplateId == quizTemplateId && !r.IsComplete);

            var result = existingResult ?? new QuizResult
            {
                UserId = user.Id,
                QuizTemplateId = quizTemplateId,
            };

            var correctAnswers = await _context.QuizQuestionTemplates
                .Where(q => q.QuizTemplateId == quizTemplateId)
                .AsNoTracking()
                .ToDictionaryAsync(q => q.Id, q => q.CorrectAnswerIndex);

            int score = 0;
            foreach (var answer in userAnswers)
            {
                if (correctAnswers.ContainsKey(answer.QuestionId) && correctAnswers[answer.QuestionId] == answer.SelectedAnswerIndex)
                {
                    score++;
                }
            }

            result.Score = score;
            result.TotalQuestions = correctAnswers.Count;
            result.IsComplete = isFinalSubmission;
            result.CompletedAt = DateTime.UtcNow;

            // Clear any old answers and add the new ones
            result.UserAnswers.Clear();
            var newUserAnswers = userAnswers.Select(a => new UserQuizAnswer
            {
                QuestionId = a.QuestionId,
                SelectedAnswerIndex = a.SelectedAnswerIndex,
                WasCorrect = correctAnswers.ContainsKey(a.QuestionId) && correctAnswers[a.QuestionId] == a.SelectedAnswerIndex
            }).ToList();
            result.UserAnswers.AddRange(newUserAnswers);

            if (existingResult == null)
            {
                _context.QuizResults.Add(result);
            }

            await _context.SaveChangesAsync();

            return result;
        }

        public async Task SubmitQuizFeedbackAsync(int quizTemplateId, SubmitQuizFeedbackDto feedbackDto, string firebaseUid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) throw new Exception("User not found");

            // Create or update feedback
            var existingFeedback = await _context.QuizFeedbacks.FirstOrDefaultAsync(f => f.UserId == user.Id && f.QuizTemplateId == quizTemplateId);
            if (existingFeedback != null)
            {
                existingFeedback.WasHelpful = feedbackDto.WasHelpful;
            }
            else
            {
                _context.QuizFeedbacks.Add(new QuizFeedback
                {
                    UserId = user.Id,
                    QuizTemplateId = quizTemplateId,
                    WasHelpful = feedbackDto.WasHelpful
                });
            }
            await _context.SaveChangesAsync();
        }

        #region Private Methods

        private async Task<T> CallAndParseGeminiObjectAsync<T>(string apiUrl, object payload)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error calling Gemini API: {response.ReasonPhrase}. Details: {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic parsedJsonResponse = JsonConvert.DeserializeObject(jsonResponse);
            string textContent = parsedJsonResponse.candidates[0].content.parts[0].text;

            // Correctly find the start and end of the JSON object
            int firstBrace = textContent.IndexOf('{');
            int lastBrace = textContent.LastIndexOf('}');
            if (firstBrace == -1 || lastBrace == -1)
            {
                throw new JsonReaderException("The response from the AI service did not contain a valid JSON object.");
            }
            string cleanedJson = textContent.Substring(firstBrace, lastBrace - firstBrace + 1);

            try
            {
                return JsonConvert.DeserializeObject<T>(cleanedJson);
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine($"Failed to deserialize JSON object: {cleanedJson}");
                throw new JsonSerializationException($"Failed to deserialize the AI's response. Details: {ex.Message}", ex);
            }
        }

        private string GetGeminiApiUrl()
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"];
            return $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
        }

        private async Task<QuizResponseDto> GetQuizFromGemini(PathTemplate pathTemplate, List<string> pastQuestions)
        {
            var apiUrl = GetGeminiApiUrl();
            var promptTemplatePath = Path.Combine(_env.ContentRootPath, "Prompts", "CreateQuizPrompt.txt");
            var promptTemplate = await File.ReadAllTextAsync(promptTemplatePath);

            var itemTitles = string.Join(", ", pathTemplate.PathItems.Select(pi => $"\"{pi.Title}\""));
            var resourceTitles = string.Join(", ", pathTemplate.PathItems.SelectMany(pi => pi.Resources).Select(r => $"\"{r.Title}\""));

            // --- This is the new logic ---
            // Format the list of past questions. If there are none, use "None".
            var pastQuestionsText = pastQuestions.Any()
                ? string.Join(", ", pastQuestions.Select(q => $"\"{q}\""))
                : "None";
            // ----------------------------

            // Inject all context into the prompt, including the new placeholder
            var fullPrompt = promptTemplate
                .Replace("{pathTitle}", pathTemplate.Title)
                .Replace("{pathDescription}", pathTemplate.Description)
                .Replace("{pathItems}", itemTitles)
                .Replace("{resources}", resourceTitles)
                .Replace("{pastQuestions}", pastQuestionsText); // <-- New placeholder

            var payload = new
            {
                contents = new[] { new { parts = new[] { new { text = fullPrompt } } } },
            };

            return await CallAndParseGeminiObjectAsync<QuizResponseDto>(apiUrl, payload);
        }

        private QuizResponseDto MapQuizToDto(QuizTemplate quizTemplate)
        {
            return new QuizResponseDto
            {
                Id = quizTemplate.Id,
                Title = quizTemplate.Title,
                Questions = quizTemplate.Questions.Select(q => new QuizQuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Options = q.Options,
                    CorrectAnswerIndex = q.CorrectAnswerIndex
                }).ToList()
            };
        }

        private int? GetQuizLimitForTier(SubscriptionTier tier)
        {
            switch (tier)
            {
                case SubscriptionTier.Free:
                    return SubscriptionConstants.FreeQuizCreationLimit;
                case SubscriptionTier.Pro:
                    return SubscriptionConstants.ProQuizCreationLimit;
                case SubscriptionTier.Unlimited:
                default:
                    return null; // Null means unlimited
            }
        }

        private void ResetMonthlyUsageCounters(User user)
        {
            var now = DateTime.UtcNow;
            if (user.LastUsageResetDate.Month != now.Month || user.LastUsageResetDate.Year != now.Year)
            {
                user.PathsGeneratedThisMonth = 0;
                user.PathsExtendedThisMonth = 0;
                user.QuizzesCreatedThisMonth = 0;
                user.LastUsageResetDate = now;

                // Note: This method only modifies the user object.
                // The calling method is responsible for saving the changes to the database.
                // This is why there is no `_context.SaveChangesAsync()` here.
            }
        }

        #endregion
    }
}
