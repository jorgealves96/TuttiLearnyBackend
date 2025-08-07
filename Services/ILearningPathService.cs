using LearningAppNetCoreApi.Dtos;
using LearningAppNetCoreApi.Models;

namespace LearningAppNetCoreApi.Services
{
    public interface ILearningPathService
    {
        Task<LearningPathResponseDto> GenerateNewPathAsync(string prompt, string firebaseUid);
        Task<IEnumerable<PathTemplateSummaryDto>> FindSimilarPathsAsync(string prompt, string firebaseUid);
        Task<LearningPathResponseDto> AssignPathToUserAsync(int pathTemplateId, string firebaseUid);
        Task<List<PathItemResponseDto>> ExtendLearningPathAsync(int userPathId, string firebaseUid);
        Task<IEnumerable<MyPathSummaryDto>> GetUserPathsAsync(string firebaseUid);
        Task<LearningPathResponseDto> GetPathByIdAsync(int userPathId, string firebaseUid);
        Task<PathReportDto?> GetUserReportForPathAsync(int pathTemplateId, string firebaseUid);
        Task<bool> CreatePathReportAsync(int pathTemplateId, string firebaseUid, ReportType reportType, string? description);
        Task<bool> AcknowledgeReportAsync(int pathTemplateId, string firebaseUid);
        Task<PathItemResponseDto> TogglePathItemCompletionAsync(int pathItemTemplateId, string firebaseUid);
        Task<ResourceResponseDto> ToggleResourceCompletionAsync(int resourceTemplateId, string firebaseUid);
        Task<bool> RatePathAsync(int pathTemplateId, string firebaseUid, int rating);
        Task<bool> DeletePathAsync(int userPathId, string firebaseUid);
        Task DeleteAllUserPathsAsync(string firebaseUid);
    }
}