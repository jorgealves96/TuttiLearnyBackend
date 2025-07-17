using LearningAppNetCoreApi.DTOs;

namespace LearningAppNetCoreApi.Services
{
    public interface IProfileService
    {
        Task<ProfileStatsDto> GetUserStatsAsync(string userAuth0Id);
    }
}
