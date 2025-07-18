using LearningAppNetCoreApi.DTOs;
using LearningAppNetCoreApi.Models;

namespace LearningAppNetCoreApi.Services
{
    public interface ILearningPathService
    {
        Task<LearningPathResponseDto> GenerateNewPathAsync(string prompt, string userAuth0Id);
        Task<IEnumerable<MyPathSummaryDto>> FindSimilarPathsAsync(string prompt);
        Task<LearningPathResponseDto> AssignPathToUserAsync(int pathId, string userAuth0Id);
        Task<List<PathItemResponseDto>> ExtendLearningPathAsync(int pathId);
        Task<IEnumerable<MyPathSummaryDto>> GetUserPathsAsync(string firebaseUid);
        Task<LearningPathResponseDto?> GetPathByIdAsync(int pathId);
        Task<PathItemResponseDto> TogglePathItemCompletionAsync(int itemId);
        Task<ResourceDto> ToggleResourceCompletionAsync(int resourceId);
        Task DeleteAllUserPathsAsync(string firebaseUid);
        Task<bool> DeletePathAsync(int pathId);
    }
}