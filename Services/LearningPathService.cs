using LearningAppNetCoreApi.DTOs;
using LearningAppNetCoreApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Text;

namespace LearningAppNetCoreApi.Services
{
    public class LearningPathService : ILearningPathService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LearningPathService(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IEnumerable<MyPathSummaryDto>> GetUserPathsAsync(string userAuth0Id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Auth0Id == userAuth0Id);
            if (user == null)
            {
                return new List<MyPathSummaryDto>();
            }

            return await _context.LearningPaths
                .Where(p => p.UserId == user.Id)
                .Include(p => p.PathItems)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new MyPathSummaryDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Category = p.Category.ToString(), // Include the category
                    Progress = p.PathItems.Any() ? (double)p.PathItems.Count(pi => pi.IsCompleted) / p.PathItems.Count : 0
                })
                .ToListAsync();
        }

        public async Task<LearningPathResponseDto> GetPathByIdAsync(int pathId)
        {
            var path = await _context.LearningPaths
                .Include(p => p.PathItems) // Eagerly load the path items
                .FirstOrDefaultAsync(p => p.Id == pathId);

            if (path == null)
            {
                return null; // Or throw a custom NotFoundException
            }

            // Map the database entity to the response DTO
            var responseDto = new LearningPathResponseDto
            {
                Id = path.Id,
                Title = path.Title,
                Description = path.Description,
                CreatedAt = path.CreatedAt,
                PathItems = path.PathItems
                    .OrderBy(pi => pi.Order) // Ensure items are ordered correctly
                    .Select(pi => new PathItemResponseDto
                    {
                        Id = pi.Id,
                        Title = pi.Title,
                        Url = pi.Url,
                        Type = pi.Type.ToString(),
                        Order = pi.Order,
                        IsCompleted = pi.IsCompleted
                    }).ToList()
            };

            return responseDto;
        }

        public async Task<LearningPathResponseDto> CreateLearningPathAsync(string prompt, string userAuth0Id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Auth0Id == userAuth0Id);
            if (user == null)
            {
                user = new User { Auth0Id = userAuth0Id, Email = "test@test.com", Name = "Test User" };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var geminiResponse = await GetLearningPathFromGemini(prompt);

            if (!string.IsNullOrEmpty(geminiResponse.Error))
            {
                throw new InvalidOperationException(geminiResponse.Error);
            }

            var newLearningPath = new LearningPath
            {
                Title = geminiResponse.Title,
                Description = geminiResponse.Description,
                GeneratedFromPrompt = prompt,
                UserId = user.Id,
                Category = Enum.Parse<PathCategory>(geminiResponse.Category, true), // Parse the category
                PathItems = (geminiResponse.Items ?? new List<GeminiPathItemDto>()).Select((item, index) => new PathItem
                {
                    Title = item.Title,
                    Type = Enum.Parse<ItemType>(item.Type, true),
                    Url = item.Url,
                    Order = index + 1
                }).ToList(),
                CreatedAt = DateTime.UtcNow
            };

            _context.LearningPaths.Add(newLearningPath);
            await _context.SaveChangesAsync();

            return new LearningPathResponseDto
            {
                Id = newLearningPath.Id,
                Title = newLearningPath.Title,
                Description = newLearningPath.Description,
                CreatedAt = newLearningPath.CreatedAt,
                PathItems = newLearningPath.PathItems.Select(pi => new PathItemResponseDto
                {
                    Id = pi.Id,
                    Title = pi.Title,
                    Url = pi.Url,
                    Type = pi.Type.ToString(),
                    Order = pi.Order,
                    IsCompleted = pi.IsCompleted
                }).ToList()
            };
        }

        public async Task<List<PathItemResponseDto>> ExtendLearningPathAsync(int pathId)
        {
            var existingPath = await _context.LearningPaths
                .Include(p => p.PathItems)
                .FirstOrDefaultAsync(p => p.Id == pathId);

            if (existingPath == null)
            {
                return null; // Or throw a NotFoundException
            }

            // --- Step 1: Get new items from the AI ---
            var newItemsFromGemini = await GetNextPathItemsFromGemini(existingPath);

            if (newItemsFromGemini == null || newItemsFromGemini.Count == 0)
            {
                return []; // Nothing new to add
            }

            // --- Step 2: Create and add the new PathItem entities ---
            var highestOrder = existingPath.PathItems.Count != 0 ? existingPath.PathItems.Max(pi => pi.Order) : 0;
            var newPathItems = newItemsFromGemini.Select((item, index) => new PathItem
            {
                Title = item.Title,
                Type = Enum.Parse<ItemType>(item.Type, true),
                Url = item.Url,
                Order = highestOrder + index + 1, // Continue the sequence
                LearningPathId = pathId
            }).ToList();

            _context.PathItems.AddRange(newPathItems);
            await _context.SaveChangesAsync();

            // --- Step 3: Map the newly created items to DTOs and return them ---
            return newPathItems.Select(pi => new PathItemResponseDto
            {
                Id = pi.Id,
                Title = pi.Title,
                Url = pi.Url,
                Type = pi.Type.ToString(),
                Order = pi.Order,
                IsCompleted = pi.IsCompleted
            }).ToList();
        }

        public async Task<PathItemResponseDto> TogglePathItemCompletionAsync(int itemId)
        {
            var pathItem = await _context.PathItems.FirstOrDefaultAsync(pi => pi.Id == itemId);

            if (pathItem == null)
            {
                return null; // Item not found
            }

            // Flip the completion status
            pathItem.IsCompleted = !pathItem.IsCompleted;

            await _context.SaveChangesAsync();

            // Map the updated entity to a DTO and return it
            return new PathItemResponseDto
            {
                Id = pathItem.Id,
                Title = pathItem.Title,
                Url = pathItem.Url,
                Type = pathItem.Type.ToString(),
                Order = pathItem.Order,
                IsCompleted = pathItem.IsCompleted
            };
        }

        #region Private Methods

        private async Task<GeminiResponseDto> GetLearningPathFromGemini(string userPrompt)
        {
            var apiUrl = GetGeminiApiUrl();

            var fullPrompt = $@"
System Instructions:
You are an expert curriculum designer. Your only function is to return a single, valid JSON object. Do not include any markdown formatting, code block fences, or any text outside of the JSON structure.

First, evaluate the user's request and determine its category. The category MUST be one of the following strings: 'Technology', 'CreativeArts', 'Music', 'Business', 'Wellness', 'LifeSkills', 'Academic', 'Other'.

Next, determine the complexity of the topic.
- If the topic is simple and can be comprehensively explained by one or two main resources (e.g., 'how to solve a Rubik's cube' can be a single video), create a short path with only 1-2 items that link to those comprehensive resources.
- For more complex topics that require multiple distinct learning stages (e.g., 'learn javascript'), create a more detailed path with 5-7 fundamental, beginner-friendly steps.

The final JSON object must contain 'title', 'description', 'category', and an 'items' array. Each item in the array must have a 'title', a 'type' (one of 'Article', 'Video', 'Book', 'Project', 'Documentation'), and a 'url'.
If the user's request is not for a learning path, return a JSON object with an 'error' field explaining the issue.

User Request:
'{userPrompt}'
";

            var payload = new { contents = new[] { new { parts = new[] { new { text = fullPrompt } } } } };
            return await CallGeminiApi<GeminiResponseDto>(apiUrl, payload);
        }

        private async Task<List<GeminiPathItemDto>> GetNextPathItemsFromGemini(LearningPath existingPath)
        {
            var apiUrl = GetGeminiApiUrl();

            var existingItems = string.Join(", ", existingPath.PathItems.Select(pi => $"\"{pi.Title}\""));
            var fullPrompt = $@"
System Instructions:
You are an expert curriculum designer who continues an existing learning path.
Your only function is to return a single, valid JSON array of items. Each item must have a 'title', a 'type', and a 'url'.
Do not include any markdown formatting, code block fences, or any text outside of the JSON array structure.
Do not repeat any topics that are already in the user's current path.

Context:
The user is learning about '{existingPath.GeneratedFromPrompt}'.
They have already been given the following learning items: [{existingItems}].

User Request:
'Generate the next 5 logical steps for this learning path.'
";

            var payload = new { contents = new[] { new { parts = new[] { new { text = fullPrompt } } } } };
            var geminiDto = await CallGeminiApi<List<GeminiPathItemDto>>(apiUrl, payload);
            return geminiDto;
        }

        private string GetGeminiApiUrl()
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"];
            return $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
        }

        private async Task<T> CallGeminiApi<T>(string apiUrl, object payload)
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

            int firstBracket = textContent.IndexOf('{');
            if (firstBracket == -1) firstBracket = textContent.IndexOf('[');

            int lastBracket = textContent.LastIndexOf('}');
            if (lastBracket == -1) lastBracket = textContent.LastIndexOf(']');

            if (firstBracket == -1 || lastBracket == -1)
            {
                throw new JsonReaderException("The response from the AI service did not contain a valid JSON object or array.");
            }

            string cleanedJson = textContent.Substring(firstBracket, lastBracket - firstBracket + 1);

            return JsonConvert.DeserializeObject<T>(cleanedJson);
        }

        #endregion
    }
}