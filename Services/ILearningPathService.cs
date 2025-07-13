using LearningAppNetCoreApi.DTOs;
using LearningAppNetCoreApi.Models;

namespace LearningAppNetCoreApi.Services
{
    public interface ILearningPathService
    {
        Task<LearningPathResponseDto> CreateLearningPathAsync(string prompt, string userAuth0Id);
        Task<List<PathItemResponseDto>> ExtendLearningPathAsync(int pathId);
        Task<IEnumerable<MyPathSummaryDto>> GetUserPathsAsync(string userAuth0Id);
        Task<LearningPathResponseDto> GetPathByIdAsync(int pathId);
        Task<PathItemResponseDto> TogglePathItemCompletionAsync(int itemId);
    }
}