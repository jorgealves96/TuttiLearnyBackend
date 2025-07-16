using LearningAppNetCoreApi.DTOs;
using LearningAppNetCoreApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Web;

namespace LearningAppNetCoreApi.Services
{
    public class LearningPathService : ILearningPathService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public LearningPathService(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IWebHostEnvironment env) // Inject the environment service
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _env = env;
        }

        public async Task<IEnumerable<MyPathSummaryDto>> GetUserPathsAsync(string userAuth0Id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Auth0Id == userAuth0Id);
            if (user == null)
            {
                return new List<MyPathSummaryDto>();
            }

            var paths = await _context.LearningPaths
                .Where(p => p.UserId == user.Id)
                .Include(p => p.PathItems)
                    .ThenInclude(pi => pi.Resources) // Eagerly load all resources
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Manually calculate progress in memory based on resources
            return paths.Select(p =>
            {
                var allResources = p.PathItems.SelectMany(pi => pi.Resources).ToList();
                var completedResources = allResources.Count(r => r.IsCompleted);
                var totalResources = allResources.Count;

                return new MyPathSummaryDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Category = p.Category.ToString(),
                    Progress = totalResources > 0 ? (double)completedResources / totalResources : 0
                };
            });
        }

        public async Task<LearningPathResponseDto?> GetPathByIdAsync(int pathId)
        {
            var path = await _context.LearningPaths
                .Include(p => p.PathItems)
                    .ThenInclude(pi => pi.Resources) // Eagerly load all levels
                .FirstOrDefaultAsync(p => p.Id == pathId);

            if (path == null)
            {
                return null;
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
                        Order = pi.Order,
                        IsCompleted = pi.IsCompleted,
                        Resources = pi.Resources.Select(r => new ResourceDto
                        {
                            Id = r.Id,
                            Title = r.Title,
                            Url = r.Url,
                            Type = r.Type.ToString(),
                            IsCompleted = r.IsCompleted,
                        }).ToList()
                    }).ToList()
            };

            return responseDto;
        }

        public async Task<LearningPathResponseDto> CreateLearningPathAsync(string prompt, string userAuth0Id, string? userName, string? userEmail)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Auth0Id == userAuth0Id);
            if (user == null)
            {
                // Correctly create the user with data passed from the controller
                user = new User
                {
                    Auth0Id = userAuth0Id,
                    Email = userEmail ?? "Not provided",
                    Name = userName ?? "Not provided"
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var geminiResponse = await GetLearningPathFromGemini(prompt);

            if (!string.IsNullOrEmpty(geminiResponse.Error))
            {
                throw new InvalidOperationException(geminiResponse.Error);
            }

            var pathItems = new List<PathItem>();
            int order = 1;

            foreach (var itemDto in geminiResponse.Items ?? new List<GeminiPathItemDto>())
            {
                var pathItem = new PathItem
                {
                    Title = itemDto.Title,
                    Order = order++,
                    Resources = new List<Resource>()
                };

                foreach (var resourceDto in itemDto.Resources ?? new List<GeminiResourceDto>())
                {
                    var resource = new Resource
                    {
                        Title = resourceDto.Title,
                        Type = Enum.Parse<ItemType>(resourceDto.Type, true),
                        Url = await SearchForUrlAsync(resourceDto.SearchQuery)
                    };
                    pathItem.Resources.Add(resource);
                }

                pathItems.Add(pathItem);
            }

            var newLearningPath = new LearningPath
            {
                Title = geminiResponse.Title,
                Description = geminiResponse.Description,
                GeneratedFromPrompt = prompt,
                UserId = user.Id,
                Category = Enum.Parse<PathCategory>(geminiResponse.Category, true),
                PathItems = pathItems,
                CreatedAt = DateTime.UtcNow
            };

            _context.LearningPaths.Add(newLearningPath);
            await _context.SaveChangesAsync();

            // Correctly map the newly created entity to the response DTO
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
                    Order = pi.Order,
                    IsCompleted = pi.IsCompleted,
                    Resources = pi.Resources.Select(r => new ResourceDto
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Url = r.Url
                    }).ToList()
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
                return null;
            }

            var newItemsFromGemini = await GetNextPathItemsFromGemini(existingPath);

            if (newItemsFromGemini == null || !newItemsFromGemini.Any())
            {
                return new List<PathItemResponseDto>();
            }

            var highestOrder = existingPath.PathItems.Any() ? existingPath.PathItems.Max(pi => pi.Order) : 0;
            var newPathItems = new List<PathItem>();

            foreach (var itemDto in newItemsFromGemini)
            {
                var pathItem = new PathItem
                {
                    Title = itemDto.Title,
                    Order = highestOrder + newPathItems.Count + 1,
                    LearningPathId = pathId,
                    Resources = new List<Resource>()
                };

                foreach (var resourceDto in itemDto.Resources ?? new List<GeminiResourceDto>())
                {
                    var resource = new Resource
                    {
                        Title = resourceDto.Title,
                        Type = Enum.Parse<ItemType>(resourceDto.Type, true),
                        Url = await SearchForUrlAsync(resourceDto.SearchQuery)
                    };
                    pathItem.Resources.Add(resource);
                }
                newPathItems.Add(pathItem);
            }

            _context.PathItems.AddRange(newPathItems);
            await _context.SaveChangesAsync();

            return newPathItems.Select(pi => new PathItemResponseDto
            {
                Id = pi.Id,
                Title = pi.Title,
                Order = pi.Order,
                IsCompleted = pi.IsCompleted,
                Resources = pi.Resources.Select(r => new ResourceDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Url = r.Url,
                    Type = r.Type.ToString()
                }).ToList()
            }).ToList();
        }

        public async Task<PathItemResponseDto> TogglePathItemCompletionAsync(int itemId)
        {
            var pathItem = await _context.PathItems
                .Include(pi => pi.Resources)
                .FirstOrDefaultAsync(pi => pi.Id == itemId);

            if (pathItem == null) return null;

            // Flip the completion status of the parent item
            pathItem.IsCompleted = !pathItem.IsCompleted;

            // Set all child resources to match the parent's new status
            foreach (var resource in pathItem.Resources)
            {
                resource.IsCompleted = pathItem.IsCompleted;
            }

            await _context.SaveChangesAsync();

            // Map and return the updated DTO
            return new PathItemResponseDto
            {
                Id = pathItem.Id,
                Title = pathItem.Title,
                Order = pathItem.Order,
                IsCompleted = pathItem.IsCompleted,
                Resources = pathItem.Resources.Select(r => new ResourceDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Url = r.Url,
                    Type = r.Type.ToString(),
                    IsCompleted = r.IsCompleted
                }).ToList()
            };
        }

        public async Task<ResourceDto> ToggleResourceCompletionAsync(int resourceId)
        {
            var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == resourceId);
            if (resource == null) return null;

            // Flip the completion status of the single resource
            resource.IsCompleted = !resource.IsCompleted;

            // After updating the resource, check the status of the parent PathItem
            var pathItem = await _context.PathItems
                .Include(pi => pi.Resources)
                .FirstOrDefaultAsync(pi => pi.Id == resource.PathItemId);

            if (pathItem != null)
            {
                // If all resources are now complete, mark the parent item as complete.
                // Otherwise, mark it as incomplete.
                pathItem.IsCompleted = pathItem.Resources.All(r => r.IsCompleted);
            }

            await _context.SaveChangesAsync();

            return new ResourceDto
            {
                Id = resource.Id,
                Title = resource.Title,
                Url = resource.Url,
                Type = resource.Type.ToString(),
                IsCompleted = resource.IsCompleted
            };
        }

        public async Task<bool> DeletePathAsync(int pathId)
        {
            var pathToDelete = await _context.LearningPaths.FindAsync(pathId);

            if (pathToDelete == null)
            {
                return false; // Path not found
            }

            // EF Core's cascading delete will handle associated PathItems and Resources
            _context.LearningPaths.Remove(pathToDelete);
            await _context.SaveChangesAsync();

            return true; // Deletion successful
        }

        public async Task DeleteAllUserPathsAsync(string userAuth0Id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Auth0Id == userAuth0Id);
            if (user != null)
            {
                // Find all paths for the user to ensure we only delete their data.
                var pathsToDelete = _context.LearningPaths
                    .Where(p => p.UserId == user.Id);

                if (await pathsToDelete.AnyAsync())
                {
                    // By removing the LearningPath entities, EF Core's cascading delete feature
                    // will automatically remove all associated PathItems and their Resources,
                    // ensuring a clean deletion.
                    _context.LearningPaths.RemoveRange(pathsToDelete);
                    await _context.SaveChangesAsync();
                }
            }
            // If user is null, there's nothing to delete.
        }

        #region Private Methods

        private async Task<GeminiResponseDto> GetLearningPathFromGemini(string userPrompt)
        {
            var apiUrl = GetGeminiApiUrl();

            // Read the prompt template from the file
            var promptTemplatePath = Path.Combine(_env.ContentRootPath, "Prompts", "CreatePathPrompt.txt");
            var promptTemplate = await File.ReadAllTextAsync(promptTemplatePath);

            // Inject the user's prompt into the template
            var fullPrompt = promptTemplate.Replace("{userPrompt}", userPrompt);

            var payload = new { contents = new[] { new { parts = new[] { new { text = fullPrompt } } } } };
            return await CallGeminiApi<GeminiResponseDto>(apiUrl, payload);
        }

        private async Task<List<GeminiPathItemDto>> GetNextPathItemsFromGemini(LearningPath existingPath)
        {
            var apiUrl = GetGeminiApiUrl();
            var existingItems = string.Join(", ", existingPath.PathItems.Select(pi => $"\"{pi.Title}\""));

            // Read the prompt template from the file
            var promptTemplatePath = Path.Combine(_env.ContentRootPath, "Prompts", "ExtendPathPrompt.txt");
            var promptTemplate = await File.ReadAllTextAsync(promptTemplatePath);

            // Inject the context into the template
            var fullPrompt = promptTemplate
                .Replace("{originalPrompt}", existingPath.GeneratedFromPrompt)
                .Replace("{existingItems}", existingItems);

            var payload = new { contents = new[] { new { parts = new[] { new { text = fullPrompt } } } } };
            return await CallGeminiApi<List<GeminiPathItemDto>>(apiUrl, payload);
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

            // --- Add more checks for a valid response structure ---
            dynamic parsedJsonResponse = JsonConvert.DeserializeObject(jsonResponse);
            if (parsedJsonResponse.candidates == null || parsedJsonResponse.candidates.Count == 0)
            {
                throw new InvalidOperationException("Gemini API returned no candidates.");
            }
            string textContent = parsedJsonResponse.candidates[0].content.parts[0].text;
            // ---

            int firstBracket = textContent.IndexOf('{');
            if (firstBracket == -1) firstBracket = textContent.IndexOf('[');

            int lastBracket = textContent.LastIndexOf('}');
            if (lastBracket == -1) lastBracket = textContent.LastIndexOf(']');

            if (firstBracket == -1 || lastBracket == -1)
            {
                throw new JsonReaderException("The response from the AI service did not contain a valid JSON object or array.");
            }

            string cleanedJson = textContent.Substring(firstBracket, lastBracket - firstBracket + 1);

            // --- Add a try-catch block around the deserialization ---
            try
            {
                return JsonConvert.DeserializeObject<T>(cleanedJson);
            }
            catch (JsonSerializationException ex)
            {
                // If deserialization fails, throw a new exception with more context.
                // Logging the 'cleanedJson' here is extremely useful for debugging.
                Console.WriteLine($"Failed to deserialize JSON: {cleanedJson}");
                throw new JsonSerializationException($"Failed to deserialize the AI's response. Details: {ex.Message}", ex);
            }
        }

        private async Task<string?> SearchForUrlAsync(string query)
        {
            var apiKey = _configuration["GoogleSearch:ApiKey"];
            var searchEngineId = _configuration["GoogleSearch:SearchEngineId"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(searchEngineId))
            {
                // Fallback to a simple Google search if API keys are not configured
                return $"https://www.google.com/search?q={HttpUtility.UrlEncode(query)}";
            }

            var apiUrl = $"https://www.googleapis.com/customsearch/v1?key={apiKey}&cx={searchEngineId}&q={HttpUtility.UrlEncode(query)}";

            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return null; // Or log the error
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var searchResult = JsonConvert.DeserializeObject<GoogleSearchResponseDto>(jsonResponse);

                // Return the link of the first search result, or null if no results
                return searchResult?.Items?.FirstOrDefault()?.Link;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error during search: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}