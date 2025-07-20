using LearningAppNetCoreApi.Dtos;
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

        public async Task<IEnumerable<MyPathSummaryDto>> GetUserPathsAsync(string firebaseUid)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return new List<MyPathSummaryDto>();

            var userPaths = await _context.UserPaths
                .Where(up => up.UserId == user.Id)
                .Include(up => up.PathTemplate)
                    .ThenInclude(pt => pt.PathItems)
                    .ThenInclude(pit => pit.Resources)
                .OrderByDescending(up => up.StartedAt)
                .ToListAsync();

            var resourceProgress = await _context.UserResourceProgress
                .Where(urp => urp.UserId == user.Id)
                .ToDictionaryAsync(urp => urp.ResourceTemplateId);

            return userPaths.Select(up =>
            {
                var allResources = up.PathTemplate.PathItems.SelectMany(pi => pi.Resources).ToList();
                var completedResources = allResources.Count(r => resourceProgress.ContainsKey(r.Id) && resourceProgress[r.Id].IsCompleted);
                var totalResources = allResources.Count;

                return new MyPathSummaryDto
                {
                    UserPathId = up.Id,
                    Title = up.PathTemplate.Title,
                    Description = up.PathTemplate.Description,
                    Category = up.PathTemplate.Category.ToString(),
                    Progress = totalResources > 0 ? (double)completedResources / totalResources : 0
                };
            });
        }

        public async Task<LearningPathResponseDto> GetPathByIdAsync(int userPathId, string firebaseUid)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return null;

            var userPath = await _context.UserPaths
                .AsNoTracking()
                .Include(up => up.PathTemplate)
                    .ThenInclude(pt => pt.PathItems)
                    .ThenInclude(pit => pit.Resources)
                .FirstOrDefaultAsync(up => up.Id == userPathId && up.UserId == user.Id);

            if (userPath == null) return null;

            var resourceProgress = await _context.UserResourceProgress
                .Where(urp => urp.UserId == user.Id)
                .ToDictionaryAsync(urp => urp.ResourceTemplateId);

            return new LearningPathResponseDto
            {
                UserPathId = userPath.Id,
                Title = userPath.PathTemplate.Title,
                Description = userPath.PathTemplate.Description,
                CreatedAt = userPath.StartedAt,
                PathItems = userPath.PathTemplate.PathItems
                    .OrderBy(pi => pi.Order)
                    .Select(pi => new PathItemResponseDto
                    {
                        Id = pi.Id,
                        Title = pi.Title,
                        Order = pi.Order,
                        IsCompleted = pi.Resources.Any() && pi.Resources.All(r => resourceProgress.ContainsKey(r.Id) && resourceProgress[r.Id].IsCompleted),
                        Resources = pi.Resources.Select(r => new ResourceResponseDto
                        {
                            Id = r.Id,
                            Title = r.Title,
                            Url = r.Url,
                            Type = r.Type.ToString(),
                            IsCompleted = resourceProgress.ContainsKey(r.Id) && resourceProgress[r.Id].IsCompleted
                        }).ToList()
                    }).ToList()
            };
        }

        public async Task<IEnumerable<PathTemplateSummaryDto>> FindSimilarPathsAsync(string prompt)
        {
            // A more comprehensive list of common words to ignore in the search.
            var stopWords = new HashSet<string> {
                "a", "an", "the", "in", "on", "of", "for", "to", "with", "by",
                "is", "are", "was", "were", "be", "been", "being",
                "i", "you", "he", "she", "it", "we", "they", "me", "us",
                "my", "your", "his", "her", "its", "our", "their",
                "what", "which", "who", "whom", "this", "that", "these", "those",
                "how", "to", "learn", "about", "more", "some", "want", "need", "like"
            };

            // Sanitize and extract significant keywords from the user's prompt.
            var keywords = prompt.ToLower()
                .Split(new[] { ' ', '\'', 's' }, StringSplitOptions.RemoveEmptyEntries) // Split by space and apostrophes
                .Where(kw => !stopWords.Contains(kw))
                .ToList();

            if (!keywords.Any())
            {
                // If the prompt only contained stop words, return an empty list.
                return new List<PathTemplateSummaryDto>();
            }

            var query = _context.PathTemplates.AsQueryable();

            // Build a query that requires ALL keywords to be present in the prompt.
            foreach (var keyword in keywords)
            {
                query = query.Where(p => p.GeneratedFromPrompt.ToLower().Contains(keyword));
            }

            var similarPaths = await query
                .Include(p => p.PathItems) // Include PathItems to get the count
                .AsNoTracking()
                .ToListAsync();

            // Group the found paths by title, and for each group, select the one
            // with the most path items. This ensures we only suggest the most
            // comprehensive version of each path.
            var distinctPaths = similarPaths
                .GroupBy(p => p.Title)
                .Select(group => group.OrderByDescending(p => p.PathItems.Count).First())
                .Take(5) // Take the top 5 distinct paths
                .ToList();

            // Map the final, distinct list to the DTO.
            return distinctPaths.Select(p => new PathTemplateSummaryDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Category = p.Category.ToString()
            });
        }
        public async Task<LearningPathResponseDto> AssignPathToUserAsync(int pathTemplateId, string firebaseUid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) throw new InvalidOperationException("User not found.");

            var existingUserPath = await _context.UserPaths
                .FirstOrDefaultAsync(up => up.UserId == user.Id && up.PathTemplateId == pathTemplateId);

            if (existingUserPath != null)
            {
                return await GetPathByIdAsync(existingUserPath.Id, firebaseUid);
            }

            var newUserPath = new UserPath
            {
                UserId = user.Id,
                PathTemplateId = pathTemplateId
            };

            _context.UserPaths.Add(newUserPath);
            await _context.SaveChangesAsync();

            var pathTemplate = await _context.PathTemplates
                .AsNoTracking()
                .Include(pt => pt.PathItems)
                .ThenInclude(pit => pit.Resources)
                .FirstAsync(pt => pt.Id == pathTemplateId);

            return new LearningPathResponseDto
            {
                UserPathId = newUserPath.Id,
                Title = pathTemplate.Title,
                Description = pathTemplate.Description,
                CreatedAt = newUserPath.StartedAt,
                PathItems = pathTemplate.PathItems
                    .OrderBy(pi => pi.Order)
                    .Select(pi => new PathItemResponseDto
                    {
                        Id = pi.Id,
                        Title = pi.Title,
                        Order = pi.Order,
                        IsCompleted = false, // Always false for a new path
                        Resources = pi.Resources.Select(r => new ResourceResponseDto
                        {
                            Id = r.Id,
                            Title = r.Title,
                            Url = r.Url,
                            Type = r.Type.ToString(),
                            IsCompleted = false
                        }).ToList()
                    }).ToList()
            };
        }

        public async Task<LearningPathResponseDto> GenerateNewPathAsync(string prompt, string firebaseUid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) throw new InvalidOperationException("User not found.");

            var geminiResponse = await GetLearningPathFromGemini(prompt);

            if (!string.IsNullOrEmpty(geminiResponse.Error))
            {
                throw new InvalidOperationException(geminiResponse.Error);
            }

            var newPathTemplate = new PathTemplate
            {
                Title = geminiResponse.Title,
                Description = geminiResponse.Description,
                GeneratedFromPrompt = prompt,
                Category = Enum.Parse<PathCategory>(geminiResponse.Category, true),
                PathItems = new List<PathItemTemplate>()
            };

            int order = 1;
            foreach (var itemDto in geminiResponse.Items ?? new List<GeminiPathItemDto>())
            {
                var pathItemTemplate = new PathItemTemplate
                {
                    Title = itemDto.Title,
                    Order = order++,
                    Resources = new List<ResourceTemplate>()
                };

                foreach (var resourceDto in itemDto.Resources ?? new List<GeminiResourceDto>())
                {
                    if (string.IsNullOrEmpty(resourceDto.SearchQuery)) continue;

                    var resourceType = Enum.Parse<ItemType>(resourceDto.Type, true);
                    string? resourceUrl = null;

                    // Prioritize the search query's intent over the AI's declared type.
                    if (resourceDto.SearchQuery.ToLower().Contains("youtube") ||
                        resourceDto.SearchQuery.ToLower().Contains("video") ||
                        resourceType == ItemType.Video)
                    {
                        resourceUrl = await SearchForYouTubeVideoAsync(resourceDto.SearchQuery);
                        // Correct the type if the AI made a mistake.
                        resourceType = ItemType.Video;
                    }
                    else
                    {
                        resourceUrl = await SearchForUrlAsync(resourceDto.SearchQuery);
                    }

                    if (!string.IsNullOrEmpty(resourceUrl))
                    {
                        pathItemTemplate.Resources.Add(new ResourceTemplate
                        {
                            Title = resourceDto.Title,
                            Type = resourceType,
                            Url = resourceUrl
                        });
                    }
                }
                newPathTemplate.PathItems.Add(pathItemTemplate);
            }

            _context.PathTemplates.Add(newPathTemplate);
            await _context.SaveChangesAsync();

            return await AssignPathToUserAsync(newPathTemplate.Id, firebaseUid);
        }

        public async Task<List<PathItemResponseDto>> ExtendLearningPathAsync(int userPathId, string firebaseUid)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return null;

            var userPath = await _context.UserPaths
                .Include(up => up.PathTemplate)
                    .ThenInclude(pt => pt.PathItems)
                    .ThenInclude(pit => pit.Resources)
                .FirstOrDefaultAsync(up => up.Id == userPathId && up.UserId == user.Id);

            if (userPath == null) return null;

            var newItemsFromGemini = await GetNextPathItemsFromGemini(userPath.PathTemplate);
            if (newItemsFromGemini == null || !newItemsFromGemini.Any()) return new List<PathItemResponseDto>();

            // 1. Create a new PathTemplate by copying the original
            var forkedPathTemplate = new PathTemplate
            {
                Title = userPath.PathTemplate.Title,
                Description = userPath.PathTemplate.Description,
                GeneratedFromPrompt = userPath.PathTemplate.GeneratedFromPrompt,
                Category = userPath.PathTemplate.Category,
                CreatedAt = DateTime.UtcNow,
                // Create a deep copy of the existing items and resources
                PathItems = userPath.PathTemplate.PathItems.Select(pi => new PathItemTemplate
                {
                    Title = pi.Title,
                    Order = pi.Order,
                    Resources = pi.Resources.Select(r => new ResourceTemplate
                    {
                        Title = r.Title,
                        Url = r.Url,
                        Type = r.Type
                    }).ToList()
                }).ToList()
            };

            // 2. Add the new items from the AI to the forked template
            var highestOrder = forkedPathTemplate.PathItems.Any() ? forkedPathTemplate.PathItems.Max(pi => pi.Order) : 0;
            var newPathItemTemplates = new List<PathItemTemplate>();

            foreach (var itemDto in newItemsFromGemini)
            {
                var pathItemTemplate = new PathItemTemplate
                {
                    Title = itemDto.Title,
                    Order = highestOrder + newPathItemTemplates.Count + 1,
                    Resources = new List<ResourceTemplate>()
                };

                foreach (var resourceDto in itemDto.Resources ?? new List<GeminiResourceDto>())
                {
                    var resourceType = Enum.Parse<ItemType>(resourceDto.Type, true);
                    string? resourceUrl = (resourceType == ItemType.Video)
                        ? await SearchForYouTubeVideoAsync(resourceDto.SearchQuery)
                        : await SearchForUrlAsync(resourceDto.SearchQuery);

                    if (!string.IsNullOrEmpty(resourceUrl))
                    {
                        pathItemTemplate.Resources.Add(new ResourceTemplate
                        {
                            Title = resourceDto.Title,
                            Type = resourceType,
                            Url = resourceUrl
                        });
                    }
                }
                newPathItemTemplates.Add(pathItemTemplate);
            }

            // Add the newly generated items to the collection
            foreach (var item in newPathItemTemplates)
            {
                forkedPathTemplate.PathItems.Add(item);
            }

            // 3. Save the new forked template to the database
            _context.PathTemplates.Add(forkedPathTemplate);
            await _context.SaveChangesAsync();

            // 4. Update the user's UserPath to point to the new forked template
            userPath.PathTemplateId = forkedPathTemplate.Id;
            await _context.SaveChangesAsync();

            // 5. Map and return only the newly added items to the frontend
            return newPathItemTemplates.Select(pi => new PathItemResponseDto
            {
                Id = pi.Id,
                Title = pi.Title,
                Order = pi.Order,
                IsCompleted = false,
                Resources = pi.Resources.Select(r => new ResourceResponseDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Url = r.Url,
                    Type = r.Type.ToString(),
                    IsCompleted = false
                }).ToList()
            }).ToList();
        }

        public async Task<PathItemResponseDto> TogglePathItemCompletionAsync(int pathItemTemplateId, string firebaseUid)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return null;

            var pathItemTemplate = await _context.PathItemTemplates
                .Include(pit => pit.Resources)
                .FirstOrDefaultAsync(pit => pit.Id == pathItemTemplateId);

            if (pathItemTemplate == null) return null;

            var resourceIds = pathItemTemplate.Resources.Select(r => r.Id).ToList();
            var progressEntries = await _context.UserResourceProgress
                .Where(urp => urp.UserId == user.Id && resourceIds.Contains(urp.ResourceTemplateId))
                .ToListAsync();

            bool areAllComplete = progressEntries.Count == resourceIds.Count && progressEntries.All(p => p.IsCompleted);
            bool newStatus = !areAllComplete;

            foreach (var resourceTemplate in pathItemTemplate.Resources)
            {
                var progress = progressEntries.FirstOrDefault(p => p.ResourceTemplateId == resourceTemplate.Id);
                if (progress != null)
                {
                    progress.IsCompleted = newStatus;
                }
                else
                {
                    _context.UserResourceProgress.Add(new UserResourceProgress
                    {
                        UserId = user.Id,
                        ResourceTemplateId = resourceTemplate.Id,
                        IsCompleted = newStatus
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Map and return the updated DTO with the correct properties
            return new PathItemResponseDto
            {
                Id = pathItemTemplate.Id,
                Title = pathItemTemplate.Title,
                Order = pathItemTemplate.Order,
                IsCompleted = newStatus, // Use the newly calculated status
                Resources = pathItemTemplate.Resources.Select(r => new ResourceResponseDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Url = r.Url,
                    Type = r.Type.ToString(),
                    IsCompleted = newStatus // All resources now share the new status
                }).ToList()
            };
        }

        public async Task<ResourceResponseDto> ToggleResourceCompletionAsync(int resourceTemplateId, string firebaseUid)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return null;

            var progress = await _context.UserResourceProgress
                .FirstOrDefaultAsync(urp => urp.UserId == user.Id && urp.ResourceTemplateId == resourceTemplateId);

            if (progress != null)
            {
                progress.IsCompleted = !progress.IsCompleted;
            }
            else
            {
                progress = new UserResourceProgress
                {
                    UserId = user.Id,
                    ResourceTemplateId = resourceTemplateId,
                    IsCompleted = true
                };
                _context.UserResourceProgress.Add(progress);
            }

            await _context.SaveChangesAsync();

            var resourceTemplate = await _context.ResourceTemplates.AsNoTracking().FirstAsync(rt => rt.Id == resourceTemplateId);

            return new ResourceResponseDto
            {
                Id = resourceTemplate.Id,
                Title = resourceTemplate.Title,
                Url = resourceTemplate.Url,
                Type = resourceTemplate.Type.ToString(),
                IsCompleted = progress.IsCompleted
            };
        }

        public async Task<bool> DeletePathAsync(int userPathId, string firebaseUid)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null) return false;

            var userPathToDelete = await _context.UserPaths
                .FirstOrDefaultAsync(up => up.Id == userPathId && up.UserId == user.Id);

            if (userPathToDelete == null)
            {
                return false; // Path not found or doesn't belong to the user
            }

            // Also delete all associated progress for this user and this path template
            var resourceIdsToDelete = await _context.ResourceTemplates
                .Where(rt => rt.PathItemTemplate.PathTemplateId == userPathToDelete.PathTemplateId)
                .Select(rt => rt.Id)
                .ToListAsync();

            var progressToDelete = _context.UserResourceProgress
                .Where(urp => urp.UserId == user.Id && resourceIdsToDelete.Contains(urp.ResourceTemplateId));

            _context.UserResourceProgress.RemoveRange(progressToDelete);
            _context.UserPaths.Remove(userPathToDelete);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task DeleteAllUserPathsAsync(string firebaseUid)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
            if (user == null)
            {
                // If user doesn't exist, there's nothing to delete.
                return;
            }

            // Find all of the user's path instances
            var userPathsToDelete = _context.UserPaths.Where(up => up.UserId == user.Id);

            // Find all of the user's resource progress entries
            var progressToDelete = _context.UserResourceProgress.Where(urp => urp.UserId == user.Id);

            if (await userPathsToDelete.AnyAsync())
            {
                _context.UserPaths.RemoveRange(userPathsToDelete);
            }

            if (await progressToDelete.AnyAsync())
            {
                _context.UserResourceProgress.RemoveRange(progressToDelete);
            }

            await _context.SaveChangesAsync();
        }

        #region Private Methods

        private async Task<GeminiResponseDto> GetLearningPathFromGemini(string userPrompt)
        {
            var apiUrl = GetGeminiApiUrl();
            var promptTemplatePath = Path.Combine(_env.ContentRootPath, "Prompts", "CreatePathPrompt.txt");
            var promptTemplate = await File.ReadAllTextAsync(promptTemplatePath);
            var fullPrompt = promptTemplate.Replace("{userPrompt}", userPrompt);

            var payload = new
            {
                contents = new[] { new { parts = new[] { new { text = fullPrompt } } } },
                tools = new[] { new { google_search = new { } } }
            };

            // We go back to the simpler CallGeminiApi that cleans the JSON from the 'text' field
            return await CallAndParseGeminiObjectAsync<GeminiResponseDto>(apiUrl, payload);
        }

        private async Task<List<GeminiPathItemDto>> GetNextPathItemsFromGemini(PathTemplate existingPath)
        {
            var apiUrl = GetGeminiApiUrl();
            var existingItems = string.Join(", ", existingPath.PathItems.Select(pi => $"\"{pi.Title}\""));

            // Read the prompt template from the file
            var promptTemplatePath = Path.Combine(_env.ContentRootPath, "Prompts", "ExtendPathPrompt.txt");
            var promptTemplate = await File.ReadAllTextAsync(promptTemplatePath);

            // Inject the context into the template
            var fullPrompt = promptTemplate
                .Replace("{pathTitle}", existingPath.Title)
                .Replace("{pathDescription}", existingPath.Description)
                .Replace("{existingItems}", existingItems);

            var payload = new { contents = new[] { new { parts = new[] { new { text = fullPrompt } } } } };
            return await CallAndParseGeminiListAsync<GeminiPathItemDto>(apiUrl, payload);
        }

        private string GetGeminiApiUrl()
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"];
            return $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
        }

        // Helper for API calls that expect a single JSON object
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

        // Helper for API calls that expect a JSON array (or a list of objects)
        private async Task<List<T>> CallAndParseGeminiListAsync<T>(string apiUrl, object payload)
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

            // Correctly find the start and end of the JSON array
            int firstBracket = textContent.IndexOf('[');
            int lastBracket = textContent.LastIndexOf(']');
            if (firstBracket == -1 || lastBracket == -1)
            {
                // Fallback for comma-separated objects without brackets
                firstBracket = textContent.IndexOf('{');
                lastBracket = textContent.LastIndexOf('}');
                if (firstBracket == -1 || lastBracket == -1)
                {
                    throw new JsonReaderException("The response from the AI service did not contain a valid JSON array.");
                }
                textContent = $"[{textContent}]"; // Wrap it in brackets
            }

            string cleanedJson = textContent.Substring(firstBracket, lastBracket - firstBracket + 1);

            try
            {
                return JsonConvert.DeserializeObject<List<T>>(cleanedJson);
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine($"Failed to deserialize JSON list: {cleanedJson}");
                throw new JsonSerializationException($"Failed to deserialize the AI's response. Details: {ex.Message}", ex);
            }
        }

        private async Task<string?> SearchForUrlAsync(string query)
        {
            var apiKey = _configuration["GoogleSearch:ApiKey"];
            var searchEngineId = _configuration["GoogleSearch:SearchEngineId"];
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(searchEngineId))
            {
                return $"https://www.google.com/search?q={HttpUtility.UrlEncode(query)}";
            }

            var apiUrl = $"https://www.googleapis.com/customsearch/v1?key={apiKey}&cx={searchEngineId}&q={HttpUtility.UrlEncode(query)}";
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode) return null;

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var searchResult = JsonConvert.DeserializeObject<GoogleSearchResponseDto>(jsonResponse);

                return searchResult?.Items?.FirstOrDefault()?.Link;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during Google search: {ex.Message}");
                return null;
            }
        }

        private async Task<string?> SearchForYouTubeVideoAsync(string query)
        {
            var apiKey = _configuration["YouTube:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return $"https://www.youtube.com/results?search_query={HttpUtility.UrlEncode(query)}";
            }

            var apiUrl = $"https://www.googleapis.com/youtube/v3/search?part=snippet&q={HttpUtility.UrlEncode(query)}&type=video&maxResults=1&key={apiKey}";
            var httpClient = _httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode) return null;

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var youtubeResult = JsonConvert.DeserializeObject<YouTubeSearchResponseDto>(jsonResponse);

                string? videoId = youtubeResult?.Items?.FirstOrDefault()?.Id?.VideoId;

                return string.IsNullOrEmpty(videoId) ? null : $"https://www.youtube.com/watch?v={videoId}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during YouTube search: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}